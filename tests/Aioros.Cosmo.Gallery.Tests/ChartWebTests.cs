using System.Text.RegularExpressions;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class ChartWebTests(GalleryHost gallery, Browser browser)
{
    private async Task<string> FixtureAsync(string name)
    {
        var html = await gallery.Client.GetStringAsync("fidelity/" + name, TestContext.Current.CancellationToken);
        return html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];
    }

    private static List<string> Tooltips(string fixture) =>
        [.. fixture.Split("<foreignObject", StringSplitOptions.None).Skip(1).Select(static tip => tip[..tip.IndexOf("</foreignObject>", StringComparison.Ordinal)]).Where(static tip => tip.Contains("pictor-content", StringComparison.Ordinal))];

    private static List<string> Texts(string markup) => [.. Paragraph().Matches(markup).Select(static match => match.Groups[1].Value)];

    [Fact]
    [Trait("AC", "CHART-04")]
    public async Task EveryCategoryHasShadcnsTooltipWithEverySeriesAsync()
    {
        var tips = Tooltips(await FixtureAsync("chart-line-multiple"));
        var first = tips[0];

        Assert.Equal(6, tips.Count);
        Assert.Equal(["January", "Desktop", "186", "Mobile", "80"], Texts(first));
        Assert.Equal(["June", "Desktop", "214", "Mobile", "140"], Texts(tips[5]));
        Assert.Contains("min-width:128px;padding:6px 10px 6px 10px;background:#ffffff;border:1px solid rgba(229,229,229,0.502);border-radius:8px;box-shadow:0 20px 25px -5px rgba(0,0,0,0.102), 0 8px 10px -6px rgba(0,0,0,0.102)", first, StringComparison.Ordinal);
        Assert.Equal(["#d4d4d4", "#737373"], Square().Matches(first).Select(static match => match.Groups[1].Value));
        Assert.Contains("line-height:16px;color:#0a0a0a;font-size:12px;font-weight:500", first, StringComparison.Ordinal);
        Assert.Contains("font-family:Geist Mono;color:#0a0a0a;font-size:12px;font-weight:500;margin:0 0 0 0;align-self:center;margin-left:auto;\">186<", first, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "CHART-05")]
    public async Task TheTooltipOptionsShapeItsLinesAsync()
    {
        var unlabelled = Tooltips(await FixtureAsync("chart-bar-default"))[0];
        var plain = Tooltips(await FixtureAsync("chart-tooltip-indicator-none"))[1];
        var lined = Tooltips(await FixtureAsync("chart-tooltip-indicator-line"))[1];
        var dashed = Tooltips(await FixtureAsync("chart-bar-multiple"))[0];

        Assert.Equal(["Desktop", "186"], Texts(unlabelled));
        Assert.Equal(["2024-07-16", "Running", "380", "Swimming", "420"], Texts(plain));
        Assert.DoesNotContain("width:10px", plain, StringComparison.Ordinal);
        Assert.Equal(["2024-07-16", "Running", "380", "Swimming", "420"], Texts(lined));
        Assert.Equal(2, Regex.Count(lined, "width:4px;background:#[0-9a-f]{6};border-radius:2px", RegexOptions.None, TimeSpan.FromSeconds(1)));
        Assert.Equal(2, Regex.Count(dashed, "box-sizing:border-box;border:1.5px dashed #[0-9a-f]{6};border-radius:2px", RegexOptions.None, TimeSpan.FromSeconds(1)));
    }

    [Fact]
    [Trait("AC", "CHART-06")]
    public async Task TheLegendContentIsACenteredRowOfEntriesAsync()
    {
        var fixture = await FixtureAsync("chart-pie-legend");
        var legend = fixture[fixture.IndexOf("recharts-legend-wrapper", StringComparison.Ordinal)..];

        Assert.Contains("<div style=\"display:flex;flex-wrap:wrap;justify-content:center;gap:8px\"><div style=\"display:flex;justify-content:center;flex-basis:25%\">", legend, StringComparison.Ordinal);
        Assert.Equal(["Chrome", "Safari", "Firefox", "Edge", "Other"], Texts(legend).Take(5));
        Assert.Equal(5, Regex.Count(legend, "width:8px;height:8px;background:#[0-9a-f]{6};border-radius:2px", RegexOptions.None, TimeSpan.FromSeconds(1)));
        Assert.Contains("padding:12px 0 0 0", legend, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "CHART-13")]
    public async Task APlainLegendIsShadcnsLegendOnTheWebAsync()
    {
        var fixture = await FixtureAsync("chart-plain-legend");
        var legends = fixture.Split("recharts-legend-wrapper", StringSplitOptions.None);

        Assert.Equal(3, legends.Length);
        Assert.Contains("display:flex;flex-wrap:wrap;justify-content:center;gap:16px", legends[1], StringComparison.Ordinal);
        Assert.Contains("width:8px;height:8px;background:#", legends[1], StringComparison.Ordinal);
        Assert.DoesNotContain("recharts-default-legend", legends[1], StringComparison.Ordinal);
        Assert.Contains("recharts-default-legend", legends[2], StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "CHART-07")]
    public async Task TheBoundDataIsDrawnWithItsFormatsAsync()
    {
        var area = await FixtureAsync("chart-area-default");
        var labels = await FixtureAsync("chart-bar-label");
        var activities = await FixtureAsync("chart-tooltip-default");

        Assert.Equal(["Jan", "Feb", "Mar", "Apr", "May", "Jun"], Tick().Matches(area).Select(static match => match.Groups[1].Value));
        Assert.Equal(["186", "305", "237", "73", "209", "214"], LabelText().Matches(labels).Select(static match => match.Groups[1].Value));
        Assert.Equal(["Mon", "Tue", "Wed", "Thu", "Fri", "Sat"], Tick().Matches(activities).Select(static match => match.Groups[1].Value));
        Assert.Contains("pictor-d1", activities, StringComparison.Ordinal);
        Assert.Equal(["2024-07-16", "Running", "380", "Swimming", "420"], Texts(Tooltips(activities)[1]));
    }

    [Fact]
    [Trait("AC", "CHART-08")]
    public async Task ChoosingATimeRangeRedrawsTheDaysAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "chart-area-interactive", "light");
        var before = await page.Locator("#fixture .recharts-xAxis .recharts-cartesian-axis-tick-value").AllTextContentsAsync();
        await page.SelectOptionAsync("#fixture select", new Microsoft.Playwright.SelectOptionValue { Label = "Last 7 days" });
        await page.WaitForSelectorAsync("#fixture select option[value='2'][selected]", new() { State = Microsoft.Playwright.WaitForSelectorState.Attached });
        var after = await page.Locator("#fixture .recharts-xAxis .recharts-cartesian-axis-tick-value").AllTextContentsAsync();
        await page.Context.CloseAsync();

        Assert.Contains("Apr 3", before);
        Assert.Contains("Jun 30", after);
        Assert.DoesNotContain(after, tick => tick.StartsWith("Apr", StringComparison.Ordinal) || tick.StartsWith("May", StringComparison.Ordinal));
    }

    [Fact]
    [Trait("AC", "CHART-09")]
    public async Task TheHeaderTogglesChooseTheSeriesAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "chart-bar-interactive", "light");
        var desktop = await page.Locator("#fixture path.recharts-rectangle[fill='#737373'] >> visible=true").CountAsync();
        await page.Locator("#fixture label:has-text('Mobile')").ClickAsync();
        await page.WaitForSelectorAsync("#fixture input[value='1'][checked]", new() { State = Microsoft.Playwright.WaitForSelectorState.Attached });
        var mobile = await page.Locator("#fixture path.recharts-rectangle[fill='#d4d4d4'] >> visible=true").CountAsync();
        var hidden = await page.Locator("#fixture path.recharts-rectangle[fill='#737373'] >> visible=true").CountAsync();
        var backgrounds = await page.Locator("#fixture [role=radiogroup] label").EvaluateAllAsync<string[]>("labels => labels.map((label) => getComputedStyle(label).backgroundColor)");
        await page.Context.CloseAsync();

        Assert.Equal(91, desktop);
        Assert.Equal(91, mobile);
        Assert.Equal(0, hidden);
        Assert.Equal("rgba(0, 0, 0, 0)", backgrounds[0]);
        Assert.NotEqual("rgba(0, 0, 0, 0)", backgrounds[1]);
    }

    [Fact]
    [Trait("AC", "CHART-11")]
    public async Task ConvertersAndAChartConfigShapeEveryLineOfTheTooltipAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/chart", TestContext.Current.CancellationToken);
        var example = html[html.IndexOf("Tooltip - Formatters", StringComparison.Ordinal)..];
        example = example[..example.IndexOf("</svg>", example.IndexOf("recharts-surface", StringComparison.Ordinal), StringComparison.Ordinal)];
        var tips = Tooltips(example);
        string[] browsers = ["Chrome", "Safari", "Firefox", "Edge", "Other"];

        Assert.Equal(6, tips.Count);
        Assert.Equal(["January 2024", "Chrome", "220 visitors", "Safari", "160 visitors", "Firefox", "150 visitors", "Edge", "138 visitors", "Other", "72 visitors"], Texts(tips[0]));
        Assert.Equal(["June 2024", .. browsers.SelectMany(static browser => new[] { browser, string.Empty })], Texts(tips[5]).Select((text, index) => index > 0 && index % 2 == 0 ? string.Empty : text));
        Assert.Equal(["#d4d4d4", "#737373", "#525252", "#404040", "#262626"], Regex.Matches(tips[0], "border:1.5px dashed (#[0-9a-f]{6})", RegexOptions.None, TimeSpan.FromSeconds(1)).Select(static match => match.Groups[1].Value));
        Assert.Equal(["Jan", "Feb", "Mar", "Apr", "May", "Jun"], Tick().Matches(example).Select(static match => match.Groups[1].Value));
    }

    [GeneratedRegex("<p style=\"[^\"]*\">([^<]*)</p>")]
    private static partial Regex Paragraph();

    [GeneratedRegex("width:10px;height:10px;background:(#[0-9a-f]{6})")]
    private static partial Regex Square();

    [GeneratedRegex("recharts-cartesian-axis-tick-value\"[^>]*><tspan[^>]*>([^<]*)</tspan>")]
    private static partial Regex Tick();

    [GeneratedRegex("class=\"recharts-text recharts-label\"[^>]*><tspan[^>]*>([^<]*)</tspan>")]
    private static partial Regex LabelText();
}
