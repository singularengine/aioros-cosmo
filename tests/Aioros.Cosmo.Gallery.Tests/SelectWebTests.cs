using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class SelectWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "NATIVESELECT-01")]
    public async Task AComboBoxIsASelectElementBoundTwoWayAsync()
    {
        var html = await gallery.Client.GetStringAsync("fidelity/native-select", TestContext.Current.CancellationToken);
        var selects = SelectTag().Matches(html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..]).ToList();

        Assert.Equal(3, selects.Count);
        Assert.All(selects, select => Assert.Matches(" name=\"f\\d+\"", select.Groups[1].Value));
        Assert.Equal(["Select status", "Todo", "Done"], Regex.Matches(selects[0].Groups[2].Value, ">([^<]+)</option>", RegexOptions.None, TimeSpan.FromSeconds(1)).Select(match => match.Groups[1].Value));
        Assert.Contains("value=\"0\" selected", selects[0].Groups[2].Value, StringComparison.Ordinal);
        Assert.Contains(" disabled", selects[2].Groups[1].Value, StringComparison.Ordinal);

        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/select").ToString());
        await page.SelectOptionAsync("select[aria-label='Fruit']", new Microsoft.Playwright.SelectOptionValue { Label = "Grapes" });
        await page.WaitForSelectorAsync("text=You picked Grapes");
        await page.Context.CloseAsync();
    }

    [Fact]
    [Trait("AC", "NATIVESELECT-02")]
    public async Task EverySelectIsComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in (string[])["native-select", "select-trigger"])
            {
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var expected = reference.Where(element => element.Slot == fixture).ToList();
                var wrappers = reference.Where(element => element.Slot == "native-select-wrapper").ToList();
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var found = await Browser.PickAsync(page, "#fixture select");
                var appearance = await page.EvaluateAsync<string[]>("() => [...document.querySelectorAll('#fixture select')].map(s => getComputedStyle(s).appearance)");
                await page.Context.CloseAsync();
                Assert.Equal(expected.Count, found.Count);
                wrong.AddRange(appearance.Where(value => value != (fixture == "native-select" ? "none" : "base-select")).Select(value => $"{fixture} {theme}: appearance {value}"));
                wrong.AddRange(found.SelectMany((select, i) => Differences($"{fixture} {theme} #{i}", select, expected[i], wrappers.Count > 0 ? wrappers[i].Opacity : expected[i].Opacity)));
            }
        }

        Assert.Empty(wrong);
    }

    private const string OpenList = """
        () => {
          const s = document.querySelector('#fixture select'), t = s.getBoundingClientRect(), p = getComputedStyle(s, '::picker(select)'), [up, down] = s.querySelectorAll('[data-aioros-scroller]');
          const shown = b => getComputedStyle(b).display === 'block', h = parseFloat(p.height), options = [...s.querySelectorAll('option')].filter(o => o.getBoundingClientRect().height > 0), first = options[0].getBoundingClientRect();
          const top = shown(up) ? up.getBoundingClientRect().top - 1 : down.getBoundingClientRect().bottom + 1 - h, left = first.left - 5;
          const box = r => [r.left - left, r.top - top, r.width, r.height];
          const checked = s.querySelector('option:checked'), mark = getComputedStyle(checked, '::checkmark'), c = checked.getBoundingClientRect();
          return JSON.stringify({ trigger: [t.left, t.top, t.width, t.height], list: [left, top, first.width + 10, h],
            up: shown(up) ? box(up.getBoundingClientRect()) : null, down: shown(down) ? box(down.getBoundingClientRect()) : null,
            options: options.map(o => ({ text: o.textContent, box: box(o.getBoundingClientRect()), size: getComputedStyle(o).fontSize, color: getComputedStyle(o).color, disabled: o.disabled })),
            checked: checked.textContent, mark: mark.display === 'none' || c.height === 0 ? null : [c.right - parseFloat(mark.right) - parseFloat(mark.width) - left, c.top + parseFloat(mark.top) - top, parseFloat(mark.width), parseFloat(mark.height)] });
        }
        """;

    private static readonly System.Text.Json.JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    private sealed record Option(string Text, double[] Box, string Size, string Color, bool Disabled);

    private sealed record OpenListState(double[] Trigger, double[] List, double[]? Up, double[]? Down, Option[] Options, string Checked, double[]? Mark);

    private static async Task<OpenListState> OpenAsync(Microsoft.Playwright.IPage page, bool framed = true)
    {
        await page.ClickAsync("#fixture select");
        const string Settled = "() => (getComputedStyle(document.querySelector('#fixture [data-aioros-scroller=down]')).display === 'block' || getComputedStyle(document.querySelector('#fixture [data-aioros-scroller=up]')).display === 'block') && [...document.querySelectorAll('#fixture option')].filter(o => o.offsetHeight > 0).every(o => Math.abs(o.getBoundingClientRect().height - o.offsetHeight) < 0.01)";
        for (var attempt = 0; attempt < 2000 && !await page.EvaluateAsync<bool>(Settled); attempt++)
        {
        }

        if (framed)
        {
            await page.EvaluateAsync("() => new Promise(done => requestAnimationFrame(() => requestAnimationFrame(done)))");
        }

        return System.Text.Json.JsonSerializer.Deserialize<OpenListState>(await page.EvaluateAsync<string>(OpenList), Json)!;
    }

    private static IEnumerable<string> Boxed(string what, double[] found, ElementMetrics expected, ElementMetrics content) =>
        new[] { ("x", found[0], expected.X - content.X), ("y", found[1], expected.Y - content.Y), ("width", found[2], expected.Width), ("height", found[3], expected.Height) }
            .Where(edge => Math.Abs(edge.Item2 - edge.Item3) > 1.5).Select(edge => $"{what}: {edge.Item1} {edge.Item2:0.##}, shadcn {edge.Item3:0.##}");

    [Fact]
    [Trait("AC", "SELECT-09")]
    public async Task AnOverflowingListIsShadcnsWithItsScrollButtonsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in (string[])["select-scrollable", "select-scrolled"])
            {
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var open = await OpenAsync(page);
                await page.Context.CloseAsync();
                wrong.AddRange(Placed($"{fixture} {theme}", open, reference));
                wrong.AddRange(Buttons($"{fixture} {theme}", open, reference));
                wrong.AddRange(Items($"{fixture} {theme}", open, reference));
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private static IEnumerable<string> Placed(string what, OpenListState open, IReadOnlyList<ElementMetrics> reference)
    {
        var content = reference.Single(element => element.Slot == "select-content");
        var trigger = reference.Single(element => element.Slot == "select-trigger");
        if (Math.Abs(open.List[1] - (open.Trigger[1] + open.Trigger[3]) - (content.Y - trigger.Y - trigger.Height)) > 1)
        {
            yield return $"{what}: list {open.List[1] - open.Trigger[1] - open.Trigger[3]:0.##} below the trigger";
        }

        if (Math.Abs(open.List[2] - content.Width) > 1.5 || Math.Abs(open.List[3] - content.Height) > 1.5)
        {
            yield return $"{what}: list {open.List[2]:0.##}x{open.List[3]:0.##}, shadcn {content.Width}x{content.Height}";
        }
    }

    private static IEnumerable<string> Buttons(string what, OpenListState open, IReadOnlyList<ElementMetrics> reference)
    {
        var content = reference.Single(element => element.Slot == "select-content");
        foreach (var (found, slot) in new[] { (open.Up, "select-scroll-up-button"), (open.Down, "select-scroll-down-button") })
        {
            var expected = reference.SingleOrDefault(element => element.Slot == slot);
            var errors = (found, expected) switch
            {
                (null, null) => [],
                ({ } box, { } metrics) => Boxed($"{what} {slot}", box, metrics, content),
                _ => [$"{what}: {slot} shown {found is not null}, shadcn {expected is not null}"],
            };
            foreach (var error in errors)
            {
                yield return error;
            }
        }
    }

    private static List<string> Items(string what, OpenListState open, IReadOnlyList<ElementMetrics> reference)
    {
        var content = reference.Single(element => element.Slot == "select-content");
        var top = open.Up is { } up ? up[1] + up[3] : 1;
        var bottom = open.Down is { } down ? down[1] : content.Height - 1;
        var errors = new List<string>();
        foreach (var expected in reference.Where(element => element.Slot is "select-item" or "select-label" && element.Y - content.Y + element.Height > top + 0.5 && element.Y - content.Y < bottom - 0.5))
        {
            var option = open.Options.FirstOrDefault(option => option.Text == expected.Text);
            errors.AddRange(option is null ? [$"{what}: no {expected.Text}"] : Boxed($"{what} {expected.Text}", option.Box, expected, content));
            errors.AddRange(option is null || expected.Slot != "select-label" || (option.Size == expected.FontSize && CssColor.Parse(option.Color).Near(CssColor.Parse(expected.Color))) ? [] : [$"{what} label {expected.Text}: {option.Size} {option.Color}, shadcn {expected.FontSize} {expected.Color}"]);
        }

        var item = reference.SingleOrDefault(element => element.Slot == "select-item" && element.Text == open.Checked);
        var mark = reference.FirstOrDefault(element => element.Slot == "select-item-indicator" && item is not null && element.Y >= item.Y && element.Y < item.Y + item.Height);
        errors.AddRange((open.Mark, mark) switch
        {
            ({ } box, { } metrics) => Boxed($"{what} check", box, CheckIconIn(metrics), content),
            (null, { } metrics) when metrics.Y - content.Y > top && metrics.Y - content.Y < bottom => [$"{what}: no check"],
            ({ }, null) => [$"{what}: a check on {open.Checked}"],
            _ => [],
        });
        return errors;
    }

    private static ElementMetrics CheckIconIn(ElementMetrics span) =>
        span with { X = span.X + (span.Width / 2) - 8, Y = span.Y + (span.Height / 2) - 8, Width = 16, Height = 16 };

    private const string AtTheViewportsEdge = """
        () => {
          const shift = document.getElementById('fixture').getBoundingClientRect().left;
          document.body.style.position = 'relative';
          document.body.style.left = -shift + 'px';
          const r = document.getElementById('fixture').getBoundingClientRect();
          return [r.left, r.top];
        }
        """;

    [Fact]
    [Trait("AC", "SELECT-12")]
    public async Task TheListKeepsTenPixelsFromTheViewportsEdgeAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in (string[])["select-scrollable", "select-scrolled"])
            {
                var content = (await ReferenceMetrics.OfAsync(fixture, theme)).Single(element => element.Slot == "select-content");
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var origin = await page.EvaluateAsync<double[]>(AtTheViewportsEdge);
                var open = await OpenAsync(page);
                await page.Context.CloseAsync();
                var (x, y) = (open.List[0] - origin[0], open.List[1] - origin[1]);
                wrong.AddRange(Math.Abs(x - content.X) <= 1.5 && Math.Abs(y - content.Y) <= 1.5 ? [] : [$"{fixture} {theme}: list at {x:0.##},{y:0.##}, shadcn {content.X},{content.Y}"]);
            }
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SELECT-10")]
    public async Task TheScrollButtonsScrollTheListOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light");
        await page.Clock.InstallAsync(new() { TimeDate = DateTime.UnixEpoch });
        await page.Clock.PauseAtAsync(DateTime.UnixEpoch.AddMilliseconds(1024));
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/select-scrollable").ToString());
        await page.EvaluateAsync("document.fonts.ready");
        var start = await OpenAsync(page, framed: false);
        const string Top = "() => { const u = document.querySelector('#fixture [data-aioros-scroller=up]'); return [...document.querySelectorAll('#fixture option')].find(o => o.getBoundingClientRect().height > 0).getBoundingClientRect().top - (getComputedStyle(u).display === 'block' ? u.offsetHeight : 0) }";
        var at = await page.EvaluateAsync<double>(Top);
        var down = start.Down!;
        await page.Mouse.MoveAsync((float)(start.List[0] + down[0] + (down[2] / 2)), (float)(start.List[1] + down[1] + (down[3] / 2)));
        await page.Clock.RunForAsync(50);
        var first = at - await page.EvaluateAsync<double>(Top);
        var upShown = false;
        for (var attempt = 0; attempt < 2000 && !upShown; attempt++)
        {
            upShown = await page.EvaluateAsync<bool>("() => getComputedStyle(document.querySelector('#fixture [data-aioros-scroller=up]')).display === 'block'");
        }

        var shifted = await page.EvaluateAsync<double>(Top);
        await page.Clock.RunForAsync(50);
        var second = shifted - await page.EvaluateAsync<double>(Top);
        await page.Mouse.MoveAsync(990, 890);
        var left = await page.EvaluateAsync<double>(Top);
        await page.Clock.RunForAsync(200);
        var still = left - await page.EvaluateAsync<double>(Top);
        await page.Context.CloseAsync();

        Assert.Equal((32.0, true, 32.0, 0.0), (Math.Round(first), upShown, Math.Round(second), Math.Round(still)));
    }

    private static IEnumerable<string> Differences(string what, ElementMetrics found, ElementMetrics expected, string opacity)
    {
        return Compare.Box(what, found, expected, 1)
            .Concat(InputWebTests.Fonts(what, found, expected))
            .Concat(new[] { ("radius", found.Radius, expected.Radius), ("border width", found.BorderWidth, expected.BorderWidth), ("opacity", found.Opacity, opacity) }
                .Where(pair => pair.Item2 != pair.Item3).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"))
            .Concat(new[] { ("border", found.BorderColor, expected.BorderColor), ("color", found.Color, expected.Color), ("background", found.Background, expected.Background) }
                .Where(pair => !CssColor.Parse(pair.Item2).Near(CssColor.Parse(pair.Item3))).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"));
    }

    [GeneratedRegex("<select([^>]*)>(.*?)</select>", RegexOptions.Singleline)]
    private static partial Regex SelectTag();
}
