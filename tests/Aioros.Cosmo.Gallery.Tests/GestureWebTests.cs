using System.Globalization;
using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class GestureWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    /// <summary>A page of the Gallery on Playwright's clock, which runs until a drag pauses it.</summary>
    private async Task<IPage> OpenAsync(string path, string theme = "light")
    {
        var page = await browser.PageAsync(gallery.Address, theme, 1280);
        await page.Clock.InstallAsync();
        await page.GotoAsync(new Uri(gallery.Address, path).ToString());
        return page;
    }

    /// <summary>Presses the mouse at a point and moves it by an offset in four steps over the given time on a paused clock, runs a check, holds it, lets go and lets the clock run again.</summary>
    private static async Task<T> DragAsync<T>(IPage page, (double X, double Y) from, (double X, double Y) by, double milliseconds, Func<Task<T>> during, double hold = 0)
    {
        await PauseAsync(page);
        await page.Mouse.MoveAsync((float)from.X, (float)from.Y);
        await page.Mouse.DownAsync();
        for (var step = 1; step <= 4; step++)
        {
            await page.Clock.RunForAsync((long)(milliseconds / 4));
            await page.Mouse.MoveAsync((float)(from.X + (by.X * step / 4)), (float)(from.Y + (by.Y * step / 4)));
        }

        var seen = await during();
        if (hold > 0)
        {
            await page.Clock.RunForAsync((long)hold);
        }

        await page.Mouse.UpAsync();
        await page.Clock.ResumeAsync();
        return seen;
    }

    /// <summary>Pauses the page's clock a second ahead of its time, reading the time again when a loaded machine let the clock pass that second before the pause arrived.</summary>
    private static async Task PauseAsync(IPage page)
    {
        while (true)
        {
            var now = await page.EvaluateAsync<double>("Date.now()");
            try
            {
                await page.Clock.PauseAtAsync(DateTimeOffset.FromUnixTimeMilliseconds((long)now + 1000).UtcDateTime);
                return;
            }
            catch (PlaywrightException error) when (error.Message.Contains("Cannot fast-forward to the past", StringComparison.Ordinal))
            {
            }
        }
    }

    private static Task<string> NothingAsync() => Task.FromResult(string.Empty);

    /// <summary>Waits until the page's entrances and transitions have ended, the cards' expiries aside.</summary>
    private static async Task RestAsync(IPage page) =>
        await page.WaitForFunctionAsync("() => document.getAnimations().filter((a) => !(a.animationName && a.effect?.target?.classList?.contains('nt')) && a.effect?.getComputedTiming().endTime !== Infinity).every((a) => a.playState !== 'running')", null, new() { PollingInterval = 20 });

    private static async Task ShowAsync(IPage page, string button)
    {
        await page.EvaluateAsync(Holds);
        await page.Locator("main button", new() { HasTextRegex = new Regex($"^{Regex.Escape(button)}$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync();
        await page.WaitForSelectorAsync(".notice:popover-open > .nt");
        await page.WaitForFunctionAsync("() => document.querySelector('.notice:popover-open')?._i", null, new() { PollingInterval = 20 });
    }

    /// <summary>Waits until no card of the deck is moving: only their expiries play.</summary>
    private static async Task StillAsync(IPage page) =>
        await page.WaitForFunctionAsync("() => [...document.querySelectorAll('.notice > .nt')].every(x => x.getAnimations().every(a => a.animationName))", null, new() { PollingInterval = 20 });

    private static async Task<(double X, double Y)> LeftOfAsync(ILocator element, double inset)
    {
        var box = (await element.BoundingBoxAsync())!;
        return (box.X + inset, box.Y + (box.Height / 2));
    }

    /// <summary>
    /// Holds each card's expiry where it stands as the card arrives, since its CSS animation runs on the browser's own time,
    /// which Playwright's clock does not drive, and notes the side each card is swiped out to, since the card is gone once
    /// its exit has played.
    /// </summary>
    private const string Holds = """
        () => {
          window.aiorosOuts = []
          new MutationObserver((changes) => {
            for (const change of changes) {
              for (const node of change.addedNodes) for (const card of node.nodeType === 1 ? [...(node.matches('.nt') ? [node] : []), ...node.querySelectorAll('.nt')] : []) card.getAnimations().forEach((animation) => animation.pause())
              if (change.type === 'attributes' && change.target.getAttribute('data-out')) window.aiorosOuts.push(change.target.getAttribute('data-out'))
            }
          }).observe(document.body, { subtree: true, childList: true, attributes: true, attributeFilter: ['data-out'] })
        }
        """;

    [Fact]
    [Trait("AC", "SONNER-09")]
    public async Task AToastIsSwipedAwayInTheBrowserAsync()
    {
        var page = await OpenAsync("components/sonner");
        await ShowAsync(page, "Show Toast");
        var marked = page.Locator(".notice:popover-open [data-aioros-swipe]").First;
        var marks = $"{await marked.GetAttributeAsync("data-aioros-swipe")} {await marked.GetAttributeAsync("data-aioros-swipe-by")}";
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        var card = page.Locator(".notice:popover-open > .nt").Last;
        var from = await LeftOfAsync(card, 40);
        var short20 = await DragAsync(page, from, (20, 0), 1000, () => card.EvaluateAsync<string>("x => x.style.transform"));
        await page.WaitForFunctionAsync("() => document.querySelector('.notice > .nt').style.transform === ''", null, new() { PollingInterval = 20 });
        await DragAsync(page, from, (60, 0), 1000, NothingAsync);
        var gone = await page.EvaluateAsync<string>("() => window.aiorosOuts.join()");
        await page.WaitForFunctionAsync("() => !document.querySelector('.notice > .nt')", null, new() { PollingInterval = 20 });
        await page.Context.CloseAsync();

        Assert.Equal(("down right 45 0.11", "translate(20px, 0px)", "right", 0), (marks, short20, gone, requests));
    }

    [Fact]
    [Trait("AC", "SONNER-11")]
    public async Task AToastsCloseButtonClosesItInTheBrowserAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var normal = (await ReferenceMetrics.SlotsAsync("sonner-close", theme, "toast-close"))[0];
            var hover = (await ReferenceMetrics.SlotsAsync("sonner-close", theme, "toast-close", "hover"))[0];
            var page = await OpenAsync("components/sonner", theme);
            await ShowAsync(page, "Show with close button");
            var card = page.Locator(".notice:popover-open > .nt").Last;
            var close = card.Locator("button[data-aioros-closes]");
            await RestAsync(page);
            var outer = (await card.BoundingBoxAsync())!;
            var box = (await close.BoundingBoxAsync())!;
            if (Math.Abs(box.X - outer.X + 6) > 0.5 || Math.Abs(box.Y - outer.Y + 6) > 0.5 || box.Width != 20 || box.Height != 20)
            {
                wrong.Add($"{theme}: close at {box.X - outer.X},{box.Y - outer.Y} {box.Width}x{box.Height}");
            }

            wrong.AddRange(await LookAsync(page, $"{theme} close", normal));
            await close.HoverAsync();
            await RestAsync(page);
            wrong.AddRange(await LookAsync(page, $"{theme} hovered close", hover));
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            await close.ClickAsync();
            await page.WaitForFunctionAsync("() => !document.querySelector('.notice > .nt')", null, new() { PollingInterval = 20 });
            wrong.AddRange(requests == 0 ? [] : [$"{theme}: {requests} requests"]);
            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    private static async Task<IEnumerable<string>> LookAsync(IPage page, string what, ElementMetrics expected)
    {
        var found = await page.EvaluateAsync<string[]>("() => { const b = document.querySelector('.notice .nt button[data-aioros-closes]'); const s = getComputedStyle(b); return [s.backgroundColor, s.borderTopColor, getComputedStyle(b.querySelector('path')).stroke, s.borderTopLeftRadius] }");
        return new[] { ("background", found[0], expected.Background), ("border", found[1], expected.BorderColor), ("icon", found[2], expected.Color) }
            .Where(look => !CssColor.Parse(look.Item2).Near(CssColor.Parse(look.Item3))).Select(look => $"{what}: {look.Item1} {look.Item2}, shadcn {look.Item3}")
            .Concat(found[3] is "10px" or "50%" ? [] : [$"{what}: radius {found[3]}"]);
    }

    [Fact]
    [Trait("AC", "SONNER-12")]
    public async Task TheStackExpandsAndPausesInTheBrowserAsync()
    {
        const string Read = "() => [...document.querySelectorAll('.notice:popover-open > .nt')].map(x => { const s = getComputedStyle(x); return `${s.translate}|${s.scale}|${x.hasAttribute('data-behind')}|${s.animationPlayState}|${x.style.getPropertyValue('--expire')}` })";
        var page = await OpenAsync("components/sonner");
        await ShowAsync(page, "Show three toasts");
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.notice:popover-open > .nt').length === 3", null, new() { PollingInterval = 20 });
        var deck = await page.Locator(".notice:popover-open").GetAttributeAsync("data-aioros-deck");
        await page.Mouse.MoveAsync(10, 10);
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.notice > .nt[data-behind]').length === 2", null, new() { PollingInterval = 20 });
        await StillAsync(page);
        var collapsed = await page.EvaluateAsync<string[]>(Read);
        await page.Locator(".notice:popover-open > .nt").Last.HoverAsync();
        await page.WaitForFunctionAsync("() => !document.querySelector('.notice > .nt[data-behind]')", null, new() { PollingInterval = 20 });
        await StillAsync(page);
        var expanded = await page.EvaluateAsync<string[]>(Read);
        var gaps = await page.EvaluateAsync<double[]>("() => { const b = [...document.querySelectorAll('.notice:popover-open > .nt')].map(x => x.getBoundingClientRect()); return [b[2].top - b[1].bottom, b[1].top - b[0].bottom].map(g => Math.round(g * 10) / 10) }");
        await page.Mouse.MoveAsync(10, 10);
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.notice > .nt[data-behind]').length === 2", null, new() { PollingInterval = 20 });
        await StillAsync(page);
        var left = await page.EvaluateAsync<string[]>(Read);
        await page.Context.CloseAsync();

        Assert.Equal("14 3 0.05", deck);
        Assert.Equal(["0px -28px|0.9|true|running|4000ms", "0px -14px|0.95|true|running|4000ms", "0px|none|false|running|4000ms"], collapsed);
        Assert.All(expanded, card => Assert.Matches(@"\|none\|false\|paused\|", card));
        Assert.Equal([14, 14], gaps);
        Assert.Equal(collapsed, left);
    }

    [Fact]
    [Trait("AC", "SONNER-13")]
    public async Task TheStackMovesAsToastsComeAndGoInTheBrowserAsync()
    {
        var page = await OpenAsync("components/sonner");
        await ShowAsync(page, "Show Toast");
        await page.EvaluateAsync("() => document.querySelector('.notice > .nt').first = true");
        await page.Locator("main button", new() { HasTextRegex = new Regex("^Show Toast$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.notice:popover-open > .nt').length === 2", null, new() { PollingInterval = 20 });
        const string First = "() => { const x = [...document.querySelectorAll('.notice > .nt')].find(x => x.first); if (!x) return 'replaced'; const s = getComputedStyle(x); return `${s.translate}|${s.scale}|${x.hasAttribute('data-behind')}|${s.animationPlayState}|${s.transitionProperty.includes('translate')} ${s.transitionDuration.split(',')[0]}` }";
        await StillAsync(page);
        var behind = await page.EvaluateAsync<string>(First);
        await page.Locator(".notice:popover-open > .nt").Last.Locator("button", new() { HasText = "Undo" }).ClickAsync();
        await page.WaitForFunctionAsync("() => document.body.innerText.includes('Undid the event')", null, new() { PollingInterval = 20 });
        await page.WaitForFunctionAsync("() => document.querySelectorAll('.notice > .nt').length === 1", null, new() { PollingInterval = 20 });
        await StillAsync(page);
        var front = await page.EvaluateAsync<string>(First);
        await page.Context.CloseAsync();

        Assert.Equal("0px -14px|0.95|true|running|true 0.4s", behind);
        Assert.Matches(@"^0px\|none\|false\|", front);
    }

    [Fact]
    [Trait("AC", "DRAWER-10")]
    public async Task ADrawerIsDraggedDownToCloseInTheBrowserAsync()
    {
        var page = await OpenAsync("components/drawer");
        await page.Locator("main button", new() { HasTextRegex = new Regex("^Open Drawer$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync();
        await page.WaitForSelectorAsync("dialog.prompt:modal");
        await RestAsync(page);
        var surface = page.Locator("dialog.prompt[open] [data-aioros-swipe]");
        var marks = $"{await surface.GetAttributeAsync("data-aioros-swipe")} {await surface.GetAttributeAsync("data-aioros-swipe-by")}";
        var box = (await surface.BoundingBoxAsync())!;
        var from = (box.X + (box.Width / 2), box.Y + 10);
        var fifth = await DragAsync(page, from, (0, box.Height / 5), 1000, async () =>
        {
            var found = await page.EvaluateAsync<double[]>("() => { const d = document.querySelector('dialog.prompt[open]'); const w = d.querySelector('[data-aioros-swipe]'); return [parseFloat(getComputedStyle(w).translate.split(' ')[1] ?? '0'), +getComputedStyle(d.querySelector('.pm')).opacity] }");
            return $"{Math.Round(found[0] / box.Height * 5, 2).ToString(CultureInfo.InvariantCulture)} {Math.Round(found[1], 2).ToString(CultureInfo.InvariantCulture)}";
        });
        await RestAsync(page);
        var back = await page.EvaluateAsync<string>("() => { const d = document.querySelector('dialog.prompt[open]'); return `${getComputedStyle(d.querySelector('[data-aioros-swipe]')).translate} ${getComputedStyle(d.querySelector('.pm')).opacity}` }");
        await DragAsync(page, from, (0, box.Height / 3), 1000, NothingAsync);
        await page.WaitForSelectorAsync("dialog.prompt[open]", new() { State = WaitForSelectorState.Detached });
        await page.Context.CloseAsync();

        Assert.Equal(("down 0.25 0.4", "1 0.8", "none 1"), (marks, fifth, back));
    }

    [Fact]
    [Trait("AC", "CAROUSEL-06")]
    public async Task ACarouselIsDraggedByTheMouseInTheBrowserAsync()
    {
        var page = await OpenAsync("components/carousel");
        var carousel = page.Locator("[aria-roledescription=carousel]").First;
        var marked = await carousel.EvaluateAsync<bool>("c => !!c.closest('[data-aioros-drag]') || !!c.querySelector('[data-aioros-drag]')");
        var strip = carousel.Locator("[data-aioros-strip]");
        await strip.ScrollIntoViewIfNeededAsync();
        var box = (await strip.BoundingBoxAsync())!;
        var from = (box.X + (box.Width / 2), box.Y + (box.Height / 2));
        async Task<double> SettledAsync()
        {
            await strip.EvaluateAsync("s => new Promise((done) => { const c = s.closest('[data-aioros-drag]'); const look = () => c._d ? setTimeout(look, 20) : done(0); look() })");
            return await strip.EvaluateAsync<double>("s => Math.round(s.scrollLeft / s.children[0].offsetWidth * 100) / 100");
        }

        await DragAsync(page, from, (-80, 0), 1000, NothingAsync, hold: 200);
        var held = await SettledAsync();
        await DragAsync(page, from, (-60, 0), 40, NothingAsync);
        var flicked = await SettledAsync();
        await page.Context.CloseAsync();

        Assert.Equal((true, 0d, 1d), (marked, held, flicked));
    }
}
