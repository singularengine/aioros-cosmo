using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class StructureWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    private async Task<List<string>> EachThemeAsync(string fixture, Func<IPage, string, IReadOnlyList<ElementMetrics>, Task<IEnumerable<string>>> check)
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
            wrong.AddRange(await check(page, theme, await ReferenceMetrics.OfAsync(fixture, theme)));
            await page.Context.CloseAsync();
        }

        return wrong;
    }

    private static IEnumerable<string> Is(string what, bool condition, object? found) => condition ? [] : [$"{what}: {found}"];

    private static Task<string> StyleAsync(ILocator element, string property) =>
        element.EvaluateAsync<string>($"(el) => getComputedStyle(el).{property}");

    [Fact]
    [Trait("AC", "SCROLLAREA-01")]
    public async Task TheScrollAreaIsABorderedBoxThatScrollsAsync()
    {
        var wrong = await EachThemeAsync("scroll-area-demo", async (page, theme, reference) =>
        {
            var area = reference.Single(element => element.Slot == "scroll-area");
            var box = (await Browser.PickAsync(page, "#fixture > div"))[0];
            var errors = Compare.Box($"{theme} area", box, area, 1).ToList();
            var scrolls = await page.Locator("#fixture > div").EvaluateAsync<string>("(el) => getComputedStyle(el).overflowY");
            errors.AddRange(Is($"{theme} scrolls and border", scrolls == "auto" && box.BorderWidth == "1px" && box.Radius == "8px" && CssColor.Parse(box.BorderColor).Near(CssColor.Parse(area.BorderColor)), $"{scrolls} {box.BorderWidth} {box.Radius} {box.BorderColor}"));
            var tags = await page.EvaluateAsync<double[]>("() => [...document.querySelectorAll('#fixture *')].filter((el) => el.childElementCount === 0 && el.textContent.startsWith('v1.2.0-beta.')).slice(0, 5).map((el) => el.getBoundingClientRect().top - document.querySelector('#fixture').getBoundingClientRect().top)");
            var separators = reference.Where(element => element.Slot == "separator").Take(5).Select(element => element.Y - 28).ToList();
            errors.AddRange(Is($"{theme} tags", tags.Length == 5 && tags.Zip(separators).All(pair => Math.Abs(pair.First - pair.Second) <= 1), string.Join(",", tags)));
            return errors;
        });
        wrong.AddRange(await EachThemeAsync("scroll-area-horizontal", async (page, theme, reference) =>
        {
            var box = (await Browser.PickAsync(page, "#fixture > div"))[0];
            var frame = await page.EvaluateAsync<double>("() => document.querySelector('#fixture img').parentElement.getBoundingClientRect().width");
            var scrolls = await page.Locator("#fixture > div").EvaluateAsync<string>("(el) => getComputedStyle(el).overflowX");
            return Is($"{theme} horizontal", Math.Abs(box.Width - 384) <= 1 && Math.Abs(frame - 300) <= 1 && scrolls == "auto", $"{box.Width} {frame} {scrolls}");
        }));

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TABLE-01")]
    public async Task TheTableIsATableWithShadcnsCellsFooterAndCaptionAsync() =>
        Assert.Empty(await EachThemeAsync("table-demo", async (page, theme, reference) =>
        {
            var heads = await page.Locator("#fixture th").AllInnerTextsAsync();
            var rows = await page.Locator("#fixture tbody tr").CountAsync();
            var cell = page.Locator("#fixture tbody td").First;
            var errors = Is($"{theme} header", string.Join(",", heads) == "Invoice,Status,Method,Amount", string.Join(",", heads)).Concat(Is($"{theme} rows", rows == 7, rows)).ToList();
            errors.AddRange(Is($"{theme} cell", await StyleAsync(cell, "paddingLeft") == "8px" && await StyleAsync(cell, "paddingTop") == "8px" && await StyleAsync(cell, "fontSize") == "14px", await StyleAsync(cell, "padding")));
            errors.AddRange(Is($"{theme} invoice weight", await page.Locator("#fixture tbody td").First.Locator("*").Last.EvaluateAsync<string>("(el) => getComputedStyle(el).fontWeight") == "500", "weight"));
            errors.AddRange(Is($"{theme} amount", await StyleAsync(page.Locator("#fixture th").Last, "textAlign") == "right", "align"));
            var footer = (await Browser.PickAsync(page, "#fixture > div > :nth-child(2)"))[0];
            var tfoot = reference.Single(element => element.Tag == "tfoot");
            var weights = await page.Locator("#fixture > div > :nth-child(2) :text-is('Total'), #fixture > div > :nth-child(2) :text-is('$2,500.00')").EvaluateAllAsync<string[]>("(els) => els.map((el) => getComputedStyle(el).fontWeight)");
            errors.AddRange(Is($"{theme} footer", CssColor.Parse(footer.Background).Near(CssColor.Parse(tfoot.Background)) && weights.Length == 2 && weights.All(weight => weight == "500"), $"{footer.Background} {string.Join(",", weights)}"));
            errors.AddRange(Is($"{theme} footer border", await page.Locator("#fixture > div > :nth-child(2)").EvaluateAsync<string>("(el) => getComputedStyle(el).borderTopWidth") == "1px", "border"));
            var caption = (await Browser.PickAsync(page, "#fixture > div > :nth-child(3)"))[0];
            var expected = reference.Single(element => element.Tag == "caption");
            errors.AddRange(Is($"{theme} caption", CssColor.Parse(caption.Color).Near(CssColor.Parse(expected.Color)) && Math.Abs(caption.X + caption.Width / 2 - 320) <= 1, $"{caption.Color} {caption.X} {caption.Width}"));
            return errors;
        }));

    [Fact]
    [Trait("AC", "TABLE-04")]
    public async Task APostedAddGrowsTheBoundTableAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/table").ToString());
        await page.ClickAsync("button:text-is('Add invoice')");
        await page.WaitForSelectorAsync("text=8 invoices");
        var rows = await page.Locator("main table").Nth(1).Locator("tbody tr").CountAsync();
        var total = await page.Locator("main :text-is('$2,350.00')").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal((8, true), (rows, total > 0));
    }

    [Fact]
    [Trait("AC", "SCROLLAREA-05")]
    public async Task TheBarsAreTransparentUntilThePointerComesAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/scroll-area-demo").ToString());
        const string Read = "() => { const area = [...document.querySelectorAll('#fixture *')].find((el) => getComputedStyle(el).overflowY === 'auto'), s = getComputedStyle(area); return s.scrollbarColor + '|' + s.transitionProperty + '|' + s.transitionDelay }";
        var rest = await page.EvaluateAsync<string>(Read);
        await page.Locator("#fixture").HoverAsync(new() { Position = new() { X = 100, Y = 100 } });
        var hovered = await page.EvaluateAsync<string>(Read);
        await page.Context.CloseAsync();

        Assert.StartsWith("rgba(0, 0, 0, 0) rgba(0, 0, 0, 0)|scrollbar-color|0.6s", rest, StringComparison.Ordinal);
        var thumb = hovered.Split('|')[0];
        Assert.True(CssColor.Parse(thumb[..thumb.LastIndexOf(" rgba(0, 0, 0, 0)", StringComparison.Ordinal)]).Near(CssColor.Parse("oklch(0.922 0 0)")) && hovered.EndsWith("|0s", StringComparison.Ordinal), hovered);
    }

    [Fact]
    [Trait("AC", "NAVMENU-01")]
    public async Task NavigationTriggersOpenAnchoredPopoversWithoutARequestAsync() =>
        Assert.Empty(await EachThemeAsync("navigation-menu-demo", async (page, theme, reference) =>
        {
            var triggers = await Browser.PickAsync(page, "#fixture button[popovertarget], #fixture a");
            var expected = reference.Where(element => element.Slot is "navigation-menu-trigger" || (element.Slot == "navigation-menu-link" && element.Text == "Docs")).ToList();
            var errors = triggers.Zip(expected).SelectMany((pair, i) => Is($"{theme} item {i}", Math.Abs(pair.First.Height - 36) <= 0.5 && Math.Abs(pair.First.Y - pair.Second.Y) <= 1 && Math.Abs(pair.First.X - pair.Second.X) <= 1 && Math.Abs(pair.First.Width - pair.Second.Width) <= 1 && pair.First.FontWeight == "500" && pair.First.FontSize == "14px", $"{pair.First.X} {pair.First.Y} {pair.First.Width} {pair.First.Height} {pair.First.FontWeight}, shadcn {pair.Second.X} {pair.Second.Width}")).ToList();
            var chevrons = await page.Locator("#fixture nav button[popovertarget]:has(svg)").CountAsync();
            var shadcnTriggers = reference.Count(element => element.Slot == "navigation-menu-trigger");
            errors.AddRange(Is($"{theme} nav", chevrons == shadcnTriggers, $"{chevrons} chevrons in a nav, shadcn {shadcnTriggers} triggers"));
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            await page.Locator("#fixture button[popovertarget]").Nth(1).ClickAsync();
            var open = page.Locator("#fixture [popover]:popover-open");
            var trigger = await page.Locator("#fixture button[popovertarget]").Nth(1).BoundingBoxAsync();
            var content = await open.BoundingBoxAsync();
            var links = await open.Locator("button").CountAsync();
            errors.AddRange(Is($"{theme} open", trigger is not null && content is not null && Math.Abs(content.X - trigger.X) <= 1 && Math.Abs(content.Y - (trigger.Y + trigger.Height + 6)) <= 1 && links == 6 && requests == 0, $"{content?.X},{content?.Y} trigger {trigger?.X},{trigger?.Y} links {links} requests {requests}"));
            return errors;
        }));

    [Fact]
    [Trait("AC", "NAVMENU-10")]
    public async Task TheArrowKeysMoveBetweenTheMenusItemsAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/navigation-menu-demo").ToString());
        await Browser.KeysAsync(page);
        var items = page.Locator("#fixture nav button, #fixture nav a");
        const string Focused = "() => document.activeElement.textContent.trim()";
        var walk = new List<string>();
        await items.First.FocusAsync();
        foreach (var key in (string[])["ArrowRight", "ArrowRight", "ArrowLeft"])
        {
            await page.Keyboard.PressAsync(key);
            walk.Add(await page.EvaluateAsync<string>(Focused));
        }

        await items.First.FocusAsync();
        await page.Keyboard.PressAsync("Tab");
        var tabbed = await page.EvaluateAsync<string>(Focused);
        await page.Context.CloseAsync();

        Assert.Equal(["Components", "Docs", "Components"], walk);
        Assert.Equal("Components", tabbed);
    }

    [Fact]
    [Trait("AC", "NAVMENU-05")]
    public async Task APostedLinkMovesTheBoundCurrentPageAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/navigation-menu").ToString());
        await page.ClickAsync("main button:has-text('Getting started')");
        await page.ClickAsync("main [popover]:popover-open button:text-is('Components')");
        var shown = await page.WaitForSelectorAsync("text=Current page: /docs/components");
        var closed = await page.Locator("main [popover]:popover-open").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal((true, 0), (shown is not null, closed));
    }

    [Fact]
    [Trait("AC", "MENUBAR-01")]
    public async Task TheMenubarIsAToolbarOfPopoverMenusAsync() =>
        Assert.Empty(await EachThemeAsync("menubar-demo", async (page, theme, reference) =>
        {
            var bar = (await Browser.PickAsync(page, "#fixture menu"))[0];
            var expected = reference.Single(element => element.Slot == "menubar");
            var errors = Compare.Box($"{theme} bar", bar, expected, 1).ToList();
            errors.AddRange(Is($"{theme} bar look", bar.BorderWidth == "1px" && bar.Radius == "8px" && CssColor.Parse(bar.Background).Near(CssColor.Parse(expected.Background)), $"{bar.BorderWidth} {bar.Radius} {bar.Background}"));
            var names = await page.Locator("#fixture menu > li > button").AllInnerTextsAsync();
            errors.AddRange(Is($"{theme} menus", string.Join(",", names) == "File,Edit,View,Profiles", string.Join(",", names)));
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            await page.Locator("#fixture menu > li > button").First.ClickAsync();
            var open = page.Locator("#fixture [popover]:popover-open");
            var items = await open.Locator(":scope > .mi").AllInnerTextsAsync();
            var shortcuts = await open.Locator("kbd").CountAsync();
            var separators = await open.Locator(":scope > hr").CountAsync();
            errors.AddRange(Is($"{theme} file menu", items.Count == 5 && items[0].StartsWith("New Tab", StringComparison.Ordinal) && shortcuts == 3 && separators == 2 && requests == 0, $"{string.Join("|", items)} {shortcuts} {separators} {requests}"));
            await page.Keyboard.PressAsync("Escape");
            await page.Locator("#fixture menu > li > button").Nth(2).ClickAsync();
            var checks = await page.Locator("#fixture [popover]:popover-open svg").CountAsync();
            errors.AddRange(Is($"{theme} view menu", checks == 1, checks));
            return errors;
        }));

    [Fact]
    [Trait("AC", "MENUBAR-05")]
    public async Task APostedMenubarItemRunsTheModelsCommandAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/menubar").ToString());
        await page.Locator("main menu").Last.Locator("button:text-is('File')").ClickAsync();
        await page.ClickAsync("main [popover]:popover-open button:has-text('Print...')");
        var shown = await page.WaitForSelectorAsync("text=Last action: Print");
        await page.Context.CloseAsync();

        Assert.NotNull(shown);
    }
}
