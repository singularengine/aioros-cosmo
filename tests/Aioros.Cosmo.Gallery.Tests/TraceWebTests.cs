using System.Globalization;
using System.Text.Json;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class TraceWebTests(GalleryHost gallery, Browser browser)
{
    private const string Elements = """
        () => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          return [...document.querySelectorAll("#fixture *")].map((el) => {
            const r = el.getBoundingClientRect(), s = getComputedStyle(el)
            return { text: el.textContent.trim(), x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height, color: s.color, opacity: s.opacity }
          })
        }
        """;

    private static readonly string[] Fixtures = ["trace-static", "trace-status", "trace-description", "trace-group", "trace-collapsible", "trace-steps",
        "trace-link", "trace-sources", "trace-dialog", "trace-sheet", "trace-commit", "trace-open", "trace-turn", "trace-demo"];

    private sealed record Box(string Text, double X, double Y, double Width, double Height, string Color, string Opacity);

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static string Bare(string text) => string.Concat(text.Where(static c => !char.IsWhiteSpace(c)));

    /// <summary>
    /// Whether an element of the page is shadcn/ui's: its text, its box, its color (for the texts and triggers) and its
    /// opacity.
    /// </summary>
    private static bool Matches(Box box, ElementMetrics element)
    {
        return Bare(box.Text) == Bare(element.Text)
            && Math.Abs(box.X - element.X) <= 1.5 && Math.Abs(box.Y - element.Y) <= 1.5 && Math.Abs(box.Height - element.Height) <= 1.5 && Math.Abs(box.Width - element.Width) <= 3
            && (element.Slot == "trace-line" || CssColor.Parse(box.Color).Near(CssColor.Parse(element.Color), 2))
            && Math.Abs(double.Parse(box.Opacity, CultureInfo.InvariantCulture) - double.Parse(element.Opacity, CultureInfo.InvariantCulture)) < 0.01;
    }

    private static string Nearest(List<Box> page, ElementMetrics element) => string.Join(" / ", page.Where(box => Bare(box.Text) == Bare(element.Text))
        .OrderBy(box => Math.Abs(box.Y - element.Y) + Math.Abs(box.X - element.X)).Take(2).Select(box => $"{box.X:0.##},{box.Y:0.##} {box.Width:0.##}x{box.Height:0.##} {box.Color} {box.Opacity}"));

    [Fact]
    [Trait("AC", "TRACE-02")]
    public async Task EveryTraceIsShadcnsOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in Fixtures)
            {
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var boxes = JsonSerializer.Deserialize<List<Box>>(await page.EvaluateAsync<JsonElement>(Elements), Options) ?? [];
                var reference = (await ReferenceMetrics.OfAsync(fixture, theme)).Select(static element => element with { Text = element.Text.Replace(" (opens in a new tab)", string.Empty, StringComparison.Ordinal) })
                    .Where(static element => element.Width > 0 && element.Slot is "trace-line" or "trace-title" or "trace-description" or "trace-collapsible-trigger");
                wrong.AddRange(reference.Where(element => !boxes.Any(box => Matches(box, element)))
                    .Select(element => $"{fixture} {theme} {element.Slot} {element.Text}: {Nearest(boxes, element)}, shadcn {element.X:0.##},{element.Y:0.##} {element.Width:0.##}x{element.Height:0.##} {element.Color} {element.Opacity}"));
                var lines = await page.EvaluateAsync<int>("() => [...document.querySelectorAll('#fixture details div')].filter((el) => getComputedStyle(el).borderLeftWidth === '1px' && getComputedStyle(el).borderLeftStyle === 'solid').length");
                wrong.AddRange(fixture != "trace-steps" || lines > 0 ? [] : [$"{fixture} {theme}: the open step's content draws no left line"]);
                await page.Context.CloseAsync();
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
    [Fact]
    [Trait("AC", "TRACE-07")]
    public async Task TheDocsTracesActOnTheWebAsync()
    {
        var wrong = new List<string>();
        var link = await browser.FixtureAsync(gallery.Address, "trace-link", "light");
        var hrefs = await link.EvaluateAsync<string[]>("() => [...document.querySelectorAll('#fixture a')].filter((a) => /docs\\.ts|Astro/.test(a.textContent)).map((a) => a.getAttribute('href'))");
        wrong.AddRange(hrefs.SequenceEqual(["#link", "https://docs.astro.build/en/install-and-setup/"]) ? [] : [$"links {string.Join(',', hrefs)}"]);
        await link.Context.CloseAsync();

        foreach (var (fixture, title) in ((string, string)[])[("trace-dialog", "Searched the web for cache keys"), ("trace-sheet", "Explored 4 files"), ("trace-commit", "Edited app/login/page.tsx")])
        {
            var page = await browser.FixtureAsync(gallery.Address, fixture, "light");
            var posts = 0;
            page.Request += (_, request) => posts += Browser.Sends(request);
            await page.Locator("#fixture button").Filter(new() { HasText = title }).First.ClickAsync();
            await page.WaitForSelectorAsync("dialog[open]", new() { State = Microsoft.Playwright.WaitForSelectorState.Visible });
            wrong.AddRange(posts == 0 ? [] : [$"{fixture}: {posts} requests"]);
            await page.Context.CloseAsync();
        }

        var open = await browser.FixtureAsync(gallery.Address, "trace-open", "light");
        await Browser.KeysAsync(open, flags: true);
        var sent = 0;
        open.Request += (_, request) => sent += Browser.Sends(request);
        const string Shown = "() => [...document.querySelectorAll('#fixture *')].some((el) => el.textContent.trim() === '10.33.4' && el.checkVisibility())";
        const string Label = "() => [...document.querySelectorAll('#fixture *')].filter((el) => /the pnpm output/.test(el.textContent) && !el.children.length && el.checkVisibility()).map((el) => el.textContent).join()";
        var states = new List<string> { $"{await open.EvaluateAsync<bool>(Shown)} {await open.EvaluateAsync<string>(Label)}" };
        await open.GetByRole(Microsoft.Playwright.AriaRole.Button, new() { Name = "Show the pnpm output" }).ClickAsync();
        await open.WaitForFunctionAsync($"() => ({Shown})() && /Hide/.test(({Label})())", null, new() { PollingInterval = 20 });
        states.Add($"True {await open.EvaluateAsync<string>(Label)}");
        await open.GetByText("Ran pnpm --version").ClickAsync();
        await open.WaitForFunctionAsync($"() => !({Shown})() && /Show/.test(({Label})())", null, new() { PollingInterval = 20 });
        states.Add($"False {await open.EvaluateAsync<string>(Label)}");
        await open.Context.CloseAsync();
        string[] expected = ["False Show the pnpm output", "True Hide the pnpm output", "False Show the pnpm output"];
        wrong.AddRange(states.SequenceEqual(expected) && sent == 0 ? [] : [$"trace-open {string.Join(" | ", states)}, {sent} requests"]);

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
