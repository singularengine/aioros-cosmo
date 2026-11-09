using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class NavigationWebTests(GalleryHost gallery, Browser browser)
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

    private static IEnumerable<string> Boxes(string what, List<ElementMetrics> found, List<ElementMetrics> expected, double tolerance) =>
        found.Count != expected.Count
            ? [$"{what}: {found.Count} elements, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{what} #{i}", pair.First, pair.Second, tolerance));

    private static IEnumerable<string> Color(string what, ElementMetrics found, string expected) =>
        CssColor.Parse(found.Color).Near(CssColor.Parse(expected)) ? [] : [$"{what}: color {found.Color}, shadcn {expected}"];

    [Fact]
    [Trait("AC", "TABS-01")]
    public async Task TabsAreATablistThatSwitchesWithoutARequestAsync() =>
        Assert.Empty(await EachThemeAsync("tabs-list", async (page, theme, reference) =>
        {
            var tabs = await Browser.PickAsync(page, "#fixture [role=tablist] > [role=tab]");
            var panels = await page.Locator("#fixture [role=tabpanel]").CountAsync();
            var triggers = reference.Where(element => element.Slot == "tabs-trigger").ToList();
            var wrong = tabs.Zip(triggers).SelectMany((pair, i) => Color($"{theme} tab {i}", pair.First, pair.Second.Color)
                .Concat(pair.First.FontSize == "14px" && pair.First.FontWeight == "500" ? [] : [$"{theme} tab {i}: font {pair.First.FontSize} {pair.First.FontWeight}"])).Take(3).ToList();
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            await page.Locator("#fixture [role=tab]").Nth(1).ClickAsync();
            var second = await page.Locator("#fixture [role=tab]").Nth(1).GetAttributeAsync("aria-selected");
            await page.Keyboard.PressAsync("ArrowRight");
            var third = await page.Locator("#fixture [role=tab]").Nth(2).GetAttributeAsync("aria-selected");
            wrong.AddRange(tabs.Count == 4 && panels == 4 && second == "true" && third == "true" && requests == 0 ? [] : [$"{theme}: tabs {tabs.Count}, panels {panels}, second {second}, third {third}, requests {requests}"]);
            return wrong;
        }));

    [Fact]
    [Trait("AC", "TABS-08")]
    public async Task TheLineAndVerticalTabsAreShadcnsOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["tabs-line", "tabs-vertical", "tabs-vertical-line"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme, reference) =>
                (await StripAsync(page, $"{fixture} {theme}", reference)).Concat(Line($"{fixture} {theme}", fixture, await page.EvaluateAsync<double[]>(SelectedLine), reference))));
        }

        Assert.Empty(wrong);
    }

    private const string SelectedLine = """
        () => {
          const tab = document.querySelector("#fixture [role=tab][aria-selected=true]"), after = getComputedStyle(tab, "::after")
          if (after.display === "none" || after.content === "none") return []
          const root = document.getElementById("fixture").getBoundingClientRect(), box = tab.getBoundingClientRect(), s = getComputedStyle(tab), px = (v) => parseFloat(v)
          const inner = { left: box.left + px(s.borderLeftWidth), right: box.right - px(s.borderRightWidth), top: box.top + px(s.borderTopWidth), bottom: box.bottom - px(s.borderBottomWidth) }
          const left = after.left !== "auto" ? inner.left + px(after.left) : inner.right - px(after.right) - px(after.width)
          const top = after.top !== "auto" ? inner.top + px(after.top) : inner.bottom - px(after.bottom) - px(after.height)
          const right = after.right !== "auto" ? inner.right - px(after.right) : left + px(after.width)
          const bottom = after.bottom !== "auto" ? inner.bottom - px(after.bottom) : top + px(after.height)
          return [left - root.left, top - root.top, right - left, bottom - top]
        }
        """;

    private static async Task<List<string>> StripAsync(IPage page, string what, IReadOnlyList<ElementMetrics> reference)
    {
        var tabs = await Browser.PickAsync(page, "#fixture [role=tab]");
        var list = (await Browser.PickAsync(page, "#fixture [role=tablist]")).Single();
        var expected = reference.Single(element => element.Slot == "tabs-list");
        var triggers = reference.Where(element => element.Slot == "tabs-trigger").ToList();
        var errors = Compare.Box($"{what} strip", list, expected, 1).Concat(Compare.Colors($"{what} strip", list, expected).Where(error => error.Contains("background", StringComparison.Ordinal))).ToList();
        errors.AddRange(tabs.Count == triggers.Count ? tabs.Zip(triggers).SelectMany((pair, i) => Compare.Box($"{what} tab {i}", pair.First, pair.Second, 1).Concat(Compare.Colors($"{what} tab {i}", pair.First, pair.Second))) : [$"{what}: {tabs.Count} tabs"]);
        if (reference.FirstOrDefault(element => element.Slot == "tabs-content" && element.Width > 0) is { } content)
        {
            var panel = (await Browser.PickAsync(page, "#fixture [role=tabpanel]:not([hidden])")).Single();
            errors.AddRange(Compare.Box($"{what} content", panel with { Height = content.Height }, content, 1));
        }

        return errors;
    }

    private static IEnumerable<string> Line(string what, string fixture, double[] line, IReadOnlyList<ElementMetrics> reference)
    {
        var selected = reference.First(element => element.Slot == "tabs-trigger");
        double[] expected = !fixture.EndsWith("line", StringComparison.Ordinal) ? [] : fixture.StartsWith("tabs-vertical", StringComparison.Ordinal)
            ? [selected.X + selected.Width + 2, selected.Y, 2, selected.Height] : [selected.X, selected.Y + selected.Height + 3, selected.Width, 2];
        return line.Length == expected.Length && line.Zip(expected).All(pair => Math.Abs(pair.First - pair.Second) <= 1)
            ? [] : [$"{what}: line {string.Join(',', line.Select(value => value.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)))}, shadcn {string.Join(',', expected)}"];
    }

    [Fact]
    [Trait("AC", "TABS-09")]
    public async Task ATabIsHoveredAndFocusedAsShadcnsOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["tabs-list", "tabs-line"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme, _) =>
            {
                var hover = await ReferenceMetrics.OfAsync(fixture, theme, "hover");
                var focus = await ReferenceMetrics.OfAsync(fixture, theme, "focus");
                await page.Locator("#fixture [role=tab]").Nth(1).HoverAsync();
                var hovered = (await Browser.PickAsync(page, "#fixture [role=tab]"))[1];
                await page.Mouse.MoveAsync(990, 890);
                await page.Locator("#fixture [role=tab]").Nth(0).EvaluateAsync("(tab) => tab.focus({ focusVisible: true })");
                var focused = (await Browser.PickAsync(page, "#fixture [role=tab]:focus")).Single();
                var outline = await page.EvaluateAsync<string>("getComputedStyle(document.activeElement).outlineStyle");
                var shadows = CssShadow.List(focused.Shadow);
                var ring = CssShadow.List(focus[0].Shadow);
                IReadOnlyList<CssShadow> expected = [new CssShadow(0, 0, 0, 1, ring[0].Color with { A = 1 }, false), .. ring];
                return Color($"{fixture} {theme} hovered", hovered, hover[1].Color)
                    .Concat(CssColor.Parse(focused.BorderColor).Near(CssColor.Parse(focus[0].BorderColor)) ? [] : [$"{fixture} {theme} focused border {focused.BorderColor}, shadcn {focus[0].BorderColor}"])
                    .Concat(CssShadow.Same(shadows, expected) && outline == "none" ? [] : [$"{fixture} {theme} focused ring {focused.Shadow} outline {outline}, shadcn {focus[0].Outline} outline and {focus[0].Shadow}"]);
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TABS-11")]
    public async Task UpAndDownMoveAVerticalStripsTabsOnTheWebAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "tabs-vertical", "light");
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        await page.Locator("#fixture [role=tab]").Nth(0).FocusAsync();
        var steps = new List<string>();
        foreach (var key in (string[])["ArrowDown", "ArrowUp"])
        {
            await page.Keyboard.PressAsync(key);
            steps.Add(await page.EvaluateAsync<string>("document.activeElement.textContent + ' ' + document.activeElement.getAttribute('aria-selected')"));
        }

        var orientation = await page.Locator("#fixture [role=tablist]").GetAttributeAsync("aria-orientation");
        await page.Context.CloseAsync();

        Assert.Equal(("Password true, Account true", "vertical", 0), (string.Join(", ", steps), orientation, requests));
    }

    [Fact]
    [Trait("AC", "TABS-05")]
    public async Task APostKeepsTheBoundTabSelectedAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/tabs").ToString());
        var password = page.Locator("main [role=tab]:has-text('Password')").Last;
        await password.ClickAsync();
        await page.ClickAsync("button:text-is('Save password')");
        await page.WaitForSelectorAsync("text=Password saved");
        var checkedAfter = await page.Locator("main [role=tab]:has-text('Password')").Last.GetAttributeAsync("aria-selected") == "true";
        var panel = await page.Locator("main :text-is(\"Change your password here. After saving, you'll be logged out.\")").Last.IsVisibleAsync();
        await page.Context.CloseAsync();

        Assert.True(checkedAfter);
        Assert.True(panel);
    }

    [Fact]
    [Trait("AC", "ACCORDION-06")]
    public async Task ABoundItemFollowsItsModelOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/accordion").ToString());
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(Uri.UnescapeDataString(request.PostData ?? string.Empty));
            }
        };
        var first = page.Locator("details", new() { Has = page.Locator("summary", new() { HasText = "Is it accessible?" }) });
        var second = page.Locator("details", new() { Has = page.Locator("summary", new() { HasText = "Is it styled?" }) }).Last;
        var toggle = page.GetByRole(AriaRole.Button, new() { Name = "Toggle the first answer" });
        async Task<bool> OpenAsync(ILocator details) => await details.EvaluateAsync<bool>("d => d.open");
        async Task PressAsync()
        {
            var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
            await toggle.ClickAsync();
            await answered;
        }

        await second.Locator("summary").ClickAsync();
        await PressAsync();
        await page.WaitForFunctionAsync("d => d.open", await first.ElementHandleAsync(), new() { PollingInterval = 20 });
        await PressAsync();
        await page.WaitForFunctionAsync("d => !d.open", await first.ElementHandleAsync(), new() { PollingInterval = 20 });
        var changed = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await first.Locator("summary").ClickAsync();
        await changed;
        var change = posts[^1];
        await PressAsync();
        await page.WaitForFunctionAsync("d => !d.open", await first.ElementHandleAsync(), new() { PollingInterval = 20 });
        var state = (await OpenAsync(first), await OpenAsync(second), posts.Count);
        await page.Context.CloseAsync();

        Assert.Contains("_chg=", change, StringComparison.Ordinal);
        Assert.Equal((false, true, 4), state);
    }

    [Fact]
    [Trait("AC", "ACCORDION-01")]
    public async Task AccordionItemsAreDetailsAsync() =>
        Assert.Empty(await EachThemeAsync("accordion-demo", async (page, theme, reference) =>
        {
            var details = await page.EvaluateAsync<bool[]>("[...document.querySelectorAll('#fixture details')].map((d) => d.open)");
            var summaries = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture summary')].map((s) => s.textContent.trim())");
            var paragraphs = await Browser.PickAsync(page, "#fixture details[open] .c > * > div:not(:has(*))");
            var borders = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture details')].map((d) => getComputedStyle(d).borderBottomWidth)");
            var expected = reference.Where(element => element.Slot == "p").ToList();
            var wrong = new List<string>();
            wrong.AddRange(details.SequenceEqual([true, false, false]) && summaries.SequenceEqual(["Product Information", "Shipping Details", "Return Policy"]) ? [] : [$"{theme}: details {string.Join(',', details)} {string.Join(',', summaries)}"]);
            wrong.AddRange(paragraphs.Count == 2 ? paragraphs.Zip(expected).SelectMany((pair, i) => Color($"{theme} paragraph {i}", pair.First, pair.Second.Color).Concat(pair.First.FontSize == pair.Second.FontSize && pair.First.LineHeight == pair.Second.LineHeight ? [] : [$"{theme} paragraph {i}: {pair.First.FontSize} {pair.First.LineHeight}"])) : [$"{theme}: {paragraphs.Count} paragraphs"]);
            wrong.AddRange(borders.SequenceEqual(["1px", "1px", "0px"]) ? [] : [$"{theme}: borders {string.Join(',', borders)}"]);
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            wrong.AddRange(await ExclusiveAsync(page, theme));
            wrong.AddRange(requests == 0 ? [] : [$"{theme}: {requests} requests"]);
            return wrong;
        }));

    /// <summary>The single accordion's items as one exclusive group: one shared name, the second summary opening its item and closing the first.</summary>
    private static async Task<IEnumerable<string>> ExclusiveAsync(IPage page, string theme)
    {
        var names = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture details')].map((d) => d.getAttribute('name') ?? '')");
        await page.Locator("#fixture summary").Nth(1).ClickAsync();
        var opened = await page.EvaluateAsync<bool[]>("[...document.querySelectorAll('#fixture details')].map((d) => d.open)");
        var errors = new List<string>();
        errors.AddRange(names.Distinct().Count() == 1 && names[0].Length > 0 ? [] : [$"{theme}: names {string.Join(',', names)}"]);
        errors.AddRange(opened.SequenceEqual([false, true, false]) ? [] : [$"{theme}: after the second summary {string.Join(',', opened)}"]);
        return errors;
    }

    [Fact]
    [Trait("AC", "COLLAPSIBLE-01")]
    public async Task TheCollapsibleIsDetailsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["collapsible-demo", "collapsible-open"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, (page, theme, reference) => CollapsibleAsync(page, theme, reference, fixture)));
        }

        Assert.Empty(wrong);
    }

    private static async Task<IEnumerable<string>> CollapsibleAsync(IPage page, string theme, IReadOnlyList<ElementMetrics> reference, string fixture)
    {
        var open = await page.EvaluateAsync<bool>("document.querySelector('#fixture details').open");
        var summary = await page.EvaluateAsync<string>("document.querySelector('#fixture summary').textContent.trim()");
        var rows = await Browser.PickAsync(page, "#fixture div");
        var expected = reference.First(element => element.Tag == "div" && element.Height == 38);
        var errors = new List<string>();
        errors.AddRange(open == (fixture == "collapsible-open") && summary.StartsWith("@peduarte starred 3 repositories", StringComparison.Ordinal) ? [] : [$"{theme} {fixture}: open {open}, summary {summary}"]);
        var toggles = rows.Where(row => row.BorderWidth == "1px" && row.Width == 32 && row.Height == 32).ToList();
        errors.AddRange(toggles.Count == 1 ? [] : [$"{theme} {fixture}: {toggles.Count} toggles"]);
        var bordered = rows.Where(row => row.BorderWidth == "1px" && row.Width != 32).ToList();
        errors.AddRange(bordered.Count >= 1 && bordered.All(row => row.Radius == expected.Radius && row.PaddingLeft == expected.PaddingLeft && row.Height == expected.Height) ? [] : [$"{theme} {fixture}: rows {string.Join(',', bordered.Select(row => $"{row.Radius} {row.PaddingLeft} {row.Height}"))}"]);
        if (open)
        {
            return errors;
        }

        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        await page.Locator("#fixture summary").ClickAsync();
        errors.AddRange(await page.EvaluateAsync<bool>("document.querySelector('#fixture details').open") && requests == 0 ? [] : [$"{theme}: did not open without a request"]);
        return errors;
    }

    [Fact]
    [Trait("AC", "BREADCRUMB-01")]
    public async Task BreadcrumbsAreLinksAtShadcnsPlacesAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["breadcrumb-demo", "breadcrumb-separator"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, (page, theme, reference) => CrumbsAsync(page, theme, reference, fixture)));
        }

        Assert.Empty(wrong);
    }

    private static async Task<IEnumerable<string>> CrumbsAsync(IPage page, string theme, IReadOnlyList<ElementMetrics> reference, string fixture)
    {
        var links = (await Browser.PickAsync(page, "#fixture a[href]")).ToList();
        var expectedLinks = reference.Where(element => element.Slot == "breadcrumb-link").ToList();
        var separators = await Browser.PickAsync(page, "#fixture svg:not(svg svg):not(button svg)");
        var expectedSeparators = reference.Where(element => element.Slot == "breadcrumb-separator").ToList();
        var pageElement = (await Browser.PickAsync(page, "#fixture div:not(:has(*))")).Last(element => element.Text == "Breadcrumb");
        var shadcnPage = reference.Single(element => element.Slot == "breadcrumb-page");
        var errors = links.Count == expectedLinks.Count ? links.Zip(expectedLinks).SelectMany((pair, i) => Compare.Box($"{theme} {fixture} link {i}", pair.First, pair.Second, 1)).ToList() : [$"{theme} {fixture}: {links.Count} links"];
        errors.AddRange(links.Zip(expectedLinks).SelectMany((pair, i) => Color($"{theme} link {i}", pair.First, pair.Second.Color)));
        var shown = separators.Where(separator => separator.Width == 14).ToList();
        errors.AddRange(shown.Count == expectedSeparators.Count ? shown.Zip(expectedSeparators).SelectMany((pair, i) => Compare.Box($"{theme} {fixture} separator {i}", pair.First, pair.Second, 1)) : [$"{theme} {fixture}: {shown.Count} separators"]);
        errors.AddRange(Compare.Box($"{theme} page", pageElement, shadcnPage, 1).Concat(Color($"{theme} page", pageElement, shadcnPage.Color)));
        var items = await page.Locator("#fixture nav[aria-label='breadcrumb'] > ol > li").CountAsync();
        var hidden = await page.Locator("#fixture li [aria-hidden='true']").CountAsync();
        errors.AddRange(items == expectedSeparators.Count + 1 && hidden == expectedSeparators.Count ? [] : [$"{theme} {fixture}: {items} items in nav > ol, {hidden} hidden separators"]);
        await page.Locator("#fixture a[href]").First.HoverAsync();
        var hovered = (await Browser.PickAsync(page, "#fixture a[href]"))[0];
        errors.AddRange(Color($"{theme} link under the pointer", hovered, shadcnPage.Color));
        return errors;
    }

    [Fact]
    [Trait("AC", "BREADCRUMB-04")]
    public async Task TheBreadcrumbsPageIsTheCurrentOneAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "breadcrumb-demo", "light");
        var current = await page.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture [aria-current]')].map(e => e.tagName + ':' + e.getAttribute('aria-current') + ':' + e.textContent.trim()).join()");
        await page.Context.CloseAsync();

        Assert.Equal("DIV:page:Breadcrumb", current);
    }

    [Fact]
    [Trait("AC", "PAGINATION-01")]
    public async Task ThePaginationsButtonsAreAtShadcnsPlacesAsync() =>
        Assert.Empty(await EachThemeAsync("pagination-demo", async (page, theme, reference) =>
        {
            var buttons = (await Browser.PickAsync(page, "#fixture button")).ToList();
            var expected = reference.Where(element => element.Slot == "pagination-link").ToList();
            var texts = buttons.Select(button => button.Text).ToList();
            if (!texts.SequenceEqual(["Previous", "1", "2", "3", "Next"]))
            {
                return [$"{theme}: {string.Join(',', texts)}"];
            }

            var errors = buttons.Zip(expected).SelectMany((pair, i) => PageButton($"{theme} button {i}", pair.First, pair.Second)).ToList();
            errors.AddRange(Math.Abs(buttons[1].X - buttons[0].X - buttons[0].Width - 4) <= 1 ? [] : [$"{theme}: buttons not 4 px apart"]);
            errors.AddRange(await page.Locator("#fixture nav[aria-label='pagination'] button").CountAsync() == 5 && await page.Locator("#fixture button:has(svg)").CountAsync() == 2 ? [] : [$"{theme}: not a pagination landmark with two chevrons"]);
            return errors;
        }));

    private static IEnumerable<string> PageButton(string what, ElementMetrics found, ElementMetrics expected) =>
        Compare.Colors(what, found, expected)
            .Concat(Math.Abs(found.Y - expected.Y) <= 1 && Math.Abs(found.Height - expected.Height) <= 1 ? [] : [$"{what}: y {found.Y} height {found.Height}"])
            .Concat(Math.Abs(found.Width - expected.Width) <= 1 ? [] : [$"{what}: width {found.Width}, shadcn {expected.Width}"]);

    [Fact]
    [Trait("AC", "PAGINATION-06")]
    public async Task ThePaginationIsAListMarkingItsCurrentPageAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "pagination-demo", "light");
        var shape = await page.EvaluateAsync<string>("""
            () => {
              const list = document.querySelector('#fixture nav[aria-label=pagination] ul'), items = list ? [...list.children] : []
              return items.map(li => li.tagName + ':' + li.children.length + ':' + li.textContent.trim() + (li.querySelector('svg') ? '+icon' : '')).join() + '|'
                + [...document.querySelectorAll('#fixture [aria-current]')].map(e => e.tagName + ':' + e.getAttribute('aria-current') + ':' + e.textContent.trim()).join()
            }
            """);
        await page.GotoAsync(new Uri(gallery.Address, "components/pagination").ToString());
        await page.Locator("main button:text-is('3')").Last.ClickAsync();
        await page.WaitForSelectorAsync("text=Page 3 of 10");
        var posted = await page.EvaluateAsync<string>("() => [...document.querySelectorAll('main [aria-current]')].map(e => e.textContent.trim()).join()");
        await page.Context.CloseAsync();

        Assert.Equal("LI:1:Previous+icon,LI:1:1,LI:1:2,LI:1:3,LI:1:+icon,LI:1:Next+icon|BUTTON:page:2", shape);
        Assert.Contains("3", posted.Split(','));
    }

    [Fact]
    [Trait("AC", "PAGINATION-05")]
    public async Task OnAPhoneThePaginationShowsOnlyItsChevronsAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 390);
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/pagination-demo").ToString());
        var shown = await page.EvaluateAsync<string>("""
            () => {
              const buttons = [...document.querySelectorAll('#fixture button')], boxes = buttons.map(b => b.getBoundingClientRect())
              return buttons.map(b => b.innerText.trim() || b.getAttribute('aria-label')).join() + '|' + boxes.slice(1).every((box, i) => box.left >= boxes[i].right - 0.5)
            }
            """);
        await page.Context.CloseAsync();

        Assert.Equal("Go to previous page,1,2,3,Go to next page|true", shown);
    }

    [Fact]
    [Trait("AC", "PAGINATION-04")]
    public async Task PostedPagesMoveTheBoundPaginationAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/pagination").ToString());
        await page.Locator("main button:text-is('3')").Last.ClickAsync();
        await page.WaitForSelectorAsync("text=Page 3 of 10");
        var third = await page.Locator("main button:text-is('3')").Last.EvaluateAsync<string>("(el) => getComputedStyle(el).borderTopWidth");
        await page.GotoAsync(new Uri(gallery.Address, "components/pagination").ToString());
        await page.Locator("main button:has-text('Next')").Last.ClickAsync();
        await page.WaitForSelectorAsync("text=Page 3 of 10");
        var next = await page.Locator("main button:text-is('3')").Last.EvaluateAsync<string>("(el) => getComputedStyle(el).borderTopWidth");
        await page.Context.CloseAsync();

        Assert.Equal(("1px", "1px"), (third, next));
    }

    [Fact]
    [Trait("AC", "BUTTONGROUP-01")]
    public async Task ButtonGroupsJoinTheirChildrenAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["button-group-demo", "button-group-size", "button-group-orientation"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme, reference) =>
            {
                var buttons = (await Browser.PickAsync(page, "#fixture button")).ToList();
                var expected = reference.Where(element => element.Tag == "button").ToList();
                var errors = Boxes($"{theme} {fixture}", buttons, expected, 1).ToList();
                var groups = reference.Count(element => element.Slot == "button-group");
                errors.AddRange(await page.Locator("#fixture [role='group']").CountAsync() == groups ? [] : [$"{theme} {fixture}: not {groups} groups"]);
                errors.AddRange(buttons.Count == expected.Count ? buttons.Zip(expected).Where(pair => pair.First.Radius != pair.Second.Radius || pair.First.BorderWidth != pair.Second.BorderWidth).Select(pair => $"{theme} {fixture} {pair.First.Text}: {pair.First.Radius} {pair.First.BorderWidth}, shadcn {pair.Second.Radius} {pair.Second.BorderWidth}") : []);
                if (fixture == "button-group-orientation")
                {
                    var separator = (await Browser.PickAsync(page, "#fixture hr")).Single();
                    var shadcn = reference.Single(element => element.Slot == "button-group-separator");
                    errors.AddRange(Compare.Box($"{theme} separator", separator, shadcn, 1).Concat(CssColor.Parse(separator.Background).Near(CssColor.Parse(shadcn.Background)) ? [] : [$"{theme} separator {separator.Background}"]));
                }

                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "BUTTONGROUP-04")]
    public async Task APostedButtonTakesTheChosenLookAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/button-group").ToString());
        await page.Locator("main button:text-is('Report')").Last.ClickAsync();
        await page.WaitForSelectorAsync("text=Reported");
        await page.AddStyleTagAsync(new() { Content = "*,*::before,*::after{transition:none!important}" });
        await page.Mouse.MoveAsync(1270, 890);
        var looks = await page.EvaluateAsync<string[]>("['Archive', 'Report', 'Snooze'].map((t) => [...document.querySelectorAll('main button')].filter((b) => b.textContent.trim() === t).pop()).map((b) => getComputedStyle(b).backgroundColor)");
        await page.Context.CloseAsync();
        var secondary = (await ReferenceMetrics.SlotsAsync("button-group-orientation", "light", "button")).First(button => button.Text == "Copy").Background;

        Assert.True(CssColor.Parse(looks[1]).Near(CssColor.Parse(secondary)), looks[1]);
        Assert.False(CssColor.Parse(looks[0]).Near(CssColor.Parse(secondary)), looks[0]);
    }
}
