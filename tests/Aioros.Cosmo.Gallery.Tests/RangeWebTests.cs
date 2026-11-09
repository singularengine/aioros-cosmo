using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;
using SkiaSharp;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class RangeWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    private async Task<string> FixtureAsync(string name)
    {
        var html = await gallery.Client.GetStringAsync("fidelity/" + name, TestContext.Current.CancellationToken);
        return html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];
    }

    [Fact]
    [Trait("AC", "SLIDER-01")]
    public async Task ASliderIsARangeInputAsync()
    {
        var ranges = RangeTag().Matches(await FixtureAsync("slider-states")).Select(match => match.Groups[1].Value).ToList();

        Assert.Equal(4, ranges.Count);
        Assert.All(ranges, range => Assert.Matches(" name=\"f\\d+\"( data-aioros-local=\"\")? min=\"0\" max=\"100\" step=\"1\"", range));
        Assert.Equal(["50", "50", "33", "40"], ranges.Select(range => Regex.Match(range, "value=\"(\\d+)\"", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value));
        Assert.Equal(["Volume", "Balance", "Locked", "Level"], ranges.Select(range => Regex.Match(range, "aria-label=\"([A-Za-z]+)\"", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value));
        Assert.Equal([false, false, true, false], ranges.Select(range => range.Contains(" disabled", StringComparison.Ordinal)));
    }

    [Fact]
    [Trait("AC", "SLIDER-04")]
    public async Task APostBindsTheSliderAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/slider").ToString());
        await page.FillAsync("input[aria-label='Bound volume']", "70");
        await page.ClickAsync("button:text-is('Apply')");
        await page.WaitForSelectorAsync("text=Volume 70");
        var kept = await page.InputValueAsync("input[aria-label='Bound volume']");
        await page.Context.CloseAsync();

        Assert.Equal("70", kept);
    }

    [Fact]
    [Trait("AC", "SLIDER-07")]
    public async Task ARangeSliderIsTwoRangeInputsDrawnAsShadcnsAsync()
    {
        var groups = RangeGroup().Matches(await FixtureAsync("slider-range")).ToList();
        Assert.Equal(["Price Range", "Levels"], groups.Select(group => group.Groups[1].Value));
        var inputs = groups.Select(group => RangeTag().Matches(group.Groups[2].Value).Select(match => match.Groups[1].Value).ToList()).ToList();
        Assert.All(inputs, pair => Assert.Equal(["Minimum", "Maximum"], pair.Select(input => Regex.Match(input, "aria-label=\"([A-Za-z]+)\"", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value)));
        Assert.Matches(" name=\"f\\d+\"( data-aioros-local=\"\")? min=\"0\" max=\"1000\" step=\"10\" value=\"200\"", inputs[0][0]);
        Assert.Matches(" name=\"f\\d+\"( data-aioros-local=\"\")? min=\"0\" max=\"1000\" step=\"10\" value=\"800\"", inputs[0][1]);
        Assert.Matches(" name=\"f\\d+\"( data-aioros-local=\"\")? min=\"0\" max=\"100\" step=\"1\" value=\"20\"", inputs[1][0]);
        Assert.Matches(" name=\"f\\d+\"( data-aioros-local=\"\")? min=\"0\" max=\"100\" step=\"1\" value=\"60\"", inputs[1][1]);

        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("slider-range", theme);
            var page = await browser.FixtureAsync(gallery.Address, "slider-range", theme);
            var origin = await page.EvaluateAsync<double[]>("() => { const r = document.getElementById('fixture').getBoundingClientRect(); return [r.left, r.top] }");
            using var frame = SKBitmap.Decode(await page.ScreenshotAsync());
            await page.Context.CloseAsync();
            var tracks = reference.Where(element => element.Slot == "slider-track").ToList();
            var ranges = reference.Where(element => element.Slot == "slider-range").ToList();
            var thumbs = reference.Where(element => element.Slot == "slider-thumb").ToList();
            for (var i = 0; i < tracks.Count; i++)
            {
                var drawn = new Drawn(frame, origin[0], origin[1], tracks[i].Width > tracks[i].Height);
                wrong.AddRange(drawn.Thumbs($"{theme} #{i}", tracks[i], thumbs.Skip(2 * i).Take(2)));
                wrong.AddRange(drawn.Track($"{theme} #{i}", tracks[i], ranges[i]));
            }
        }

        Assert.Empty(wrong);
    }

    private sealed record Drawn(SKBitmap Frame, double Left, double Top, bool Horizontal)
    {
        private CssColor At(double along, double across, ElementMetrics track)
        {
            var (x, y) = Horizontal ? (track.X + along, track.Y + across) : (track.X + across, track.Y + along);
            var pixel = Frame.GetPixel((int)Math.Floor(Left + x), (int)Math.Floor(Top + y));
            return new CssColor(pixel.Red, pixel.Green, pixel.Blue, 1);
        }

        private static bool White(CssColor color) => color.R >= 252 && color.G >= 252 && color.B >= 252;

        private static List<(double Start, double End)> Runs(Func<double, CssColor> at, double from, double to)
        {
            var runs = new List<(double Start, double End)>();
            for (var position = from; position < to; position++)
            {
                if (White(at(position)))
                {
                    if (runs.Count > 0 && runs[^1].End == position)
                    {
                        runs[^1] = (runs[^1].Start, position + 1);
                    }
                    else
                    {
                        runs.Add((position, position + 1));
                    }
                }
            }

            return runs;
        }

        public IEnumerable<string> Thumbs(string what, ElementMetrics track, IEnumerable<ElementMetrics> expected)
        {
            var length = Horizontal ? track.Width : track.Height;
            var thickness = Horizontal ? track.Height : track.Width;
            var runs = Runs(along => At(along, Math.Floor(thickness / 2), track), -8, length + 8).Where(run => run.Start > -8 && run.End < length + 8).ToList();
            if (runs.Count != 2)
            {
                return [$"{what}: {runs.Count} thumbs"];
            }

            var offset = Horizontal ? track.X : track.Y;
            var found = runs.Select(run => (Start: run.Start - 1 + offset, Size: run.End - run.Start + 2)).ToList();
            var targets = expected.Select(thumb => (Start: Horizontal ? thumb.X : thumb.Y, Size: Horizontal ? thumb.Width : thumb.Height)).OrderBy(thumb => thumb.Start).ToList();
            var across = runs.Select(run => Runs(position => At(Math.Floor((run.Start + run.End) / 2), position, track), -8, thickness + 8).Single(span => span.Start <= thickness / 2 && span.End >= thickness / 2))
                .Select(span => (Start: span.Start - 1, Size: span.End - span.Start + 2)).ToList();
            var crossTargets = expected.Select(thumb => (Start: Horizontal ? thumb.Y - track.Y : thumb.X - track.X, Size: Horizontal ? thumb.Height : thumb.Width)).ToList();
            return Apart($"{what} thumb at", found, targets).Concat(Apart($"{what} thumb across", across, crossTargets));
        }

        private static IEnumerable<string> Apart(string what, IEnumerable<(double Start, double Size)> found, IEnumerable<(double Start, double Size)> expected) =>
            found.Zip(expected).Where(pair => Math.Abs(pair.First.Start + (pair.First.Size / 2) - pair.Second.Start - (pair.Second.Size / 2)) > 1 || Math.Abs(pair.First.Size - pair.Second.Size) > 2)
                .Select(pair => $"{what} {pair.First.Start:0.##} ({pair.First.Size:0.##}), shadcn {pair.Second.Start:0.##} ({pair.Second.Size:0.##})");

        public IEnumerable<string> Track(string what, ElementMetrics track, ElementMetrics range)
        {
            var muted = CssColor.Parse(track.Background);
            var primary = CssColor.Parse(range.Background);
            var length = Horizontal ? track.Width : track.Height;
            var middle = Math.Floor((Horizontal ? track.Height : track.Width) / 2);
            var start = Horizontal ? range.X - track.X : range.Y - track.Y;
            var end = start + (Horizontal ? range.Width : range.Height);
            var points = new[] { (1.0, muted), (length - 2, muted), (start - 10, muted), (end + 9, muted), (Math.Floor((start + end) / 2), primary) };
            return points.Select(point => (point.Item1, point.Item2, At(point.Item1, middle, track)))
                .Where(point => !point.Item3.Near(point.Item2, 3))
                .Select(point => $"{what} track at {point.Item1:0}: {point.Item3}, shadcn {point.Item2}");
        }
    }

    [Fact]
    [Trait("AC", "SLIDER-08")]
    public async Task ARangeSliderTakesThePointerAndTheKeysAndPostsInOrderAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/slider").ToString());
        var group = page.Locator("[role=group][aria-label='Budget']");
        await group.ScrollIntoViewIfNeededAsync();
        var lower = group.Locator("input[aria-label='Minimum']");
        var upper = group.Locator("input[aria-label='Maximum']");
        var box = (await lower.BoundingBoxAsync())!;
        float At(double value) => (float)(box.X + (value / 1000 * box.Width));
        var middle = box.Y + (box.Height / 2);
        var steps = new List<string>();
        async Task ReadAsync() => steps.Add(await lower.InputValueAsync() + " " + await upper.InputValueAsync());

        await page.Mouse.ClickAsync(At(700), middle);
        await ReadAsync();
        await page.Mouse.MoveAsync(At(200), middle);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(At(500), middle);
        await page.Mouse.MoveAsync(At(900), middle);
        await page.Mouse.UpAsync();
        await ReadAsync();
        foreach (var key in (string[])["ArrowRight", "Shift+ArrowRight", "ArrowLeft", "Home", "End"])
        {
            await page.Keyboard.PressAsync(key);
            await ReadAsync();
        }

        await page.ClickAsync("button:text-is('Apply budget')");
        await page.WaitForSelectorAsync("text=Budget $0 - $1000");
        await ReadAsync();
        await page.Context.CloseAsync();

        Assert.Equal(["200 700", "700 900", "700 910", "700 1000", "700 990", "0 990", "0 1000", "0 1000"], steps);
    }

    [Fact]
    [Trait("AC", "PROGRESS-01")]
    public async Task AProgressBarIsAProgressElementComputedAsShadcnsAsync()
    {
        var values = ProgressTag().Matches(await FixtureAsync("progress")).Select(match => match.Groups[1].Value).ToList();
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("progress", theme, "progress");
            var page = await browser.FixtureAsync(gallery.Address, "progress", theme);
            var found = await Browser.PickAsync(page, "#fixture progress");
            await page.Context.CloseAsync();
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{theme} #{i}", pair.First, pair.Second, 0.5)
                .Concat(CssColor.Parse(pair.First.Background).Near(CssColor.Parse(pair.Second.Background)) ? [] : [$"{theme} #{i}: track {pair.First.Background}, shadcn {pair.Second.Background}"])
                .Concat(Compare.Px(pair.First.Radius) >= 4 ? [] : [$"{theme} #{i}: radius {pair.First.Radius}"])));
        }

        Assert.Equal(["33", "66", "100"], values);
        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "PROGRESS-03")]
    public async Task APostedCommandAdvancesTheBoundProgressAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/progress").ToString());
        await page.ClickAsync("button:text-is('+10%')");
        await page.WaitForSelectorAsync("text=70% done");
        var value = await page.GetAttributeAsync("progress[aria-label='Upload']", "value");
        await page.Context.CloseAsync();

        Assert.Equal("70", value);
    }

    [Fact]
    [Trait("AC", "INPUTGROUP-03")]
    public async Task AnInputGroupIsOneBorderedElementWithItsAddonsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("input-group", theme);
            var groups = reference.Where(element => element.Slot == "input-group").ToList();
            var addons = reference.Where(element => element.Slot == "input-group-addon" && element.Text.Length > 0).ToList();
            var page = await browser.FixtureAsync(gallery.Address, "input-group", theme);
            var found = await Browser.PickAsync(page, "#fixture > div > div");
            var holds = await page.EvaluateAsync<int[]>("[...document.querySelectorAll('#fixture > div > div')].map((g) => g.querySelectorAll('input, textarea').length)");
            var texts = await Browser.PickAsync(page, "#fixture > div > div div:not(:has(*))");
            await page.Context.CloseAsync();
            Assert.Equal(groups.Count, found.Count);
            Assert.All(holds, count => Assert.Equal(1, count));
            wrong.AddRange(found.Zip(groups).SelectMany((pair, i) => new[] { ("border", pair.First.BorderColor, pair.Second.BorderColor) }
                .Where(colors => !CssColor.Parse(colors.Item2).Near(CssColor.Parse(colors.Item3))).Select(colors => $"{theme} #{i}: border {colors.Item2}, shadcn {colors.Item3}")
                .Concat(Compare.Box($"{theme} #{i}", pair.First, pair.Second, 1))
                .Concat(pair.First.BorderWidth == pair.Second.BorderWidth && pair.First.Radius == pair.Second.Radius ? [] : [$"{theme} #{i}: {pair.First.BorderWidth} {pair.First.Radius}, shadcn {pair.Second.BorderWidth} {pair.Second.Radius}"])));
            foreach (var addon in addons)
            {
                var text = texts.FirstOrDefault(element => element.Text == addon.Text);
                wrong.AddRange(text is not null && CssColor.Parse(text.Color).Near(CssColor.Parse(addon.Color)) && text.FontSize == addon.FontSize && Math.Abs(text.Y + text.Height / 2 - Middle(addon)) <= 1.5 ? [] : [$"{theme} addon {addon.Text}: {text?.Color} {text?.FontSize} at {text?.Y}, shadcn {addon.Color} {addon.FontSize} at {addon.Y}"]);
            }
        }

        Assert.Empty(wrong);
    }

    private static double Middle(ElementMetrics element)
    {
        var top = Compare.Px(element.PaddingTop ?? "0px");
        return element.Y + top + (element.Height - top - Compare.Px(element.PaddingBottom ?? "0px")) / 2;
    }

    [Fact]
    [Trait("AC", "INPUTGROUP-04")]
    public async Task APostBindsTheGroupsTextBoxAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/input-group").ToString());
        await page.FillAsync("input[aria-label='Username']", "alex");
        await page.PressAsync("input[aria-label='Username']", "Enter");
        await page.WaitForSelectorAsync("text=4/24");
        var kept = await page.InputValueAsync("input[aria-label='Username']");
        await page.Context.CloseAsync();

        Assert.Equal("alex", kept);
    }

    [Fact]
    [Trait("AC", "INPUTOTP-04")]
    public async Task TypingFillsTheSlotsAndMovesTheActiveLookAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "input-otp", "light");
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        const string Read = """
            () => [...document.querySelector('#fixture [data-aioros-slots]').querySelectorAll('[data-aioros-slot-box]')].map((box) => {
              const s = getComputedStyle(box)
              return box.textContent.trim() + (box.hasAttribute('data-aioros-active') ? '*' + s.borderTopColor + '/' + s.boxShadow.includes('3px') : '')
            }).join('|')
            """;
        var input = page.Locator("#fixture input").First;
        await input.FocusAsync();
        await input.PressAsync("End");
        await input.PressSequentiallyAsync("4");
        var typed = await page.EvaluateAsync<string>(Read);
        await input.BlurAsync();
        var left = await page.EvaluateAsync<string>(Read);
        await page.Context.CloseAsync();

        var slots = typed.Split('|');
        Assert.Equal(["1", "2", "3", "4"], slots[..4]);
        Assert.StartsWith("*", slots[4], StringComparison.Ordinal);
        Assert.True(CssColor.Parse(slots[4][1..slots[4].IndexOf('/', StringComparison.Ordinal)]).Near(CssColor.Parse("oklch(0.708 0 0)")) && slots[4].EndsWith("/true", StringComparison.Ordinal), slots[4]);
        Assert.DoesNotContain("*", left, StringComparison.Ordinal);
        Assert.Equal(0, requests);
    }

    [Fact]
    [Trait("AC", "INPUTOTP-01")]
    public async Task TheCodeBoxesAreComputedAsShadcnsAsync()
    {
        var fixture = await FixtureAsync("input-otp");
        var inputs = Regex.Matches(fixture, "<input type=\"text\"([^>]*)>", RegexOptions.None, TimeSpan.FromSeconds(1)).Select(match => match.Groups[1].Value).ToList();
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("input-otp", theme, "input-otp-slot");
            var page = await browser.FixtureAsync(gallery.Address, "input-otp", theme);
            var found = (await Browser.PickAsync(page, "#fixture div")).Where(element => Math.Abs(element.Width - 36) < 0.5 && Math.Abs(element.Height - 36) < 0.5 && element.BorderWidth == "1px").ToList();
            await page.Context.CloseAsync();
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{theme} #{i}", pair.First, pair.Second, 0.5)
                .Concat(CssColor.Parse(pair.First.BorderColor).Near(CssColor.Parse(pair.Second.BorderColor)) ? [] : [$"{theme} #{i}: border {pair.First.BorderColor}, shadcn {pair.Second.BorderColor}"])
                .Concat(pair.First.Text == pair.Second.Text ? [] : [$"{theme} #{i}: text '{pair.First.Text}', shadcn '{pair.Second.Text}'"])));
            wrong.AddRange(found[0].Radius == expected[0].Radius ? [] : [$"{theme}: first radius {found[0].Radius}, shadcn {expected[0].Radius}"]);
        }

        Assert.Equal(2, inputs.Count);
        Assert.Matches(" name=\"f\\d+\"[^>]*maxlength=\"6\"", inputs[0]);
        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUTOTP-03")]
    public async Task APostBindsTheCodeAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/input-otp").ToString());
        var code = page.Locator("input[maxlength='6']").Last;
        await code.FillAsync("123456");
        await page.ClickAsync("button:text-is('Verify')");
        await page.WaitForSelectorAsync("text=Verified 123456");
        var boxes = await page.Locator("input[maxlength='6']").Last.EvaluateAsync<string>("(input) => input.parentElement.textContent");
        await page.Context.CloseAsync();

        Assert.Contains("123456", boxes, StringComparison.Ordinal);
    }

    [GeneratedRegex("<div role=\"group\" aria-label=\"([^\"]+)\"[^>]*><div data-aioros-range[^>]*>(.*?)</div></div>")]
    private static partial Regex RangeGroup();

    [GeneratedRegex("<input type=\"range\"([^>]*)>")]
    private static partial Regex RangeTag();

    [GeneratedRegex("<progress max=\"100\" value=\"(\\d+)\"")]
    private static partial Regex ProgressTag();
}
