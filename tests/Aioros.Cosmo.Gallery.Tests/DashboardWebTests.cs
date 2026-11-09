using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class DashboardWebTests(GalleryHost gallery, Browser browser)
{
    private const string Days = "() => Math.min(...[...[...document.querySelectorAll('#fixture svg')].find((svg) => svg.getBoundingClientRect().width > 300).querySelectorAll('path')].map((path) => (path.getAttribute('d') || '').split(/[LC]/).length))";

    private const string Pager = "() => [...document.querySelectorAll('#fixture *')].find((e) => !e.children.length && /^Page \\d+ of \\d+$/.test(e.textContent.trim())).textContent";

    private const string Selected = "() => [...document.querySelectorAll('#fixture *')].find((e) => !e.children.length && /row\\(s\\) selected\\.$/.test(e.textContent.trim())).textContent";

    private const string Headers = "() => [...document.querySelectorAll('#fixture tbody tr')].filter((row) => row.checkVisibility()).map((row) => row.querySelectorAll('td')[2].textContent.trim())";

    private async Task<IPage> DashboardAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "dashboard-01", "light", 800, 1560);
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
    [Trait("AC", "BLOCKS-21")]
    public async Task TheDashboardsChartAndTabsFollowTheirChoicesOnTheWebAsync()
    {
        var page = await DashboardAsync();
        var days = new List<int> { await page.EvaluateAsync<int>(Days) };
        foreach (var range in (string[])["Last 7 days", "Last 30 days"])
        {
            await PostAsync(page, () => page.Locator("#fixture label", new() { HasText = range }).ClickAsync());
            days.Add(await page.EvaluateAsync<int>(Days));
        }

        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        await page.Locator("#fixture [role=tab]", new() { HasText = "Key Personnel" }).ClickAsync();
        var shown = await page.EvaluateAsync<string>("() => [[...document.querySelectorAll('#fixture table')].some((t) => t.checkVisibility()), [...document.querySelectorAll('#fixture *')].filter((e) => e.checkVisibility() && getComputedStyle(e).borderTopStyle === 'dashed').length].join()");
        await page.Context.CloseAsync();

        Assert.Equal([91, 8, 31], days);
        Assert.Equal(("false,1", 1), (shown, requests));
    }

    [Fact]
    [Trait("AC", "BLOCKS-23")]
    public async Task TheDashboardsTablePagesSelectsHidesAssignsAndReordersOnTheWebAsync()
    {
        var page = await DashboardAsync();
        var steps = new List<string>
        {
            $"{(await page.EvaluateAsync<string[]>(Headers)).Length} {await page.EvaluateAsync<string>(Pager)} "
            + await page.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture tbody tr')].slice(0, 2).map((row) => row.querySelectorAll('td')[4].textContent.trim() + ':' + [...row.querySelectorAll('td')[4].querySelectorAll('svg')].filter((svg) => svg.checkVisibility() && !svg.closest('foreignObject')).length).join(' ')")
            + $" selects {await page.Locator("#fixture tbody select[aria-label=Reviewer]").CountAsync()}",
        };
        await PostAsync(page, () => page.Locator("#fixture button[aria-label='Go to last page']").ClickAsync());
        steps.Add($"{await page.EvaluateAsync<string>(Pager)} {await page.Locator("#fixture button[aria-label='Go to next page']").IsDisabledAsync()} {await page.Locator("#fixture button[aria-label='Go to last page']").IsDisabledAsync()}");
        await PostAsync(page, () => page.Locator("#fixture button[aria-label='Go to first page']").ClickAsync());
        steps.Add(await page.EvaluateAsync<string>(Pager));
        var size = page.Locator("#fixture select:not([aria-label])").First;
        await PostAsync(page, () => size.SelectOptionAsync(new SelectOptionValue { Label = "20" }));
        steps.Add($"{await page.EvaluateAsync<string>(Pager)} {(await page.EvaluateAsync<string[]>(Headers)).Length}");
        await PostAsync(page, () => size.SelectOptionAsync(new SelectOptionValue { Label = "10" }));
        foreach (var row in (int[])[0, 1])
        {
            await PostAsync(page, () => page.Locator("#fixture tbody input[type=checkbox]").Nth(row).CheckAsync());
        }

        steps.Add(await page.EvaluateAsync<string>(Selected));
        var columns = page.Locator("#fixture button", new() { HasText = "Customize Columns" }).First;
        await columns.ClickAsync();
        var menu = page.Locator("#" + await columns.GetAttributeAsync("popovertarget"));
        await PostAsync(page, () => menu.GetByText("Status", new() { Exact = true }).ClickAsync());
        steps.Add("columns " + string.Join("|", await page.EvaluateAsync<string[]>("() => [...document.querySelectorAll('#fixture thead th')].filter((th) => th.checkVisibility() && th.textContent.trim()).map((th) => th.textContent.trim())")));
        var reviewer = page.Locator("#fixture tbody select[aria-label=Reviewer]").First;
        await PostAsync(page, () => reviewer.SelectOptionAsync(new SelectOptionValue { Label = "Eddie Lake" }));
        steps.Add("reviewer " + await page.Locator("#fixture tbody select[aria-label=Reviewer]").First.EvaluateAsync<string>("(s) => s.options[s.selectedIndex].textContent"));
        var handle = page.Locator("#fixture tbody [data-drag]").First;
        var key = await handle.GetAttributeAsync("data-drag");
        var first = (await page.EvaluateAsync<string[]>(Headers))[0];
        await handle.FocusAsync();
        await page.Keyboard.PressAsync("Space");
        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("ArrowDown");
        await PostAsync(page, () => page.Keyboard.PressAsync("Space"));
        var order = await page.EvaluateAsync<string[]>(Headers);
        steps.Add($"moved {key} {Array.IndexOf(order, first)} {order[0]} {await page.EvaluateAsync<string>(Selected)}");
        var inputs = await page.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture tbody tr')].slice(0, 3).map((row) => [...row.querySelectorAll('input[type=text]')].map((input) => input.getAttribute('aria-label') + '=' + input.value + ':' + (input.name.length > 1)).join(' ')).join(' | ')");
        await page.Context.CloseAsync();

        Assert.Equal(
        [
            "10 Page 1 of 7 In Process:1 Done:1 selects 3", "Page 7 of 7 True True", "Page 1 of 7", "Page 1 of 4 20", "2 of 68 row(s) selected.",
            "columns Header|Section Type|Target|Limit|Reviewer", "reviewer Eddie Lake", "moved 1 2 Table of contents 2 of 68 row(s) selected.",
        ], steps);
        Assert.Equal("Target=29:true Limit=24:true | Target=10:true Limit=13:true | Target=18:true Limit=5:true", inputs);
    }

    [Fact]
    [Trait("AC", "DRAWER-12")]
    public async Task TheViewerIsADrawerFromTheRightOnTheWebAsync()
    {
        var page = await DashboardAsync();
        await PostAsync(page, () => page.Locator("#fixture tbody button", new() { HasText = "Cover page" }).First.ClickAsync());
        await Browser.AtRestAsync(page);
        var viewer = await page.EvaluateAsync<string>("""
            () => {
              const root = document.getElementById('fixture').getBoundingClientRect(), dialog = [...document.querySelectorAll('dialog')].find((d) => d.open)
              const surface = dialog.querySelector('[data-aioros-swipe]'), box = surface.getBoundingClientRect()
              const handles = [...surface.querySelectorAll('*')].filter((e) => { const r = e.getBoundingClientRect(); return r.height <= 8 && r.width >= 50 && r.width <= 120 && getComputedStyle(e).backgroundColor !== 'rgba(0, 0, 0, 0)' }).length
              const title = [...surface.querySelectorAll('*')].some((e) => !e.children.length && e.textContent.trim() === 'Cover page')
              const target = [...surface.querySelectorAll('input')].find((input) => input.getAttribute('aria-label') === 'Target')?.value
              return [surface.dataset.aiorosSwipe, Math.round(box.right - root.right), Math.round(box.y - root.y), Math.round(box.width), Math.round(box.height), handles, title, target].join()
            }
            """);
        await page.Context.CloseAsync();

        Assert.Equal("right,0,0,384,800,0,true,18", viewer);
    }

    [Fact]
    [Trait("AC", "DATATABLE-08")]
    public async Task TheTablesStateTravelsAcrossPostsAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/data-table").ToString());
        await Browser.KeysAsync(page);
        const string Shown = "() => [...document.querySelectorAll('main tbody tr')].map((row) => [...row.querySelectorAll('td')].filter((cell) => cell.checkVisibility()).map((cell) => cell.textContent.trim().split('@')[0]).filter(Boolean).slice(0, 2).join(':')).join(',')";
        var steps = new List<string>();
        await PostAsync(page, () => page.Locator("main button:has-text('Next')").Last.ClickAsync());
        steps.Add(await page.EvaluateAsync<string>(Shown));
        await PostAsync(page, () => page.Locator("main thead button", new() { HasText = "Email" }).First.ClickAsync());
        steps.Add(await page.EvaluateAsync<string>(Shown));
        await PostAsync(page, () => page.Locator("main button:has-text('Next')").Last.ClickAsync());
        steps.Add(await page.EvaluateAsync<string>(Shown));
        var columns = page.Locator("main button", new() { HasText = "Columns" }).First;
        await columns.ClickAsync();
        var menu = page.Locator("#" + await columns.GetAttributeAsync("popovertarget"));
        await PostAsync(page, () => menu.GetByText("Status", new() { Exact = true }).ClickAsync());
        steps.Add(await page.EvaluateAsync<string>(Shown));
        await PostAsync(page, () => page.Locator("main tbody input[type=checkbox]").First.CheckAsync());
        steps.Add(await page.EvaluateAsync<string>(Shown) + " " + await page.EvaluateAsync<string>("() => [...document.querySelectorAll('main *')].find((e) => !e.children.length && /row\\(s\\) selected\\.$/.test(e.textContent.trim())).textContent"));
        var checkedRow = await page.EvaluateAsync<string>("() => [...document.querySelectorAll('main tbody tr')].find((row) => row.querySelector('input[type=checkbox]').checked)?.textContent.match(/[a-z0-9.]+@/i)?.[0]");
        await page.Context.CloseAsync();

        Assert.Equal(
        [
            "Pending:olivia,Success:liam,Failed:noah,Processing:emma,Success:ava",
            "Success:abe45,Success:ava,Failed:carmella,Processing:emma,Success:ken99",
            "Success:liam,Pending:lucas,Success:mia,Processing:monserrat44,Failed:noah",
            "liam:$456.00,lucas:$932.00,mia:$188.00,monserrat44:$837.00,noah:$98.00",
            "liam:$456.00,lucas:$932.00,mia:$188.00,monserrat44:$837.00,noah:$98.00 1 of 12 row(s) selected.",
        ], steps);
        Assert.Equal("liam@", checkedRow);
    }
}
