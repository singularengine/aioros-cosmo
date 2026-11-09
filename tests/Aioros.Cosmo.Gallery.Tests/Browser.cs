using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

[assembly: AssemblyFixture(typeof(Aioros.Cosmo.Gallery.Tests.Browser))]

namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>
/// The Chromium the browser tests drive (the path in <c>CHROMIUM</c>, <c>/usr/bin/chromium</c> by default, as CI installs
/// it), started once, with the sRGB color profile the reference captures were taken with.
/// </summary>
public sealed class Browser : IAsyncLifetime
{
    public const string Probe = """
        (() => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          const pick = (el) => {
            const s = getComputedStyle(el), r = el.getBoundingClientRect()
            return { tag: el.tagName.toLowerCase(), text: el.textContent.trim(), x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height,
              background: s.backgroundColor, color: s.color, borderColor: s.borderTopColor, borderWidth: s.borderTopWidth, radius: s.borderTopLeftRadius,
              shadow: s.boxShadow, fontSize: s.fontSize, fontWeight: s.fontWeight, opacity: s.opacity, decoration: s.textDecorationLine }
          }
          const fixture = document.getElementById("fixture")
          const rounded = (el) => parseFloat(getComputedStyle(el).borderTopLeftRadius) >= 9999
          return {
            buttons: [...fixture.querySelectorAll("button, a")].map(pick),
            badges: [...fixture.querySelectorAll("div")].filter(rounded).map((el) => {
              const leaf = [...el.querySelectorAll("*")].find((child) => child.children.length === 0 && child.textContent.trim().length > 0) ?? el
              return { ...pick(el), color: getComputedStyle(leaf).color }
            }),
            cards: [...fixture.querySelectorAll("div")].filter((el) => getComputedStyle(el).borderTopLeftRadius === "14px").map(pick),
          }
        })()
        """;

    /// <summary>What <see cref="PickAsync"/> reads from each element: its box relative to the fixture and its computed styles.</summary>
    public const string Pick = """
        (selector) => {
          const root = (document.getElementById("fixture") ?? document.body).getBoundingClientRect()
          return [...document.querySelectorAll(selector)].map((el) => {
            const s = getComputedStyle(el), r = el.getBoundingClientRect()
            return { tag: el.tagName.toLowerCase(), text: el.textContent.trim(), x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height,
              background: s.backgroundColor, color: s.color, borderColor: s.borderTopColor, borderWidth: s.borderTopWidth, radius: s.borderTopLeftRadius,
              shadow: s.boxShadow, fontSize: s.fontSize, fontWeight: s.fontWeight, opacity: s.opacity, decoration: s.textDecorationLine,
              paddingLeft: s.paddingLeft, paddingTop: s.paddingTop, slot: "", lineHeight: s.lineHeight }
          })
        }
        """;

    private static readonly System.Text.Json.JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private IPlaywright? _playwright;

    public static string Chromium { get; } = Environment.GetEnvironmentVariable("CHROMIUM") is { Length: > 0 } path ? path : "/usr/bin/chromium";

    public IBrowser Instance { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        Assert.True(File.Exists(Chromium), $"No Chromium at {Chromium}: install it or set CHROMIUM to its executable.");
        _playwright = await Playwright.CreateAsync();
        Instance = await _playwright.Chromium.LaunchAsync(new() { ExecutablePath = Chromium, Args = ["--no-sandbox", "--force-color-profile=srgb", "--disable-lcd-text"] });
    }

    public async ValueTask DisposeAsync()
    {
        await Instance.DisposeAsync();
        _playwright?.Dispose();
    }

    /// <summary>A page of a new context in <paramref name="theme"/> (the Gallery's theme cookie), at a viewport width.</summary>
    public async Task<IPage> PageAsync(Uri gallery, string theme, int width = 1000, int height = 900)
    {
        var context = await Instance.NewContextAsync(new() { ViewportSize = new() { Width = width, Height = height }, DeviceScaleFactor = 1 });
        await context.AddCookiesAsync([new Cookie { Name = "theme", Value = theme, Url = gallery.ToString() }]);
        return await context.NewPageAsync();
    }

    /// <summary>Reads a fixture page's elements with <see cref="Probe"/>.</summary>
    internal static async Task<FixtureElements> ReadAsync(IPage page)
    {
        var json = await page.EvaluateAsync<System.Text.Json.JsonElement>(Probe);
        return System.Text.Json.JsonSerializer.Deserialize<FixtureElements>(json, Options) ?? throw new InvalidOperationException("The probe read nothing.");
    }

    /// <summary>Reads the elements a CSS selector finds with <see cref="Pick"/>, in document order.</summary>
    internal static async Task<IReadOnlyList<ElementMetrics>> PickAsync(IPage page, string selector)
    {
        var json = await page.EvaluateAsync<System.Text.Json.JsonElement>(Pick, selector);
        return System.Text.Json.JsonSerializer.Deserialize<List<ElementMetrics>>(json, Options) ?? [];
    }

    /// <summary>
    /// Opens an overlay fixture's page in a theme and opens its overlay as a user does, then leaves the page standing with
    /// transitions off and nothing focused, as the reference capture does.
    /// </summary>
    internal async Task<IPage> OverlayAsync(Uri gallery, string fixture, string theme, OverlayOpening opening)
    {
        var page = opening.Viewport is [var width, var height] ? await PageAsync(gallery, theme, width, height) : await PageAsync(gallery, theme);
        await page.GotoAsync(new Uri(gallery, "fidelity/" + fixture).ToString());
        await page.EvaluateAsync("document.fonts.ready");
        await ScrollsAsync(page);
        if (opening.Viewport is not null)
        {
            await page.EvaluateAsync("Object.assign(document.getElementById('fixture').style, { position: 'fixed', left: '0', top: '0' })");
        }

        var target = page.Locator("#fixture " + opening.Selector).First;
        switch (opening.Action)
        {
            case "press":
                await KeysAsync(page);
                await page.Keyboard.PressAsync(opening.Selector);
                break;
            case "hover":
                await target.HoverAsync();
                break;
            case "contextmenu":
                await target.ClickAsync(new() { Button = MouseButton.Right });
                break;
            default:
                await target.ClickAsync();
                break;
        }

        if (opening.Wait is { } wait)
        {
            await page.WaitForSelectorAsync(wait);
            await page.EvaluateAsync("document.fonts.ready");
        }

        if (opening.Away)
        {
            var box = await page.Locator("#fixture").BoundingBoxAsync();
            await page.Mouse.MoveAsync(box!.X + 2, box.Y + box.Height - 2);
        }

        await page.AddStyleTagAsync(new() { Content = Stilled });
        await page.EvaluateAsync("document.activeElement?.blur?.()");
        return page;
    }

    /// <summary>
    /// Waits until the framework's keyboard module (and the flag module when the page switches a panel) has run in the
    /// page: the page imports them once it is parsed, and a key pressed before they have run is dropped. Importing the same
    /// address resolves once the module has run, as Aioros's own script tests wait for their module.
    /// </summary>
    public static async Task KeysAsync(IPage page, bool flags = false) =>
        await page.EvaluateAsync("us => Promise.all(us.map(u => import(u))).then(() => 0)", (string[])[Aioros.Targets.Ssr.Documents.KeyboardScript.Path, .. flags ? (string[])[Aioros.Targets.Ssr.Documents.FlagScript.Path] : []]);

    /// <summary>
    /// Transitions and animations off, but the scroll-driven ones of a scroll fade (Aioros's <c>sf</c> classes), which show
    /// its fades as the reference's capture keeps them (tools/shadcn/capture.mjs).
    /// </summary>
    public const string Stilled = "*,*::before,*::after,*::details-content{transition:none!important}:not(.sfx,.sfy,.sft,.sfb,.sfs,.sfe),::before,::after{animation:none!important}";

    /// <summary>
    /// How a fixture is captured, as the reference is: a page running scroll-driven animations stilled by hand (a finite
    /// animation finished, an endless one cancelled) with those left running; any other page with Playwright's <c>Disabled</c>.
    /// </summary>
    public static async Task<ScreenshotAnimations> StillAsync(IPage page) =>
        await page.EvaluateAsync<bool>("""
            () => {
              const scrolled = (animation) => typeof ScrollTimeline !== "undefined" && animation.timeline instanceof ScrollTimeline
              const animations = document.getAnimations()
              if (!animations.some(scrolled)) return false
              for (const animation of animations.filter((animation) => !scrolled(animation))) {
                if (animation.effect?.getComputedTiming().iterations === Infinity) animation.cancel()
                else animation.finish()
              }
              return true
            }
            """) ? ScreenshotAnimations.Allow : ScreenshotAnimations.Disabled;

    /// <summary>1 for a request a page sends on its own (a post or a document), 0 for one the framework's script makes ahead (a plan, a module).</summary>
    public static int Sends(IRequest request) => request.Method == "POST" || request.ResourceType == "document" ? 1 : 0;

    /// <summary>Waits until the framework's scroll module has run in a page holding a scroll-to-end button, so the button shows only away from its viewer's end.</summary>
    public static async Task ScrollsAsync(IPage page) =>
        await page.EvaluateAsync("u => document.querySelector('[data-aioros-scrolls]') ? import(u).then(() => 0) : 0", Aioros.Targets.Ssr.Documents.ScrollScript.Path);

    /// <summary>Waits until the page's finite animations and transitions have ended (an overlay's entrance), so it is read at rest.</summary>
    public static async Task AtRestAsync(IPage page) =>
        await page.WaitForFunctionAsync("() => document.getAnimations().filter((animation) => animation.effect?.getComputedTiming().endTime !== Infinity).every((animation) => animation.playState !== 'running')", null, new() { PollingInterval = 20 });

    /// <summary>Opens a fixture page in a theme with transitions off and its fonts loaded, in a viewport of the given height.</summary>
    public async Task<IPage> FixtureAsync(Uri gallery, string fixture, string theme, int height = 900, int width = 1000)
    {
        var page = await PageAsync(gallery, theme, width, height);
        await page.GotoAsync(new Uri(gallery, "fidelity/" + fixture).ToString());
        await page.EvaluateAsync("document.fonts.ready");
        await ScrollsAsync(page);
        await page.AddStyleTagAsync(new() { Content = Stilled });
        return page;
    }
}

/// <summary>How a user opens an overlay fixture on its Gallery page: the action, the element acted on, what shows once it is open, and whether the pointer then leaves.</summary>
internal sealed record OverlayOpening(string Action, string Selector, string? Wait, bool Away, int[]? Viewport = null);

/// <summary>What <see cref="Browser.Probe"/> reads from a fixture: its buttons and links, its badges and its cards.</summary>
internal sealed record FixtureElements(IReadOnlyList<ElementMetrics> Buttons, IReadOnlyList<ElementMetrics> Badges, IReadOnlyList<ElementMetrics> Cards);
