using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class BadgeCardWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "BADGE-01")]
    public async Task EveryBadgeIsComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var expected = (await ReferenceMetrics.OfAsync("badge-variants", theme)).Where(element => element.Tag == "span").ToList();
            var page = await browser.FixtureAsync(gallery.Address, "badge-variants", theme);
            var found = (await Browser.ReadAsync(page)).Badges;
            await page.Context.CloseAsync();
            Assert.Equal(expected.Count, found.Count);
            for (var i = 0; i < found.Count; i++)
            {
                var what = $"badge-variants {theme} #{i}";
                wrong.AddRange(Compare.Colors(what, found[i], expected[i]));
                wrong.AddRange(Compare.Box(what, found[i], expected[i], 0.5));
                if (Compare.Px(found[i].Radius) < found[i].Height / 2)
                {
                    wrong.Add($"{what}: radius {found[i].Radius}");
                }
            }

            var texts = await TextStylesAsync(theme);
            if (texts.Any(text => text != "12px 500"))
            {
                wrong.Add($"badge-variants {theme}: text styles {string.Join(", ", texts)}");
            }
        }

        var icons = (await ReferenceMetrics.OfAsync("badge-icons", "light")).Where(element => element.Tag == "span").ToList();
        var iconPage = await browser.FixtureAsync(gallery.Address, "badge-icons", "light");
        var iconBadges = (await Browser.ReadAsync(iconPage)).Badges;
        await iconPage.Context.CloseAsync();
        Assert.Equal(icons.Count, iconBadges.Count);
        wrong.AddRange(iconBadges.Zip(icons).SelectMany((pair, i) => Compare.Box($"badge-icons #{i}", i == 0 ? pair.First : pair.First with { X = pair.Second.X, Width = pair.Second.Width }, pair.Second, 0.5)));

        Assert.Empty(wrong);
    }

    private async Task<IReadOnlyList<string>> TextStylesAsync(string theme)
    {
        var page = await browser.FixtureAsync(gallery.Address, "badge-variants", theme);
        var texts = await page.EvaluateAsync<string[]>("""
            [...document.querySelectorAll("#fixture div")].filter((el) => parseFloat(getComputedStyle(el).borderTopLeftRadius) >= 9999)
              .map((badge) => { const text = [...badge.querySelectorAll("*")].find((el) => el.children.length === 0) ?? badge; const s = getComputedStyle(text); return s.fontSize + " " + s.fontWeight })
            """);
        await page.Context.CloseAsync();
        return texts;
    }

    [Fact]
    [Trait("AC", "CARD-01")]
    public async Task TheCardAndItsButtonsHaveShadcnsBoxesAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in (string[])["card-basic", "card-plain"])
            {
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var card = reference.Single(element => element.Radius == "14px");
                var buttons = reference.Where(element => element.Tag == "button").ToList();
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var found = await Browser.ReadAsync(page);
                await page.Context.CloseAsync();
                var what = $"{fixture} {theme}";
                var surface = found.Cards.Single();
                wrong.AddRange(Compare.Colors(what + " card", surface, card));
                wrong.AddRange(Compare.Box(what + " card", surface, card, 1));
                if (!SameShadow(surface.Shadow, card.Shadow))
                {
                    wrong.Add($"{what}: shadow {surface.Shadow}, shadcn {card.Shadow}");
                }

                Assert.Equal(buttons.Count, found.Buttons.Count);
                wrong.AddRange(found.Buttons.Zip(buttons).SelectMany((pair, i) => Compare.Box($"{what} button #{i}", pair.First, pair.Second, 1)));
            }
        }

        Assert.Empty(wrong);
    }

    private static bool SameShadow(string found, string expected)
    {
        static IEnumerable<string> Visible(string shadows) =>
            System.Text.RegularExpressions.Regex.Split(shadows, @",\s*(?![^()]*\))").Where(shadow => !shadow.StartsWith("rgba(0, 0, 0, 0)", StringComparison.Ordinal) && shadow != "none")
                .Select(shadow => System.Text.RegularExpressions.Regex.Replace(shadow, @"\s0px$", string.Empty));

        return Visible(found).Order(StringComparer.Ordinal).SequenceEqual(Visible(expected).Order(StringComparer.Ordinal));
    }

    [Fact]
    [Trait("AC", "BADGE-05")]
    public async Task ALinkBadgeTakesShadcnsHoverAndFocusOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var rest = await ReferenceMetrics.SlotsAsync("badge-links", theme, "badge");
            var hover = await ReferenceMetrics.OfAsync("badge-links", theme, "hover");
            var focus = await ReferenceMetrics.OfAsync("badge-links", theme, "focus");
            var page = await browser.FixtureAsync(gallery.Address, "badge-links", theme);
            await page.Mouse.MoveAsync(990, 890);
            var found = await Browser.PickAsync(page, "#fixture a[href]");
            Assert.Equal(rest.Count, found.Count);
            var links = await page.QuerySelectorAllAsync("#fixture a[href]");
            for (var i = 0; i < rest.Count; i++)
            {
                wrong.AddRange(Compare.Box($"{theme} #{i}", found[i], rest[i], 1).Concat(Look($"{theme} #{i} rest", found[i], rest[i])));
                await links[i].HoverAsync();
                wrong.AddRange(Look($"{theme} #{i} hover", (await Browser.PickAsync(page, "#fixture a[href]"))[i], hover[i]));
                await page.Mouse.MoveAsync(990, 890);
                await links[i].EvaluateAsync("(a) => a.focus({ focusVisible: true })");
                var focused = (await Browser.PickAsync(page, "#fixture a:focus")).Single();
                wrong.AddRange(CssColor.Parse(focused.BorderColor).Near(CssColor.Parse(focus[i].BorderColor)) ? [] : [$"{theme} #{i} focus border {focused.BorderColor}, shadcn {focus[i].BorderColor}"]);
                wrong.AddRange(CssShadow.Same(CssShadow.List(focused.Shadow), CssShadow.List(focus[i].Shadow)) ? [] : [$"{theme} #{i} ring {focused.Shadow}, shadcn {focus[i].Shadow}"]);
            }

            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Look(string what, ElementMetrics found, ElementMetrics expected) =>
        Compare.Colors(what, found, expected).Concat(found.Decoration == expected.Decoration ? [] : [$"{what}: decoration {found.Decoration}, shadcn {expected.Decoration}"]);

    [Fact]
    [Trait("AC", "CARD-05")]
    public async Task ASeparatedHeaderAndFooterAreRuledOffOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var reference = await ReferenceMetrics.OfAsync("card-separated", theme);
            var page = await browser.FixtureAsync(gallery.Address, "card-separated", theme);
            var all = await Browser.PickAsync(page, "#fixture *");
            var rules = await page.EvaluateAsync<string[][]>("[...document.querySelectorAll('#fixture *')].map((el) => { const s = getComputedStyle(el); return [s.borderTopWidth, s.borderBottomWidth, s.borderLeftWidth, s.borderTopColor, s.borderBottomColor] })");
            await page.Context.CloseAsync();
            wrong.AddRange(Compare.Standing(theme, all, reference.Where(element => element.Slot is "card" or "card-header" or "card-content" or "card-footer" or "button"), 1, colors: false));
            var border = CssColor.Parse(reference.Single(element => element.Slot == "card").BorderColor);
            var bottom = rules.Count(rule => rule is ["0px", "1px", "0px", _, var color] && CssColor.Parse(color).Near(border));
            var top = rules.Count(rule => rule is ["1px", "0px", "0px", var color, _] && CssColor.Parse(color).Near(border));
            wrong.AddRange(bottom == 1 && top == 1 ? [] : [$"{theme}: {bottom} bottom rules, {top} top rules"]);
        }

        Assert.Empty(wrong);
    }
}
