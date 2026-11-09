using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class FieldWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "FIELD-02")]
    public async Task TheFieldPartsAreComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var reference = await ReferenceMetrics.OfAsync("field-form", theme);
            var page = await browser.FixtureAsync(gallery.Address, "field-form", theme);
            var inputs = await Browser.PickAsync(page, "#fixture input:not([type=checkbox])");
            var labels = await Browser.PickAsync(page, "#fixture label:not(:has(input))");
            var texts = await Browser.PickAsync(page, "#fixture div:not(:has(*)), #fixture legend");
            var alert = await page.EvaluateAsync<string>("(() => { const f = document.querySelector('#fixture [aria-invalid=true]'); const a = f && document.getElementById(f.getAttribute('aria-describedby'))?.querySelector('[role=alert]'); return a ? f.tagName + ' ' + a.textContent : '' })()");
            await page.Context.CloseAsync();

            var expectedInputs = reference.Where(element => element.Slot == "input").ToList();
            Assert.Equal(expectedInputs.Count, inputs.Count);
            wrong.AddRange(inputs.Zip(expectedInputs).SelectMany((pair, i) => Compare.Box($"{theme} input #{i}", pair.First, pair.Second, 1)));
            var expectedLabels = reference.Where(element => element.Slot == "field-label").ToList();
            Assert.Equal(expectedLabels.Count, labels.Count);
            wrong.AddRange(labels.Zip(expectedLabels).SelectMany((pair, i) => Text($"{theme} label #{i}", pair.First, pair.Second)));
            wrong.AddRange(reference.Where(element => element.Slot is "field-description" or "field-legend").SelectMany(expected => Shown(theme, texts, expected)));

            var error = reference.Single(element => element.Slot == "field-error");
            var shown = texts.FirstOrDefault(text => text.Text == error.Text);
            if (shown is null || !CssColor.Parse(shown.Color).Near(CssColor.Parse(error.Color)) || shown.FontSize != error.FontSize || shown.LineHeight != error.LineHeight || Math.Abs(shown.Y - error.Y) > 1)
            {
                wrong.Add($"{theme}: error {shown?.Color} {shown?.FontSize}/{shown?.LineHeight} at {shown?.Y}, shadcn {error.Color} {error.FontSize}/{error.LineHeight} at {error.Y}");
            }

            if (alert != "INPUT " + error.Text)
            {
                wrong.Add($"{theme}: alert '{alert}'");
            }
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "FIELD-10")]
    public async Task AFieldSetIsAFieldsetWithItsLegendAsync()
    {
        var wrong = new List<string>();
        var markup = await gallery.Client.GetStringAsync("fidelity/field-form", TestContext.Current.CancellationToken);
        wrong.AddRange(Legend().IsMatch(markup) ? [] : ["no <fieldset> starting with its <legend>Notifications</legend>"]);
        foreach (var theme in (string[])["light", "dark"])
        {
            var reference = await ReferenceMetrics.OfAsync("field-form", theme);
            var page = await browser.FixtureAsync(gallery.Address, "field-form", theme);
            var found = await Browser.PickAsync(page, "#fixture fieldset > legend, #fixture fieldset > div:not(:has(*)), #fixture fieldset input[type=checkbox]");
            await page.Context.CloseAsync();
            var expected = reference.Where(element => element.Slot is "field-legend" or "checkbox" || (element.Slot == "field-description" && element.Text == "Choose what we email you about.")).ToList();
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany(pair => new[] { ("x", pair.First.X, pair.Second.X), ("y", pair.First.Y, pair.Second.Y) }
                .Where(edge => Math.Abs(edge.Item2 - edge.Item3) > 1).Select(edge => $"{theme} {pair.Second.Slot} {pair.Second.Text}: {edge.Item1} {edge.Item2:0.##}, shadcn {edge.Item3:0.##}")));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "FIELD-11")]
    public async Task AFieldContentTakesTheRowItsControlLeavesOnTheWebAsync() =>
        Assert.Empty(await PartsAsync("field-content", "#fixture input[type=checkbox]:not([role=switch])", "#fixture input[role=switch]"));

    [Fact]
    [Trait("AC", "FIELD-12")]
    public async Task AResponsiveFieldTurnsAtItsGroupsWidthOnTheWebAsync() =>
        Assert.Empty(await PartsAsync("field-responsive", "", ""));

    private async Task<List<string>> PartsAsync(string fixture, string checkbox, string toggle)
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var reference = await ReferenceMetrics.OfAsync(fixture, theme);
            var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
            var found = new Dictionary<string, IReadOnlyList<ElementMetrics>>(StringComparer.Ordinal)
            {
                ["field-label"] = await Browser.PickAsync(page, "#fixture label:not(:has(input)), #fixture div:not(:has(*)):not(:empty)"),
                ["input"] = await Browser.PickAsync(page, "#fixture input:not([type=checkbox])"),
                ["checkbox"] = checkbox.Length > 0 ? await Browser.PickAsync(page, checkbox) : [],
                ["switch"] = toggle.Length > 0 ? await Browser.PickAsync(page, toggle) : [],
            };
            await page.Context.CloseAsync();
            foreach (var slot in (string[])["field-label", "field-description", "input", "checkbox", "switch"])
            {
                var expected = reference.Where(element => element.Slot == slot).ToList();
                var ours = slot is "field-label" or "field-description"
                    ? [.. expected.SelectMany((element, i) => found["field-label"].Where(text => text.Text == element.Text).Skip(expected.Take(i).Count(before => before.Text == element.Text)).Take(1))]
                    : found[slot].ToList();
                if (ours.Count != expected.Count)
                {
                    wrong.Add($"{theme} {slot}: {ours.Count} parts, shadcn {expected.Count}");
                    continue;
                }

                wrong.AddRange(ours.Zip(expected).SelectMany(pair => Compare.Box($"{theme} {slot} {pair.Second.Text}", pair.First, pair.Second, 1.5)));
            }
        }

        return wrong;
    }

    [Fact]
    [Trait("AC", "FIELD-14")]
    public async Task AHiddenPartLeavesNoGapOnTheWebAsync()
    {
        const string Gap = "() => document.querySelector('label[for=updatesname]').getBoundingClientRect().top - document.getElementById('updatesmail').getBoundingClientRect().bottom";
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/field").ToString());
        var shown = await page.EvaluateAsync<double>(Gap);
        await page.Locator("#updates").ClickAsync();
        await page.ClickAsync("main button:text-is('Save')");
        await page.WaitForSelectorAsync("text=Product updates off");
        await page.WaitForFunctionAsync("() => !document.getElementById('updates').checked && [...document.querySelectorAll('main div')].every((part) => part.textContent !== 'We send one email a month.' || part.getClientRects().length === 0)");
        var hidden = await page.EvaluateAsync<double>(Gap);
        await page.Context.CloseAsync();

        Assert.InRange(hidden, 27, 29);
        Assert.InRange(shown - hidden, 32, 34);
    }

    [GeneratedRegex("<fieldset[^>]*><legend[^>]*>Notifications</legend>")]
    private static partial Regex Legend();

    private static IEnumerable<string> Shown(string theme, IReadOnlyList<ElementMetrics> texts, ElementMetrics expected) =>
        texts.FirstOrDefault(text => text.Text == expected.Text) is { } found ? Text($"{theme} {expected.Slot} {expected.Text}", found, expected) : [$"{theme}: no {expected.Slot} {expected.Text}"];

    private static IEnumerable<string> Text(string what, ElementMetrics found, ElementMetrics expected) =>
        InputWebTests.Fonts(what, found, expected)
            .Concat(CssColor.Parse(found.Color).Near(CssColor.Parse(expected.Color)) ? [] : [$"{what}: color {found.Color}, shadcn {expected.Color}"])
            .Concat(Math.Abs(found.X - expected.X) > 0.5 ? [$"{what}: x {found.X}, shadcn {expected.X}"] : []);

    [Fact]
    [Trait("AC", "FIELD-04")]
    public async Task APostShowsEachErrorUnderItsFieldAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "examples/forms").ToString());
        await page.FillAsync("#form-title", "abc");
        await page.FillAsync("#form-email", "nope");
        await page.ClickAsync("button:text-is('Submit')");
        await page.WaitForSelectorAsync("text=Fix the errors above");
        var terms = await page.Locator("text=Accept the terms to send the report.").CountAsync();
        var invalid = await page.EvaluateAsync<string[]>("""
            [...document.querySelectorAll("[aria-invalid=true][aria-describedby]")].filter((field) => field.id).map((field) => {
              const alert = document.getElementById(field.getAttribute("aria-describedby")).querySelector("[role=alert]"), label = document.querySelector(`label[for="${field.id}"]`)
              return [field.id, alert.textContent, getComputedStyle(field).borderTopColor, getComputedStyle(label).color].join(" | ")
            })
            """);
        await page.FillAsync("#form-title", "Login button broken");
        await page.FillAsync("#form-email", "ada@example.com");
        await page.CheckAsync("label:has-text('I accept the terms') input");
        await page.ClickAsync("button:text-is('Submit')");
        await page.WaitForSelectorAsync("text=Saved Login button broken for ada@example.com on Pro");
        var left = await page.Locator("[role=alert]").CountAsync() + await page.Locator("text=Accept the terms to send the report.").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(
            ["form-title | Title must be at least 5 characters. | rgb(231, 0, 11) | rgb(231, 0, 11)", "form-email | Enter a valid email address. | rgb(231, 0, 11) | rgb(231, 0, 11)"],
            invalid);
        Assert.Equal(1, terms);
        Assert.Equal(0, left);
    }

    [Fact]
    [Trait("AC", "FIELD-16")]
    public async Task ADescriptionBeforeTheLastPartStandsCloserToItsTitleAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var reference = await ReferenceMetrics.OfAsync("slider-range", theme);
            var title = reference.Single(element => element.Slot == "field-label");
            var description = reference.Single(element => element.Slot == "field-description");
            var page = await browser.FixtureAsync(gallery.Address, "slider-range", theme);
            var texts = await Browser.PickAsync(page, "#fixture div");
            await page.Context.CloseAsync();
            var foundTitle = texts.Single(element => element.Text == title.Text);
            var foundDescription = texts.Single(element => element.Text == description.Text);
            wrong.AddRange(Compare.Box($"{theme} description", foundDescription with { X = foundDescription.X - foundTitle.X, Y = foundDescription.Y - foundTitle.Y }, description with { X = description.X - title.X, Y = description.Y - title.Y }, 1));
        }

        Assert.Empty(wrong);
    }
}
