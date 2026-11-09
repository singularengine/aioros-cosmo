using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class ChoiceWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "CHECKBOX-01")]
    public async Task ACheckBoxIsACheckboxInputInALabelAsync()
    {
        var html = await gallery.Client.GetStringAsync("fidelity/checkbox-states", TestContext.Current.CancellationToken);
        var fixture = html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];
        var checks = CheckTag().Matches(fixture).ToList();

        Assert.Equal(3, checks.Count);
        Assert.All(checks, check => Assert.Matches(" name=\"f\\d+\"", check.Groups[1].Value));
        Assert.Equal(["Accept terms and conditions", "Enable notifications", "Disabled"], checks.Select(check => check.Groups[2].Value));
        Assert.DoesNotContain(" checked", checks[0].Groups[1].Value, StringComparison.Ordinal);
        Assert.Contains(" checked", checks[1].Groups[1].Value, StringComparison.Ordinal);
        Assert.Contains(" disabled", checks[2].Groups[1].Value, StringComparison.Ordinal);
        Assert.Matches("<input type=\"radio\"[^>]*value=\"null\" checked", fixture);
    }

    [Fact]
    [Trait("AC", "CHECKBOX-02")]
    public async Task EveryCheckBoxTextAndTheIndeterminateBoxAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var labels = await ReferenceMetrics.SlotsAsync("checkbox-states", theme, "label");
            var boxes = await ReferenceMetrics.SlotsAsync("checkbox-states", theme, "checkbox");
            var page = await browser.FixtureAsync(gallery.Address, "checkbox-states", theme);
            var texts = (await Browser.PickAsync(page, "#fixture > div > label, #fixture .tri > label")).Where(text => text.Width > 0).ToList();
            var tri = (await Browser.PickAsync(page, "#fixture .tri > label .box")).Single(box => box.Width > 0);
            var inputs = await Browser.PickAsync(page, "#fixture > div > label > input[type=checkbox]");
            await page.Context.CloseAsync();
            Assert.Equal(labels.Count, texts.Count);
            for (var i = 0; i < texts.Count; i++)
            {
                var expected = labels.Single(label => label.Text == texts[i].Text);
                wrong.AddRange(InputWebTests.Fonts($"{theme} {texts[i].Text}", texts[i], expected));
                wrong.AddRange(CssColor.Parse(texts[i].Color).Near(CssColor.Parse(expected.Color)) ? [] : [$"{theme} {texts[i].Text}: color {texts[i].Color}, shadcn {expected.Color}"]);
                wrong.AddRange(texts[i].Opacity == expected.Opacity ? [] : [$"{theme} {texts[i].Text}: opacity {texts[i].Opacity}, shadcn {expected.Opacity}"]);
            }

            wrong.AddRange(inputs.Zip(new[] { boxes[0], boxes[1], boxes[3] }).SelectMany((pair, i) => Compare.Colors($"{theme} box #{i}", pair.First with { Color = pair.Second.Color }, pair.Second)
                .Concat(pair.First.Width == pair.Second.Width && pair.First.Height == pair.Second.Height && pair.First.Radius == pair.Second.Radius ? [] : [$"{theme} box #{i}: {pair.First.Width}x{pair.First.Height} {pair.First.Radius}, shadcn {pair.Second.Width}x{pair.Second.Height} {pair.Second.Radius}"])));
            wrong.AddRange(Compare.Colors($"{theme} indeterminate box", tri, boxes[2]));
            wrong.AddRange(tri.Radius == boxes[2].Radius ? [] : [$"{theme}: indeterminate radius {tri.Radius}"]);
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "CHECKBOX-05")]
    public async Task APostBindsTheCheckBoxBeforeTheCommandAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/checkbox").ToString());
        await page.CheckAsync("label:has-text('Email me about new features') input");
        await page.ClickAsync("button:text-is('Save')");
        await page.WaitForSelectorAsync("text=Notifications on");
        var kept = await page.IsCheckedAsync("label:has-text('Email me about new features') input");
        await page.Context.CloseAsync();

        Assert.True(kept);
    }

    [GeneratedRegex("<input type=\"checkbox\"([^>]*)>([^<]*)</label>")]
    private static partial Regex CheckTag();
}
