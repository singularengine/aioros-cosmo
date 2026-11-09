using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class ButtonWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    [Fact]
    [Trait("AC", "BUTTON-01")]
    public async Task AButtonIsARealButtonElementAsync()
    {
        var cancel = TestContext.Current.CancellationToken;
        var variants = await gallery.Client.GetStringAsync("fidelity/button-variants", cancel);
        var disabled = await gallery.Client.GetStringAsync("fidelity/button-disabled", cancel);
        var sizes = await gallery.Client.GetStringAsync("fidelity/button-sizes", cancel);

        var buttons = ButtonTag().Matches(Fixture(variants)).ToList();
        Assert.Equal(6, buttons.Count);
        Assert.All(buttons, button =>
        {
            Assert.Contains("type=\"submit\" name=\"_btn\"", button.Groups[1].Value, StringComparison.Ordinal);
            Assert.DoesNotContain("disabled", button.Groups[1].Value, StringComparison.Ordinal);
            Assert.Equal("Button", button.Groups[2].Value);
        });
        Assert.All(ButtonTag().Matches(Fixture(disabled)), button => Assert.EndsWith(" disabled", button.Groups[1].Value, StringComparison.Ordinal));
        Assert.Equal(2, Regex.Count(Fixture(sizes), "<button[^>]*aria-label=\"Next\"[^>]*><svg", RegexOptions.None, TimeSpan.FromSeconds(1)));
    }

    private static string Fixture(string html) => html[html.IndexOf("id=\"fixture\"", StringComparison.Ordinal)..];

    [GeneratedRegex(@"<button([^>]*)>([^<]*)</button>")]
    private static partial Regex ButtonTag();

    [Fact]
    [Trait("AC", "BUTTON-03")]
    public async Task EveryVariantIsComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            foreach (var fixture in (string[])["button-variants", "button-disabled"])
            {
                var expected = await ReferenceMetrics.OfAsync(fixture, theme);
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var found = (await Browser.ReadAsync(page)).Buttons;
                Assert.Equal(expected.Count, found.Count);
                wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Differences($"{fixture} {theme} #{i}", pair.First, pair.Second)));
                await page.Context.CloseAsync();
            }
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Differences(string what, ElementMetrics found, ElementMetrics expected) =>
        Compare.Colors(what, found, expected)
            .Concat(new[] { ("radius", found.Radius, expected.Radius), ("font size", found.FontSize, expected.FontSize), ("weight", found.FontWeight, expected.FontWeight), ("opacity", found.Opacity, expected.Opacity) }
                .Where(pair => pair.Item2 != pair.Item3).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"))
            .Concat(Math.Abs(found.Height - expected.Height) > 0.01 ? [$"{what}: height {found.Height}, shadcn {expected.Height}"] : []);

    [Fact]
    [Trait("AC", "BUTTON-04")]
    public async Task EverySizeHasShadcnsBoxAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["button-sizes", "button-extra-sizes", "button-icons"])
        {
            var expected = await ReferenceMetrics.OfAsync(fixture, "light");
            var page = await browser.FixtureAsync(gallery.Address, fixture, "light");
            var found = (await Browser.ReadAsync(page)).Buttons;
            Assert.Equal(expected.Count, found.Count);
            wrong.AddRange(found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{fixture} #{i}", pair.First, pair.Second, 0.5)));
            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BUTTON-16")]
    public async Task ALinksContentFollowsItsAlignmentAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "link-content-alignment", "light");

        var offsets = await page.EvaluateAsync<double[][]>("""
            () => [...document.querySelectorAll("#fixture a")].map((link) => {
              const box = link.getBoundingClientRect();
              const range = document.createRange();
              range.selectNodeContents(link);
              const text = range.getBoundingClientRect();
              return [text.left - box.left, box.right - text.right];
            })
            """);
        await page.Context.CloseAsync();

        Assert.Equal(3, offsets.Length);
        Assert.InRange(offsets[0][0] - offsets[0][1], -1, 1);
        Assert.InRange(offsets[1][0] - offsets[1][1], -1, 1);
        Assert.InRange(offsets[2][0], 15, 17);
        Assert.True(offsets[2][1] > 40, $"the left-aligned link's right offset is {offsets[2][1]}");
    }

    [Fact]
    [Trait("AC", "BUTTON-15")]
    public async Task AnExtraSmallButtonsIconIsTwelvePixelsOnTheWebAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/button", TestContext.Current.CancellationToken);
        var widths = System.Text.RegularExpressions.Regex.Matches(html, "<button[^>]*aria-label=\"Add\"[^>]*>(?:(?!</button>).)*?<svg[^>]* width=\"([0-9]+)\"", System.Text.RegularExpressions.RegexOptions.Singleline).Select(static match => match.Groups[1].Value).ToList();

        Assert.Equal(["12", "16", "16", "16"], widths.Take(4));
    }

    [Fact]
    [Trait("AC", "BUTTON-05")]
    public async Task UnderThePointerEveryVariantIsShadcnsHoverAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.OfAsync("button-variants", theme, "hover");
            var page = await browser.FixtureAsync(gallery.Address, "button-variants", theme);
            var buttons = await page.QuerySelectorAllAsync("#fixture button");
            for (var i = 0; i < buttons.Count; i++)
            {
                await buttons[i].HoverAsync();
                wrong.AddRange(Compare.Colors($"{theme} #{i} hover", (await Browser.ReadAsync(page)).Buttons[i], expected[i]));
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BUTTON-09")]
    public async Task AHyperlinkButtonIsALinkWithTheButtonsLookAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light");
        await page.GotoAsync(new Uri(gallery.Address, "components/button").ToString());
        var styles = await page.EvaluateAsync<System.Text.Json.JsonElement>("""
            (() => {
              const read = (el) => { const s = getComputedStyle(el); return [el.tagName.toLowerCase(), el.getAttribute("href") ?? "", s.backgroundColor, s.color, s.borderTopWidth, s.height, s.paddingLeft, s.borderTopLeftRadius].join(" | ") }
              const button = (text) => [...document.querySelectorAll("button")].find((b) => b.textContent.trim() === text)
              const link = (text) => [...document.querySelectorAll("a")].find((a) => a.textContent.trim() === text)
              return [read(link("Go to Badge")), read(button("Button")), read(link("Go to Card")), read(button("Outline"))]
            })()
            """);
        await page.Context.CloseAsync();
        var values = styles.EnumerateArray().Select(value => value.GetString()!.Split(" | ")).ToList();

        Assert.Equal(["a", "/components/badge"], values[0][..2]);
        Assert.Equal(["a", "/components/card"], values[2][..2]);
        Assert.Equal(values[1][2..], values[0][2..]);
        Assert.Equal(values[3][2..], values[2][2..]);
    }

    [Fact]
    [Trait("AC", "BUTTON-10")]
    public async Task TheKeyboardFocusShowsShadcnsRingOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = await ReferenceMetrics.OfAsync("button-variants", theme, "focus");
            var page = await browser.FixtureAsync(gallery.Address, "button-variants", theme);
            await page.Mouse.MoveAsync(990, 890);
            var buttons = await page.QuerySelectorAllAsync("#fixture button");
            for (var i = 0; i < expected.Count; i++)
            {
                await buttons[i].EvaluateAsync("(b) => b.focus({ focusVisible: true })");
                var found = (await Browser.PickAsync(page, "#fixture button:focus")).Single();
                var outline = await page.EvaluateAsync<string>("getComputedStyle(document.activeElement).outlineStyle");
                if (!CssShadow.Same(CssShadow.List(found.Shadow), CssShadow.List(expected[i].Shadow)) || outline != "none")
                {
                    wrong.Add($"{theme} #{i}: ring {found.Shadow} outline {outline}, shadcn {expected[i].Shadow}");
                }

                if (Compare.Px(expected[i].BorderWidth) > 0 && !CssColor.Parse(found.BorderColor).Near(CssColor.Parse(expected[i].BorderColor)))
                {
                    wrong.Add($"{theme} #{i}: border {found.BorderColor}, shadcn {expected[i].BorderColor}");
                }
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BUTTON-13")]
    public async Task AnInvalidButtonTakesTheDestructiveBorderAndRingOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var rest = await ReferenceMetrics.SlotsAsync("button-invalid", theme, "button");
            var focus = await ReferenceMetrics.OfAsync("button-invalid", theme, "focus");
            var page = await browser.FixtureAsync(gallery.Address, "button-invalid", theme);
            await page.Mouse.MoveAsync(990, 890);
            var found = await Browser.PickAsync(page, "#fixture button");
            Assert.Equal(rest.Count, found.Count);
            var messages = await page.Locator("#fixture [role=alert]").CountAsync();
            wrong.AddRange(messages == 0 ? [] : [$"{theme}: {messages} error messages written"]);
            var buttons = await page.QuerySelectorAllAsync("#fixture button");
            for (var i = 0; i < rest.Count; i++)
            {
                wrong.AddRange(Border($"{theme} #{i} rest", found[i], rest[i]));
                wrong.AddRange(Compare.Box($"{theme} #{i}", found[i], rest[i], 1));
                await buttons[i].EvaluateAsync("(b) => b.focus({ focusVisible: true })");
                var focused = (await Browser.PickAsync(page, "#fixture button:focus")).Single();
                wrong.AddRange(Border($"{theme} #{i} focus", focused, focus[i]));
                wrong.AddRange(CssShadow.Same(CssShadow.List(focused.Shadow), CssShadow.List(focus[i].Shadow)) ? [] : [$"{theme} #{i}: ring {focused.Shadow}, shadcn {focus[i].Shadow}"]);
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Border(string what, ElementMetrics found, ElementMetrics expected) =>
        found.BorderWidth != expected.BorderWidth ? [$"{what}: border width {found.BorderWidth}, shadcn {expected.BorderWidth}"]
        : Compare.Px(expected.BorderWidth) > 0 && !CssColor.Parse(found.BorderColor).Near(CssColor.Parse(expected.BorderColor)) ? [$"{what}: border {found.BorderColor}, shadcn {expected.BorderColor}"]
        : [];

    [Fact]
    [Trait("AC", "BUTTON-11")]
    public async Task ButtonsTransitionAndTheLinkIsUnderlinedUnderThePointerAsync()
    {
        var expected = await ReferenceMetrics.OfAsync("button-variants", "light");
        var page = await browser.PageAsync(gallery.Address, "light");
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/button-variants").ToString());
        var transitions = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture button')].map((b) => { const s = getComputedStyle(b); return s.transitionProperty + ' ' + s.transitionDuration + ' ' + s.transitionTimingFunction })");
        var shadow = (await Browser.PickAsync(page, "#fixture button"))[2].Shadow;
        var link = page.Locator("#fixture button").Nth(5);
        var rest = await link.EvaluateAsync<string>("(b) => getComputedStyle(b).textDecorationLine");
        await link.HoverAsync();
        var hovered = await link.EvaluateAsync<string>("(b) => getComputedStyle(b).textDecorationLine");
        await page.Context.CloseAsync();

        Assert.All(transitions, transition => Assert.Equal("background-color, border-color, color, box-shadow 0.15s, 0.15s, 0.15s, 0.15s cubic-bezier(0.4, 0, 0.2, 1), cubic-bezier(0.4, 0, 0.2, 1), cubic-bezier(0.4, 0, 0.2, 1), cubic-bezier(0.4, 0, 0.2, 1)", transition));
        Assert.True(CssShadow.Same(CssShadow.List(shadow), CssShadow.List(expected[2].Shadow)), shadow);
        Assert.Equal(("none", "underline"), (rest, hovered));
    }
}
