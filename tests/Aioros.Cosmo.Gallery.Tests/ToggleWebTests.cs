using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class ToggleWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    private async Task<string> FixtureAsync(string name)
    {
        var html = await gallery.Client.GetStringAsync("fidelity/" + name, TestContext.Current.CancellationToken);
        return html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];
    }

    [Fact]
    [Trait("AC", "RADIO-01")]
    public async Task RadioButtonsAreRadioInputsSharingOneNameAsync()
    {
        var radios = RadioTag().Matches(await FixtureAsync("radio-group")).ToList();

        Assert.Equal(3, radios.Count);
        Assert.Single(radios.Select(radio => Regex.Match(radio.Groups[1].Value, "name=\"(f\\d+)\"", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value).Distinct());
        Assert.Equal(["0", "1", "2"], radios.Select(radio => Regex.Match(radio.Groups[1].Value, "value=\"(\\d)\"", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value));
        Assert.Equal(["Default", "Comfortable", "Compact"], radios.Select(radio => radio.Groups[2].Value));
        Assert.Equal([false, true, false], radios.Select(radio => radio.Groups[1].Value.Contains(" checked", StringComparison.Ordinal)));
        Assert.Equal([false, false, true], radios.Select(radio => radio.Groups[1].Value.Contains(" disabled", StringComparison.Ordinal)));
    }

    [Fact]
    [Trait("AC", "RADIO-02")]
    public async Task EveryRadioTextAndCircleAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var labels = await ReferenceMetrics.SlotsAsync("radio-group", theme, "label");
            var circles = await ReferenceMetrics.SlotsAsync("radio-group", theme, "radio-group-item");
            var page = await browser.FixtureAsync(gallery.Address, "radio-group", theme);
            var texts = await Browser.PickAsync(page, "#fixture label");
            var inputs = await Browser.PickAsync(page, "#fixture input");
            var dots = await page.EvaluateAsync<string?[]>("[...document.querySelectorAll('#fixture input')].map((input) => decodeURIComponent(getComputedStyle(input).backgroundImage).match(/fill='([^']*)'/)?.[1] ?? null)");
            await page.Context.CloseAsync();
            wrong.AddRange(texts.Zip(labels).SelectMany(pair => Text($"{theme} {pair.Second.Text}", pair.First, pair.Second)));
            wrong.AddRange(inputs.Select((input, i) => (input, i)).Where(pair => !CssColor.Parse(pair.input.BorderColor).Near(CssColor.Parse(circles[pair.i].BorderColor)) || Compare.Px(pair.input.Radius) < pair.input.Width / 2).Select(pair => $"{theme} #{pair.i}: border {pair.input.BorderColor} radius {pair.input.Radius}, shadcn {circles[pair.i].BorderColor}"));
            wrong.AddRange(inputs.Zip(circles).SelectMany((pair, i) => Compare.Box($"{theme} circle #{i}", pair.First, pair.Second, 1)));
            wrong.AddRange(dots.Length == 3 && dots[0] is null && dots[2] is null && dots[1] is { } dot && CssColor.Parse(dot).Near(CssColor.Parse(circles[1].Color)) ? [] : [$"{theme}: dots {string.Join(',', dots)}, shadcn {circles[1].Color}"]);
            wrong.AddRange(texts[2].Opacity == "0.5" ? [] : [$"{theme}: disabled radio {texts[2].Opacity}"]);
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Text(string what, ElementMetrics found, ElementMetrics expected) =>
        InputWebTests.Fonts(what, found, expected).Concat(CssColor.Parse(found.Color).Near(CssColor.Parse(expected.Color)) ? [] : [$"{what}: color {found.Color}, shadcn {expected.Color}"]);

    [Fact]
    [Trait("AC", "RADIO-05")]
    public async Task APostBindsTheCheckedRadioAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/radio-group").ToString());
        var binding = page.Locator("div.preview, div").Filter(new() { HasText = "Apply" }).Last;
        await page.Locator("label:has-text('Compact') input").Last.CheckAsync();
        await page.ClickAsync("button:text-is('Apply')");
        await page.WaitForSelectorAsync("text=Density: Compact");
        var kept = await page.Locator("label:has-text('Compact') input").Last.IsCheckedAsync();
        await page.Context.CloseAsync();

        Assert.NotNull(binding);
        Assert.True(kept);
    }

    [Fact]
    [Trait("AC", "SWITCH-01")]
    public async Task ASwitchIsASwitchCheckboxWithItsLabelAsync()
    {
        var fixture = await FixtureAsync("switch-states");
        var switches = SwitchTag().Matches(fixture).Select(match => match.Groups[1].Value).ToList();

        Assert.Equal(4, switches.Count);
        Assert.Equal(["airplane", "wifi", "locked", "small"], switches.Select(attributes => Regex.Match(attributes, "id=\"([a-z]+)\"", RegexOptions.None, TimeSpan.FromSeconds(1)).Groups[1].Value));
        Assert.All(switches, attributes => Assert.Matches(" name=\"f\\d+\"", attributes));
        Assert.Equal([false, true, false, true], switches.Select(attributes => attributes.Contains(" checked", StringComparison.Ordinal)));
        Assert.Equal([false, false, true, false], switches.Select(attributes => attributes.Contains(" disabled", StringComparison.Ordinal)));
        Assert.Contains("<label for=\"airplane\"", fixture, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "SWITCH-02")]
    public async Task EverySwitchTrackAndLabelAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var tracks = await ReferenceMetrics.SlotsAsync("switch-states", theme, "switch");
            var labels = await ReferenceMetrics.SlotsAsync("switch-states", theme, "label");
            var page = await browser.FixtureAsync(gallery.Address, "switch-states", theme);
            var inputs = await Browser.PickAsync(page, "#fixture input");
            var texts = await Browser.PickAsync(page, "#fixture label[for]");
            await page.Context.CloseAsync();
            wrong.AddRange(inputs.Select((input, i) => (input, i)).Where(pair => !CssColor.Parse(pair.input.Background).Near(CssColor.Parse(tracks[pair.i].Background)) || Math.Abs(pair.input.Width - tracks[pair.i].Width) > 1 || Math.Abs(pair.input.Height - tracks[pair.i].Height) > 1).Select(pair => $"{theme} #{pair.i}: track {pair.input.Background} {pair.input.Width}x{pair.input.Height}, shadcn {tracks[pair.i].Background} {tracks[pair.i].Width}x{tracks[pair.i].Height}"));
            wrong.AddRange(texts.Zip(labels).SelectMany(pair => Text($"{theme} {pair.Second.Text}", pair.First, pair.Second)));
            var disabled = await DisabledOpacityAsync("switch-states", theme);
            wrong.AddRange(disabled == "0.5" ? [] : [$"{theme}: disabled switch opacity {disabled}"]);
        }

        Assert.Empty(wrong);
    }

    private async Task<string> DisabledOpacityAsync(string fixture, string theme)
    {
        var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
        var opacity = await page.EvaluateAsync<string>("getComputedStyle(document.querySelector('#fixture input[disabled]').closest('label')).opacity");
        await page.Context.CloseAsync();
        return opacity;
    }

    [Fact]
    [Trait("AC", "SWITCH-05")]
    public async Task APostBindsTheSwitchAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/switch").ToString());
        await page.CheckAsync("#mode");
        await page.ClickAsync("button:text-is('Save')");
        await page.WaitForSelectorAsync("text=Airplane mode on");
        var kept = await page.IsCheckedAsync("#mode");
        await page.Context.CloseAsync();

        Assert.True(kept);
    }

    [Fact]
    [Trait("AC", "TOGGLE-01")]
    public async Task AToggleButtonIsALabelWithAHiddenCheckboxAsync()
    {
        var toggles = ToggleTag().Matches(await FixtureAsync("toggle-states")).ToList();

        Assert.Equal(7, toggles.Count);
        Assert.All(toggles, toggle => Assert.Matches(" name=\"f\\d+\"", toggle.Groups[1].Value));
        Assert.Equal([false, true, false, false, false, false, false], toggles.Select(toggle => toggle.Groups[1].Value.Contains(" checked", StringComparison.Ordinal)));
        Assert.Equal([false, false, false, false, false, true, false], toggles.Select(toggle => toggle.Groups[1].Value.Contains(" disabled", StringComparison.Ordinal)));
        Assert.All(toggles, toggle => Assert.Contains("<svg", toggle.Groups[2].Value, StringComparison.Ordinal));
        Assert.Contains(">Favorite<", toggles[6].Groups[2].Value, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "TOGGLE-02")]
    public async Task EveryToggleIsComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("toggle-states", theme, "toggle");
            var hover = await ReferenceMetrics.SlotsAsync("toggle-states", theme, "toggle", "hover");
            var page = await browser.FixtureAsync(gallery.Address, "toggle-states", theme);
            var found = await Browser.PickAsync(page, "#fixture > div > label");
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Compare.Colors($"{theme} #{i}", pair.First, pair.Second)
                .Concat(Compare.Box($"{theme} #{i}", pair.First, pair.Second, 0.5))
                .Concat(InputWebTests.Fonts($"{theme} #{i}", pair.First, pair.Second))
                .Concat(pair.First.Radius == pair.Second.Radius && pair.First.Opacity == pair.Second.Opacity ? [] : [$"{theme} #{i}: radius {pair.First.Radius} opacity {pair.First.Opacity}, shadcn {pair.Second.Radius} {pair.Second.Opacity}"])));
            var labels = await page.QuerySelectorAllAsync("#fixture > div > label");
            for (var i = 0; i < labels.Count; i++)
            {
                if (i == 5)
                {
                    continue;
                }

                await labels[i].HoverAsync();
                var hovered = (await Browser.PickAsync(page, "#fixture > div > label"))[i];
                wrong.AddRange(new[] { ("background", hovered.Background, hover[i].Background), ("color", hovered.Color, hover[i].Color) }
                    .Where(pair => !CssColor.Parse(pair.Item2).Near(CssColor.Parse(pair.Item3))).Select(pair => $"{theme} #{i} hover: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"));
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOGGLEGROUP-01")]
    public async Task AToggleGroupIsAGroupOfLabelledChoicesAsync()
    {
        var fixture = await FixtureAsync("toggle-group");
        var groups = GroupTag().Matches(fixture).ToList();

        Assert.Equal(["group", "radiogroup", "radiogroup"], groups.Select(group => group.Groups[1].Value));
        var inputs = groups.Select(group => Regex.Matches(group.Groups[2].Value, "<input type=\"(checkbox|radio)\"[^>]*name=\"(f\\d+)\"[^>]*value=\"(\\d)\"([^>]*)>", RegexOptions.None, TimeSpan.FromSeconds(1)).ToList()).ToList();
        Assert.All(inputs, items => Assert.Equal(3, items.Count));
        Assert.All(inputs, items => Assert.Single(items.Select(item => item.Groups[2].Value).Distinct()));
        Assert.Equal(["checkbox", "radio", "radio"], inputs.Select(items => items[0].Groups[1].Value));
        Assert.Equal(["1", "1", "0"], inputs.Select(items => items.Single(item => item.Groups[4].Value.Contains("checked", StringComparison.Ordinal)).Groups[3].Value));
        Assert.Contains(">Center</label>", groups[1].Groups[2].Value, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "TOGGLEGROUP-02")]
    public async Task TheToggleGroupItemsAreComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("toggle-group", theme, "toggle-group-item");
            var hover = await ReferenceMetrics.SlotsAsync("toggle-group", theme, "toggle-group-item", "hover");
            var page = await browser.FixtureAsync(gallery.Address, "toggle-group", theme);
            var found = await Browser.PickAsync(page, "#fixture [role] > label");
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => InputWebTests.Fonts($"{theme} #{i}", pair.First, pair.Second)
                .Concat(Compare.Box($"{theme} #{i}", pair.First, pair.Second, 1))
                .Concat((i != 0 || pair.First.Radius == pair.Second.Radius) && pair.First.BorderWidth == pair.Second.BorderWidth ? [] : [$"{theme} #{i}: radius {pair.First.Radius} border {pair.First.BorderWidth}, shadcn {pair.Second.Radius} {pair.Second.BorderWidth}"])
                .Concat(new[] { ("background", pair.First.Background, pair.Second.Background), ("color", pair.First.Color, pair.Second.Color) }
                    .Where(colors => !CssColor.Parse(colors.Item2).Near(CssColor.Parse(colors.Item3))).Select(colors => $"{theme} #{i}: {colors.Item1} {colors.Item2}, shadcn {colors.Item3}"))));
            var labels = await page.QuerySelectorAllAsync("#fixture [role] > label");
            for (var i = 0; i < labels.Count; i++)
            {
                await labels[i].HoverAsync();
                var hovered = (await Browser.PickAsync(page, "#fixture [role] > label"))[i];
                wrong.AddRange(CssColor.Parse(hovered.Background).Near(CssColor.Parse(hover[i].Background)) ? [] : [$"{theme} #{i} hover: background {hovered.Background}, shadcn {hover[i].Background}"]);
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOGGLEGROUP-05")]
    public async Task APostBindsTheSelectedItemAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/toggle-group").ToString());
        await page.Locator("[role=radiogroup]").Last.Locator("label:has-text('Right')").ClickAsync();
        await page.ClickAsync("button:text-is('Apply')");
        await page.WaitForSelectorAsync("text=Aligned Right");
        var kept = await page.Locator("[role=radiogroup]").Last.Locator("label:has-text('Right') input").IsCheckedAsync();
        await page.Context.CloseAsync();

        Assert.True(kept);
    }

    [GeneratedRegex("<label[^>]*><input type=\"radio\"([^>]*)>([^<]*)</label>")]
    private static partial Regex RadioTag();

    [GeneratedRegex("<input type=\"checkbox\" role=\"switch\"([^>]*)>")]
    private static partial Regex SwitchTag();

    [GeneratedRegex("<label class=\"[^\"]*\"><input type=\"checkbox\"([^>]*)>(.*?)</label>", RegexOptions.Singleline)]
    private static partial Regex ToggleTag();

    [GeneratedRegex("<div role=\"(group|radiogroup)\"[^>]*>(.*?)</div>", RegexOptions.Singleline)]
    private static partial Regex GroupTag();
}
