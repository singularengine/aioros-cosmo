using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class InputWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    [Fact]
    [Trait("AC", "LABEL-01")]
    public async Task ALabelIsALabelElementForItsTargetAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("input-states", theme, "label")).Single();
            var page = await browser.FixtureAsync(gallery.Address, "input-states", theme);
            var found = (await Browser.PickAsync(page, "#fixture label")).Single();
            var target = await page.GetAttributeAsync("#fixture label", "for");
            await page.Context.CloseAsync();
            wrong.AddRange(Compare.Colors($"{theme} label", found, expected));
            wrong.AddRange(Compare.Box($"{theme} label", found, expected, 0.5));
            wrong.AddRange(Fonts($"{theme} label", found, expected));
            if (target != "email")
            {
                wrong.Add($"{theme}: the label is for {target}");
            }
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "LABEL-05")]
    public async Task ALabelsLinesAreFourteenPixelsHighAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("input-file", theme, "label")).Single();
            var page = await browser.FixtureAsync(gallery.Address, "input-file", theme);
            var found = (await Browser.PickAsync(page, "#fixture label[for]")).Single();
            await page.Context.CloseAsync();
            wrong.AddRange(Compare.Box($"{theme} label", found, expected, 1).Where(error => !error.Contains(": width ", StringComparison.Ordinal)));
            wrong.AddRange(found.LineHeight == "14px" ? [] : [$"{theme}: line height {found.LineHeight}"]);
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUT-10")]
    public async Task AFilePickersTextsAreTranslatedOnTheWebAsync()
    {
        var file = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}", "photo.png");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file)!);
        await File.WriteAllBytesAsync(file, [1, 2, 3], TestContext.Current.CancellationToken);
        var page = await browser.FixtureAsync(gallery.Address, "input-file-texts", "light");
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        const string Texts = "() => [...document.querySelectorAll('#fixture label:has(input[type=file])')].map((label) => [...label.querySelectorAll('*')].filter((e) => !e.children.length && e.textContent.trim()).map((e) => e.textContent).join('|'))";

        var shown = await page.EvaluateAsync<string[]>(Texts);
        await page.Locator("#fixture input[type=file]").First.SetInputFilesAsync(file);
        var chosen = await page.EvaluateAsync<string[]>(Texts);
        await page.Context.CloseAsync();

        Assert.Equal(["Elegir archivo|Ningún archivo", "Choose File|No file chosen"], shown);
        Assert.Equal("Elegir archivo|photo.png", chosen[0]);
        Assert.Equal(0, requests);
    }

    [Fact]
    [Trait("AC", "INPUT-08")]
    public async Task AFilePickerIsAFileInputWithShadcnsLookAsync()
    {
        var file = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}", "photo.png");
        Directory.CreateDirectory(System.IO.Path.GetDirectoryName(file)!);
        await File.WriteAllBytesAsync(file, [1, 2, 3], TestContext.Current.CancellationToken);
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("input-file", theme, "input")).Single();
            var page = await browser.FixtureAsync(gallery.Address, "input-file", theme);
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            var picker = (await Browser.PickAsync(page, "#fixture label:has(input[type=file])")).Single();
            var texts = await page.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture label:has(input[type=file]) *')].filter((e) => !e.children.length && e.textContent.trim()).map((e) => e.textContent.trim()).join('|')");
            await page.Locator("#fixture input[type=file]").SetInputFilesAsync(file);
            var chosen = await page.TextContentAsync("#fixture [data-aioros-file-name]");
            await page.Context.CloseAsync();
            wrong.AddRange(Compare.Box($"{theme} picker", picker, expected, 1));
            wrong.AddRange(Compare.Colors($"{theme} picker", picker, expected));
            wrong.AddRange(texts == "Choose File|No file chosen" && chosen == "photo.png" && requests == 0 ? [] : [$"{theme}: texts {texts}, chosen {chosen}, {requests} requests"]);
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "LABEL-04")]
    public async Task ABoundLabelIsWrittenWithItsFormattedValueAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/input", TestContext.Current.CancellationToken);

        Assert.Matches("<label[^>]*>Name \\(3 unread\\)</label>", html);
    }

    [Fact]
    [Trait("AC", "INPUT-01")]
    public async Task ATextBoxIsAnInputElementAsync()
    {
        var cancel = TestContext.Current.CancellationToken;
        var fixture = Fixture(await gallery.Client.GetStringAsync("fidelity/input-states", cancel));
        var password = await gallery.Client.GetStringAsync("components/input", cancel);
        var inputs = InputTag().Matches(fixture).Select(match => match.Groups[1].Value).ToList();

        Assert.Equal(3, inputs.Count);
        Assert.All(inputs, input => Assert.Matches(" name=\"f\\d+\"", input));
        Assert.Contains("type=\"email\"", inputs[0], StringComparison.Ordinal);
        Assert.Contains("placeholder=\"Email\"", inputs[0], StringComparison.Ordinal);
        Assert.Contains("value=\"shadcn@example.com\"", inputs[1], StringComparison.Ordinal);
        Assert.Contains(" disabled", inputs[2], StringComparison.Ordinal);
        Assert.DoesNotContain(" disabled", inputs[0], StringComparison.Ordinal);
        Assert.Matches("<input type=\"password\"[^>]*placeholder=\"Password\"", password);
    }

    [Fact]
    [Trait("AC", "INPUT-02")]
    public async Task EveryInputIsComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("input-states", theme, "input");
            var page = await browser.FixtureAsync(gallery.Address, "input-states", theme);
            var found = await Browser.PickAsync(page, "#fixture input");
            var placeholder = await page.EvaluateAsync<string>("getComputedStyle(document.querySelector('#fixture input'), '::placeholder').color");
            await page.Context.CloseAsync();
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Field($"{theme} #{i}", pair.First, pair.Second)));
            if (!CssColor.Parse(placeholder).Near(CssColor.Parse(Muted(theme))))
            {
                wrong.Add($"{theme}: placeholder {placeholder}");
            }
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUT-03")]
    public async Task AFocusedInputTakesShadcnsRingBorderAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("input-states", theme, "input", "focus");
            var page = await browser.FixtureAsync(gallery.Address, "input-states", theme);
            var inputs = await page.QuerySelectorAllAsync("#fixture input:not([disabled])");
            for (var i = 0; i < inputs.Count; i++)
            {
                await inputs[i].FocusAsync();
                var found = (await Browser.PickAsync(page, "#fixture input:focus")).Single();
                if (!CssColor.Parse(found.BorderColor).Near(CssColor.Parse(expected[i].BorderColor)) || !CssShadow.Same(CssShadow.List(found.Shadow), CssShadow.List(expected[i].Shadow)))
                {
                    wrong.Add($"{theme} #{i}: focused border {found.BorderColor} ring {found.Shadow}, shadcn {expected[i].BorderColor} {expected[i].Shadow}");
                }
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "INPUT-06")]
    public async Task APostBindsTheFieldAndRunsTheCommandAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light");
        await page.GotoAsync(new Uri(gallery.Address, "components/input").ToString());
        await page.FillAsync("input[aria-label='Name']", "Ada");
        await page.ClickAsync("button:text-is('Greet')");
        await page.WaitForSelectorAsync("text=Hello, Ada");
        var kept = await page.InputValueAsync("input[aria-label='Name']");
        await page.Context.CloseAsync();

        Assert.Equal("Ada", kept);
    }

    [Fact]
    [Trait("AC", "TEXTAREA-01")]
    public async Task AMultiLineTextBoxIsATextareaComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.SlotsAsync("textarea-states", theme, "textarea");
            var page = await browser.FixtureAsync(gallery.Address, "textarea-states", theme);
            var found = await Browser.PickAsync(page, "#fixture textarea");
            var sizing = await page.EvaluateAsync<string>("getComputedStyle(document.querySelector('#fixture textarea')).fieldSizing");
            await page.Context.CloseAsync();
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Field($"{theme} #{i}", pair.First, pair.Second)));
            if (sizing != "content")
            {
                wrong.Add($"{theme}: field-sizing {sizing}");
            }
        }

        Assert.Empty(wrong);
    }

    private static string Muted(string theme) => theme == "light" ? "#737373" : "#A1A1A1";

    internal static IEnumerable<string> Fonts(string what, ElementMetrics found, ElementMetrics expected) =>
        new[] { ("font size", found.FontSize, expected.FontSize), ("weight", found.FontWeight, expected.FontWeight) }
            .Where(pair => pair.Item2 != pair.Item3).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}");

    /// <summary>The differences of a text field: its colors, box (x, width, height), radius, padding, font and opacity.</summary>
    internal static IEnumerable<string> Field(string what, ElementMetrics found, ElementMetrics expected) =>
        Compare.Colors(what, found, expected)
            .Concat(Compare.Box(what, found, expected, 0.5))
            .Concat(CssShadow.Same(CssShadow.List(found.Shadow), CssShadow.List(expected.Shadow)) ? [] : [$"{what}: shadow {found.Shadow}, shadcn {expected.Shadow}"])
            .Concat(Fonts(what, found, expected))
            .Concat(new[] { ("radius", found.Radius, expected.Radius), ("padding left", found.PaddingLeft, expected.PaddingLeft), ("padding top", found.PaddingTop, expected.PaddingTop), ("opacity", found.Opacity, expected.Opacity) }
                .Where(pair => pair.Item2 != pair.Item3).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"));

    private static string Fixture(string html) => html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];

    [GeneratedRegex("<input([^>]*)>")]
    private static partial Regex InputTag();
}
