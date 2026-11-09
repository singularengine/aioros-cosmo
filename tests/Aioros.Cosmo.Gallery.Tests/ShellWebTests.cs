using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class ShellWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Shells = [.. GalleryHost.ShellFixtures.Select(static fixture => fixture.Fixture)];

    private const string Misses = """
        (boxes) => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          const ours = [...document.querySelectorAll("#fixture *")].map((element) => element.getBoundingClientRect()).filter((box) => box.width > 0)
          return boxes.filter(([x, y, width, height]) => !ours.some((box) =>
            Math.abs(box.x - root.x - x) <= 1.5 && Math.abs(box.y - root.y - y) <= 3 && Math.abs(box.width - width) <= 1.5 && Math.abs(box.height - height) <= 3))
            .map(([x, y, width, height, text]) => `${text} ${x},${y} ${width}x${height}`)
        }
        """;

    private const string Sidebars = "() => [...document.querySelectorAll('#fixture nav')].filter((nav) => nav.getBoundingClientRect().height > 600).map((nav) => Math.round(nav.getBoundingClientRect().width)).join('/') + ' ' + Math.round(document.querySelector('#fixture main').getBoundingClientRect().width)";

    private const string Turn = "(element) => { const t = getComputedStyle(element).transform; return t === 'none' ? 0 : Math.round(Math.atan2(new DOMMatrix(t).b, new DOMMatrix(t).a) * 180 / Math.PI) }";

    /// <summary>A shell's fixture in a viewport that holds it beside the Gallery's navigation, its framework modules loaded.</summary>
    private async Task<IPage> ShellAsync(string block, string theme = "light")
    {
        var page = await browser.FixtureAsync(gallery.Address, block, theme, 800, 1560);
        await Browser.KeysAsync(page, flags: true);
        return page;
    }

    private static async Task PostAsync(IPage page, Func<Task> act)
    {
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await act();
        await answered;
        await page.WaitForFunctionAsync("() => !document.querySelector('form[aria-busy=true]')", null, new() { PollingInterval = 20 });
    }

    [Fact]
    [Trait("AC", "BLOCKS-10")]
    public async Task EveryShellStandsAtShadcnsBoxesOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var shell in Shells.Where(static shell => shell != "sidebar-13"))
            {
                var boxes = (await ReferenceMetrics.OfAsync(shell, theme))
                    .Where(static element => element.Slot is "sidebar-menu-button" or "sidebar-menu-sub-button" or "sidebar-group-label" or "sidebar-trigger" && element.Width > 0)
                    .Select(static element => new object[] { element.X, element.Y, element.Width, element.Height, element.Text[..Math.Min(30, element.Text.Length)] }).ToArray();
                var page = await ShellAsync(shell, theme);
                var missed = await page.EvaluateAsync<string[]>(Misses, boxes);
                await page.Context.CloseAsync();
                wrong.AddRange(boxes.Length == 0 ? [$"{shell} {theme}: no boxes"] : missed.Select(miss => $"{shell} {theme}: nothing at {miss}"));
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BLOCKS-11")]
    public async Task EveryShellIsAGalleryPageAsync()
    {
        var found = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var page = await browser.PageAsync(gallery.Address, theme, 1400, 1000);
            foreach (var shell in Shells)
            {
                await page.GotoAsync(new Uri(gallery.Address, "blocks/" + shell).ToString());
                found.Add(shell + " " + await page.EvaluateAsync<string>("""
                    () => {
                      const main = document.querySelector("main"), content = main.getBoundingClientRect(), style = getComputedStyle(main)
                      const frame = [...main.querySelectorAll("div")].find((div) => Math.round(div.getBoundingClientRect().height) === 800)
                      const box = frame.getBoundingClientRect(), width = content.width - parseFloat(style.paddingLeft) - parseFloat(style.paddingRight)
                      const sidebar = [...frame.querySelectorAll("nav")].some((nav) => nav.getBoundingClientRect().height > 400)
                      const dialog = [...document.querySelectorAll("dialog")].some((dialog) => dialog.open && dialog.matches(":modal"))
                      const links = new Set([...document.querySelectorAll('a[href^="/blocks/"]')].map((link) => link.getAttribute("href"))).size
                      return [Math.round(box.width) === Math.round(width), parseFloat(getComputedStyle(frame).borderTopLeftRadius) > 0, sidebar || dialog, links].join()
                    }
                    """));
            }

            await page.Context.CloseAsync();
        }

        Assert.All(found, line => Assert.EndsWith(" true,true,true,27", line, StringComparison.Ordinal));
    }

    [Fact]
    [Trait("AC", "BLOCKS-13")]
    public async Task EveryShellsSidebarFoldsAndUnfoldsInTheBrowserAsync()
    {
        var found = new List<string>();
        foreach (var (shell, rail) in new[] { ("sidebar-07", true), ("sidebar-01", false), ("sidebar-14", true), ("sidebar-16", false), ("sidebar-15", false) })
        {
            var page = await ShellAsync(shell);
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            var trigger = page.Locator("#fixture button[aria-label='Toggle Sidebar']").Filter(new() { Visible = true });
            var steps = new List<string> { await page.EvaluateAsync<string>(Sidebars) };
            await (await SmallestAsync(trigger)).ClickAsync();
            steps.Add(await page.EvaluateAsync<string>(Sidebars));
            if (rail)
            {
                await (await RailAsync(page)).ClickAsync(new() { Force = true });
                steps.Add(await page.EvaluateAsync<string>(Sidebars));
            }

            found.Add($"{shell} {string.Join(" > ", steps)} requests {requests}");
            await page.Context.CloseAsync();
        }

        Assert.Equal(
        [
            "sidebar-07 256 1024 > 48 1232 > 256 1024 requests 0", "sidebar-01 256 1024 > 0 1280 requests 0", "sidebar-14 256 1024 > 0 1280 > 256 1024 requests 0",
            "sidebar-16 256 1024 > 0 1280 requests 0", "sidebar-15 256/256 768 > 0/256 1024 requests 0",
        ], found);
    }

    private static async Task<ILocator> SmallestAsync(ILocator buttons)
    {
        var count = await buttons.CountAsync();
        for (var index = 0; index < count; index++)
        {
            if ((await buttons.Nth(index).BoundingBoxAsync())?.Height < 100)
            {
                return buttons.Nth(index);
            }
        }

        return buttons.First;
    }

    private static async Task<ILocator> RailAsync(IPage page)
    {
        var buttons = page.Locator("#fixture button[aria-label='Toggle Sidebar']");
        var count = await buttons.CountAsync();
        for (var index = 0; index < count; index++)
        {
            if ((await buttons.Nth(index).BoundingBoxAsync())?.Height >= 100)
            {
                return buttons.Nth(index);
            }
        }

        return buttons.Last;
    }

    [Fact]
    [Trait("AC", "BLOCKS-15")]
    public async Task TheSwitchersPickTheVersionAndTheTeamOnTheWebAsync()
    {
        var found = new List<string>();
        foreach (var (shell, pick) in new[] { ("sidebar-01", "v2.0.0-beta1"), ("sidebar-07", "Acme Corp."), ("sidebar-10", "Acme Corp.") })
        {
            var page = await ShellAsync(shell);
            var switcher = page.Locator("#fixture nav button[popovertarget]").First;
            await switcher.ClickAsync();
            var menu = page.Locator("#" + await switcher.GetAttributeAsync("popovertarget"));
            await PostAsync(page, () => menu.GetByText(pick, new() { Exact = true }).First.ClickAsync());
            switcher = page.Locator("#fixture nav button[popovertarget]").First;
            var shown = string.Join(" ", (await switcher.InnerTextAsync()).Split(['\n', ' '], StringSplitOptions.RemoveEmptyEntries));
            var checks = await page.EvaluateAsync<string>("(id) => [...document.getElementById(id).querySelectorAll('[role^=menuitem]')].filter((item) => item.querySelector('svg')).map((item) => item.textContent.trim()).join(',')", await switcher.GetAttributeAsync("popovertarget"));
            var open = await page.EvaluateAsync<bool>("(id) => document.getElementById(id).matches(':popover-open')", await switcher.GetAttributeAsync("popovertarget"));
            found.Add($"{shell} {shown}{(shell == "sidebar-01" ? $" checked {checks}" : string.Empty)} open {open}");
            await page.Context.CloseAsync();
        }

        Assert.Equal(["sidebar-01 Documentation v2.0.0-beta1 checked v2.0.0-beta1 open False", "sidebar-07 Acme Corp. Startup open False", "sidebar-10 Acme Corp. open False"], found);
    }

    /// <summary>The links and buttons a part shows: visible, and not in closed details (whose content the browser does not render).</summary>
    private static async Task<string> ShownAsync(ILocator part) =>
        await part.EvaluateAsync<string>("(element) => [...element.querySelectorAll('a, button')].filter((link) => !link.closest('summary') && !link.hasAttribute('data-aioros-flips') && link.checkVisibility() && !(link.closest('details') && !link.closest('details').open)).map((link) => link.textContent.trim().replace(/^\\p{Extended_Pictographic}+/u, '')).join(',')");

    [Fact]
    [Trait("AC", "BLOCKS-17")]
    public async Task TheShellsCollapsiblePartsFoldAndUnfoldInTheBrowserAsync()
    {
        var found = new List<string>();
        var requests = 0;
        async Task<IPage> OpenAsync(string shell)
        {
            var page = await ShellAsync(shell);
            page.Request += (_, request) => requests += Browser.Sends(request);
            return page;
        }

        var group = await OpenAsync("sidebar-02");
        var details = group.Locator("#fixture details").First;
        await details.Locator("summary").ClickAsync();
        found.Add($"02 {await details.EvaluateAsync<bool>("(d) => d.open")} {await ShownAsync(details)} turn {await details.Locator("summary svg").First.EvaluateAsync<int>(Turn)}");
        await group.Context.CloseAsync();

        var plus = await OpenAsync("sidebar-05");
        var started = plus.Locator("#fixture [data-aioros-flag=IsExpanded]", new() { HasText = "Getting Started" }).First;
        const string Icon = "(item) => [...item.querySelector('[data-aioros-flips]').querySelectorAll('svg path')].filter((path) => path.checkVisibility()).map((path) => path.getAttribute('d').includes('M12 5') ? 'plus' : 'minus').join()";
        var icons = new List<string> { await started.EvaluateAsync<string>(Icon) };
        await started.Locator("[data-aioros-flips]").ClickAsync();
        icons.Add(await started.EvaluateAsync<string>(Icon) + " " + await ShownAsync(started));
        await started.Locator("[data-aioros-flips]").ClickAsync();
        icons.Add(await started.EvaluateAsync<string>(Icon));
        found.Add("05 " + string.Join(" > ", icons));
        await plus.Context.CloseAsync();

        var models = await OpenAsync("sidebar-07");
        var item = models.Locator("#fixture [data-aioros-flag=IsExpanded]", new() { HasText = "Models" }).First;
        await item.Locator("[data-aioros-flips]").ClickAsync();
        found.Add($"07 {await ShownAsync(item)} turn {await item.Locator("[data-aioros-flips]").EvaluateAsync<int>("(b) => { const turn = " + Turn + "; return Math.max(...[...b.querySelectorAll('svg')].map(turn)) }")}");
        await models.Context.CloseAsync();

        var workspace = await OpenAsync("sidebar-10");
        var life = workspace.Locator("#fixture [data-aioros-flag=IsExpanded]", new() { HasText = "Personal Life Management" }).First;
        await life.HoverAsync();
        var chevron = life.Locator("[data-aioros-flips]").First;
        var start = (await chevron.BoundingBoxAsync())!.X - (await life.BoundingBoxAsync())!.X;
        await chevron.ClickAsync();
        found.Add($"10 {start} {string.Join(",", (await ShownAsync(life)).Split(',').Skip(2))}");
        await workspace.Context.CloseAsync();

        var files = await OpenAsync("sidebar-11");
        var app = files.Locator("#fixture button[data-aioros-flips]", new() { HasText = "app" }).First;
        await app.ClickAsync();
        found.Add("11 " + await app.EvaluateAsync<string>("(b) => { const icons = [...b.children[0].querySelectorAll(':scope svg')].filter((svg) => svg.checkVisibility() && !svg.closest('foreignObject')); const turn = " + Turn + "; return b.getAttribute('aria-expanded') + ' ' + turn(icons[0]) + ' ' + (icons[0].getBoundingClientRect().x < icons[1].getBoundingClientRect().x) }"));
        await files.Context.CloseAsync();

        var calendars = await OpenAsync("sidebar-12");
        var favorites = calendars.Locator("#fixture details", new() { HasText = "Favorites" }).First;
        await favorites.Locator("summary").ClickAsync();
        found.Add($"12 {await favorites.EvaluateAsync<bool>("(d) => d.open")} {await ShownAsync(favorites)}");
        await calendars.Context.CloseAsync();

        Assert.Equal(
        [
            "02 False  turn 0", "05 plus > minus Installation,Project Structure > plus", "07 Genesis,Explorer,Quantum turn 90",
            "10 8 Daily Journal & Reflection,Health & Wellness Tracker,Personal Growth & Learning Goals", "11 true 90 true", "12 True Holidays,Birthdays",
        ], found);
        Assert.Equal(0, requests);
    }

    [Fact]
    [Trait("AC", "BLOCKS-19")]
    public async Task TheFoldersThePopoverAndTheDialogOpenAsTheBlocksDoOnTheWebAsync()
    {
        var notes = await ShellAsync("sidebar-10");
        var popover = await notes.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture [popover]')].filter((p) => p.matches(':popover-open')).map((p) => Math.round(p.getBoundingClientRect().width)).join()");
        await notes.Context.CloseAsync();
        var settings = await ShellAsync("sidebar-13");
        var dialog = await settings.EvaluateAsync<bool>("() => [...document.querySelectorAll('dialog')].some((d) => d.open && d.matches(':modal'))");
        await settings.Context.CloseAsync();
        var mail = await ShellAsync("sidebar-09");
        await (await SmallestAsync(mail.Locator("#fixture button[aria-label='Toggle Sidebar']"))).ClickAsync();
        var folded = await mail.EvaluateAsync<string>(Sidebars);
        await PostAsync(mail, () => mail.Locator("#fixture nav button").Filter(new() { HasTextRegex = new("^Drafts$") }).First.ClickAsync());
        var drafts = await mail.EvaluateAsync<string>("""
            () => {
              const title = [...document.querySelectorAll('#fixture *')].find((e) => !e.children.length && getComputedStyle(e).fontSize === '16px' && e.textContent.trim()).textContent
              const look = (text) => [...document.querySelectorAll('#fixture nav button')].find((b) => b.textContent.trim() === text).className
              return [title, look('Drafts') !== look('Inbox'), document.querySelector('#fixture [data-aioros-flag=IsOpen]').hasAttribute('data-aioros-on')].join()
            }
            """);
        var unfolded = await mail.EvaluateAsync<string>(Sidebars);
        await mail.Context.CloseAsync();

        Assert.Equal(("224", true, "48 1232", "Drafts,true,true", "350 930"), (popover, dialog, folded, drafts, unfolded));
    }

    [Fact]
    [Trait("AC", "SIDEBAR-12")]
    public async Task AToggleActionOpensItsSubMenuInTheBrowserAsync()
    {
        var page = await ShellAsync("sidebar-08");
        var requests = 0;
        var item = page.Locator("#fixture [data-aioros-flag=IsExpanded]", new() { HasText = "Models" }).First;
        var action = item.Locator("[data-aioros-flips]").First;
        page.Request += (_, request) => requests += Browser.Sends(request);
        var states = new List<string>();
        foreach (var _ in Enumerable.Range(0, 2))
        {
            await action.ClickAsync();
            states.Add($"{await ShownAsync(item)} {await action.EvaluateAsync<int>(Turn)}");
        }

        var sent = requests;
        var linked = await item.EvaluateAsync<int>("(li) => [...li.querySelectorAll('a, button')].filter((link) => !link.hasAttribute('data-aioros-flips')).length");
        await page.Context.CloseAsync();

        Assert.Equal(["Models,Genesis,Explorer,Quantum 90", "Models 0"], states);
        Assert.Equal((0, true), (sent, linked > 0));
    }

    [Fact]
    [Trait("AC", "SIDEBAR-14")]
    public async Task TheProvidersPartsAreDockedOnTheWebAsync()
    {
        const string Docked = """
            () => {
              const root = document.getElementById('fixture').getBoundingClientRect()
              const box = (element) => { const r = element.getBoundingClientRect(); return `${Math.round(r.x - root.x)},${Math.round(r.y - root.y)} ${Math.round(r.width)}x${Math.round(r.height)}` }
              const sidebars = [...document.querySelectorAll('#fixture nav')].filter((nav) => nav.getBoundingClientRect().height > 600).map(box)
              const header = [...document.querySelectorAll('#fixture header, #fixture div')].find((e) => Math.round(e.getBoundingClientRect().height) === 57 && Math.round(e.getBoundingClientRect().width) === 1280)
              return [...sidebars, box(document.querySelector('#fixture main')), header ? box(header) : 'no header'].join(' | ')
            }
            """;
        var found = new List<string>();
        foreach (var shell in (string[])["sidebar-16", "sidebar-15"])
        {
            var page = await ShellAsync(shell);
            found.Add(await page.EvaluateAsync<string>(Docked));
            await page.Context.CloseAsync();
        }

        Assert.Equal("0,57 256x743 | 256,57 1024x743 | 0,0 1280x57", found[0]);
        Assert.StartsWith("0,0 256x800 | 1024,0 256x800 | 256,0 768x800 | ", found[1], StringComparison.Ordinal);
    }
}
