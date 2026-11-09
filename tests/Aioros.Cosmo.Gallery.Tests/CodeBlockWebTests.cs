using System.Text.Json;
using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class CodeBlockWebTests(GalleryHost gallery, Browser browser)
{
    private const string Elements = """
        () => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          return [...document.querySelectorAll("#fixture *")].map((el) => {
            const r = el.getBoundingClientRect(), s = getComputedStyle(el)
            return { text: el.textContent.trim(), leaf: el.children.length === 0, x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height, background: s.backgroundColor, color: s.color }
          })
        }
        """;

    private static readonly string[] Fixtures =
    [
        "code-block-demo", "code-block-basic", "code-block-header", "code-block-line-numbers", "code-block-highlighted-lines", "code-block-diff",
        "code-block-destructive", "code-block-size", "code-block-highlighting",
        "code-block-actions", "code-block-ansi", "code-block-chat", "code-block-floating-actions", "code-block-height", "code-block-languages", "code-block-rtl",
        "code-block-show-more", "code-block-show-more-scroll", "code-block-start-line", "code-block-streaming", "code-block-tabs", "code-block-trace", "code-block-typeset", "code-block-wrap", "code-block-markdown",
    ];

    private sealed record Box(string Text, bool Leaf, double X, double Y, double Width, double Height, string Background, string Color);

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static bool Near(Box found, ElementMetrics expected, double width) =>
        Math.Abs(found.X - expected.X) <= 1.5 && Math.Abs(found.Y - expected.Y) <= 1.5 && Math.Abs(found.Height - expected.Height) <= 1.5 && Math.Abs(found.Width - expected.Width) <= width;

    private static bool Painted(Box found, string expected) =>
        CssColor.Parse(expected) is var color && (color.A == 0 || CssColor.Parse(found.Background).Near(color, 2));

    /// <summary>shadcn/ui's elements of a slot that no element of Cosmo's page matches: the same text, the same box and the same background.</summary>
    private static string Bare(string text) => string.Concat(text.Where(static c => !char.IsWhiteSpace(c)));

    /// <summary>Whether an element holds a text: its own, among a line's number and marker, or any text for a block (<paramref name="text"/> false).</summary>
    private static bool Holds(Box box, ElementMetrics element, bool text) => !text || (Bare(element.Text).Length == 0 ? Bare(box.Text).All(char.IsAsciiDigit) : Bare(box.Text).Contains(Bare(element.Text), StringComparison.Ordinal));

    private static IEnumerable<string> Unmatched(string what, List<Box> page, IEnumerable<ElementMetrics> expected, double width = 1.5, bool text = true) =>
        expected.Where(element => !page.Any(box => Holds(box, element, text) && Near(box, element, width) && Painted(box, element.Background)))
            .Select(element => $"{what} {element.Slot} {element.Text}: {Looks(page, element)}, shadcn {element.X:0.##},{element.Y:0.##} {element.Width:0.##}x{element.Height:0.##} {element.Background}");

    private static string Looks(List<Box> page, ElementMetrics element) => string.Join(" / ", page.Where(box => Holds(box, element, true))
        .OrderBy(box => Math.Abs(box.Y - element.Y) + Math.Abs(box.X - element.X)).Take(2).Select(box => $"{box.X:0.##},{box.Y:0.##} {box.Width:0.##}x{box.Height:0.##} {box.Background}"));

    /// <summary>The highlighted tokens of the fixtures (samples/Gallery/tools/highlighted.json): their text and shiki's light and dark colors.</summary>
    private static async Task<Dictionary<string, List<(string Text, string Light, string Dark)>>> TokensAsync()
    {
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Repository.Path("samples", "Gallery", "tools", "highlighted.json"), TestContext.Current.CancellationToken));
        return document.RootElement.EnumerateObject().ToDictionary(fixture => fixture.Name, fixture => fixture.Value.GetProperty("lines").EnumerateArray()
            .SelectMany(static line => line.EnumerateArray()).Select(static token => (token[0].GetString()!, token[1].GetString()!, token[2].GetString()!))
            .Where(static token => token.Item1.Trim().Length > 0).ToList(), StringComparer.Ordinal);
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-05")]
    public async Task TheCopyButtonCopiesInTheBrowserAsync()
    {
        var xaml = await File.ReadAllTextAsync(Repository.Path("samples", "Gallery", "Gallery", "Pages", "fidelity", "code-block-destructive", "Page.axaml"), TestContext.Current.CancellationToken);
        var value = System.Net.WebUtility.HtmlDecode(System.Text.RegularExpressions.Regex.Match(xaml, "<c:CodeBlockCopyButton Value=\"(?<value>[^\"]*)\"").Groups["value"].Value);
        var page = await browser.PageAsync(gallery.Address, "light", 1000, 900);
        await page.Context.GrantPermissionsAsync(["clipboard-read", "clipboard-write"]);
        await page.Clock.InstallAsync();
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/code-block-destructive").ToString());
        await page.EvaluateAsync("u => import(u).then(() => 0)", Aioros.Targets.Ssr.Documents.CopyScript.Path);
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        const string Icons = "() => [...document.querySelectorAll('#fixture button[data-aioros-copies] foreignObject > * > *')].map((icon) => getComputedStyle(icon).opacity).join()";

        var before = await page.EvaluateAsync<string>(Icons);
        await page.ClickAsync("#fixture button[data-aioros-copies]");
        await page.WaitForFunctionAsync("() => document.querySelector('#fixture button[data-aioros-copies]').hasAttribute('data-copied')", null, new() { PollingInterval = 20 });
        var copied = await page.EvaluateAsync<string>("() => navigator.clipboard.readText()");
        var after = await page.EvaluateAsync<string>(Icons);
        await page.Clock.RunForAsync(2001);
        var later = await page.EvaluateAsync<string>(Icons);
        await page.Context.CloseAsync();

        Assert.Equal(value, copied);
        Assert.Equal(("1,0", "0,1", "1,0"), (before, after, later));
        Assert.Equal(0, requests);
    }

    [Fact]
    [Trait("AC", "CODEBLOCK-02")]
    public async Task EveryCodeBlockIsShadcnsOnTheWebAsync()
    {
        var tokens = await TokensAsync();
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in Fixtures)
            {
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var boxes = JsonSerializer.Deserialize<List<Box>>(await page.EvaluateAsync<JsonElement>(Elements), Options) ?? [];
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var what = $"{fixture} {theme}";
                wrong.AddRange(Unmatched(what, boxes, reference.Where(static element => element.Slot is "code-block" or "code-block-content"), text: false));
                wrong.AddRange(Unmatched(what, boxes, reference.Where(static element => element.Slot is "code-block-title" or "code-block-line"), 3));
                foreach (var (text, light, dark) in tokens.GetValueOrDefault(fixture) ?? [])
                {
                    var expected = CssColor.Parse(theme == "light" ? light : dark);
                    wrong.AddRange(boxes.Any(box => box.Leaf && box.Text == text.Trim() && CssColor.Parse(box.Color).Near(expected, 1)) ? [] : [$"{what} token {text}: not in {expected}"]);
                }

                wrong.AddRange((await page.ContentAsync()).Contains("shiki", StringComparison.OrdinalIgnoreCase) ? [$"{what}: the page names shiki"] : []);
                await page.Context.CloseAsync();
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
    private async Task<(IPage Page, Func<int> Posts)> OpenAsync(string fixture)
    {
        var page = await browser.FixtureAsync(gallery.Address, fixture, "light", 1300);
        var posts = 0;
        page.Request += (_, request) => posts += Browser.Sends(request);
        return (page, () => posts);
    }

    private const string Scroller = "document.querySelector('#fixture [class~=sfx], #fixture [class~=sfy]')";

    [Fact]
    [Trait("AC", "CODEBLOCK-07")]
    public async Task TheDocsExamplesBehaveAsShadcnsOnTheWebAsync()
    {
        var wrong = new List<string>();
        var (more, morePosts) = await OpenAsync("code-block-show-more");
        await Browser.KeysAsync(more, flags: true);
        var closed = await more.EvaluateAsync<double>($"() => {Scroller}.getBoundingClientRect().height");
        await more.GetByRole(AriaRole.Button, new() { Name = "Show more" }).ClickAsync();
        await more.WaitForFunctionAsync($"() => {Scroller}.getBoundingClientRect().height > 300", null, new() { PollingInterval = 20 });
        await more.GetByRole(AriaRole.Button, new() { Name = "Show less" }).ClickAsync();
        await more.WaitForFunctionAsync($"() => {Scroller}.getBoundingClientRect().height <= 256", null, new() { PollingInterval = 20 });
        wrong.AddRange(closed == 256 && morePosts() == 0 ? [] : [$"show more: closed {closed}, {morePosts()} requests"]);
        await more.Context.CloseAsync();

        var (actions, actionPosts) = await OpenAsync("code-block-actions");
        await Browser.KeysAsync(actions, flags: true);
        var wrapped = await actions.EvaluateAsync<double>($"() => {Scroller}.scrollWidth - {Scroller}.clientWidth");
        await actions.GetByRole(AriaRole.Button, new() { Name = "Wrap lines" }).ClickAsync();
        await actions.WaitForFunctionAsync($"() => {Scroller}.scrollWidth > {Scroller}.clientWidth", null, new() { PollingInterval = 20 });
        await actions.GetByRole(AriaRole.Button, new() { Name = "Wrap lines" }).ClickAsync();
        await actions.WaitForFunctionAsync($"() => {Scroller}.scrollWidth <= {Scroller}.clientWidth", null, new() { PollingInterval = 20 });
        wrong.AddRange(wrapped <= 0 && actionPosts() == 0 ? [] : [$"wrap: overflow {wrapped} while wrapped, {actionPosts()} requests"]);
        await actions.Context.CloseAsync();

        var (floating, _) = await OpenAsync("code-block-floating-actions");
        const string Shown = "() => getComputedStyle(document.querySelector('#fixture [data-aioros-copies]').parentElement.parentElement).opacity";
        var rest = await floating.EvaluateAsync<string>(Shown);
        await floating.HoverAsync("#fixture [class~=sfx]");
        await floating.WaitForFunctionAsync($"() => ({Shown})() === '1'", null, new() { PollingInterval = 20 });
        wrong.AddRange(rest == "0" ? [] : [$"floating at rest {rest}"]);
        await floating.Context.CloseAsync();

        var (height, _) = await OpenAsync("code-block-height");
        var box = await height.EvaluateAsync<double[]>($"() => [{Scroller}.getBoundingClientRect().height, {Scroller}.scrollHeight]");
        wrong.AddRange(box[0] <= 192 && box[1] > 192 ? [] : [$"height: {box[0]} tall, {box[1]} of content"]);
        await height.Context.CloseAsync();

        var (rtl, _) = await OpenAsync("code-block-rtl");
        var directions = await rtl.EvaluateAsync<string[]>($"() => [document.querySelector('#fixture [dir=rtl]') ? 'rtl' : '', {Scroller}.closest('[dir=ltr]') ? 'ltr' : '', getComputedStyle({Scroller}).direction]");
        wrong.AddRange(directions is ["rtl", "ltr", "ltr"] ? [] : [$"rtl: {string.Join(' ', directions)}"]);
        await rtl.Context.CloseAsync();

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
    [Fact]
    [Trait("AC", "CODEBLOCK-09")]
    public async Task MarkdownIsWrittenOnTheWebAsync()
    {
        var wrong = new List<string>();
        (string Fixture, string[] Codes)[] fixtures = [("code-block-markdown", ["useDebounce", "delay"]), ("bubble-markdown", ["code"])];
        foreach (var (fixture, codes) in fixtures)
        {
            var (page, _) = await OpenAsync(fixture);
            var found = await page.EvaluateAsync<string[]>("() => [...document.querySelectorAll('#fixture code')].map((el) => { let leaf = el; while (leaf.firstElementChild) leaf = leaf.firstElementChild; return el.textContent + ' ' + getComputedStyle(leaf).fontSize + ' ' + getComputedStyle(el).backgroundColor })");
            var muted = await page.EvaluateAsync<string>("() => { let el = [...document.querySelectorAll('#fixture *')].find((candidate) => /^(How do I debounce|Hello! Are you)/.test(candidate.textContent)); while (el && getComputedStyle(el).backgroundColor === 'rgba(0, 0, 0, 0)') el = el.firstElementChild; return el ? getComputedStyle(el).backgroundColor : '' }");
            wrong.AddRange(found.Select(static entry => entry.Split(' ')[0]).SequenceEqual(codes) && found.All(entry => entry.Contains("12.6px", StringComparison.Ordinal) && entry.EndsWith(muted, StringComparison.Ordinal)) ? [] : [$"{fixture} inline code: {string.Join(" | ", found)}, muted {muted}"]);
            if (fixture == "bubble-markdown")
            {
                var gaps = await page.EvaluateAsync<double[]>("() => { const ps = ['Ghost bubbles work for assistant text, markdown, and other content that should not be framed.', 'This is perfect for assistant messages that should not have a frame and can take the full width of the container. You can also render code in it.', 'Ghost bubbles are full width and can take the full width of the container.'].map((text) => [...document.querySelectorAll('#fixture *')].find((el) => el.textContent.trim() === text)); return ps.slice(1).map((p, i) => p.getBoundingClientRect().top - ps[i].getBoundingClientRect().top) }");
                wrong.AddRange(gaps is [var first, var second] && Math.Abs(first - (45.5 + 16)) <= 0.5 && Math.Abs(second - (68.25 + 16)) <= 0.5 ? [] : [$"bubble-markdown paragraphs {string.Join(',', gaps)} apart"]);
            }
            else
            {
                var numbers = await page.EvaluateAsync<int>("() => [...document.querySelectorAll('#fixture *')].filter((el) => !el.children.length && ['1', '2', '3'].includes(el.textContent.trim()) && el.checkVisibility()).length");
                wrong.AddRange(numbers == 0 ? [] : [$"code-block-markdown: {numbers} line numbers shown"]);
            }

            await page.Context.CloseAsync();
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
