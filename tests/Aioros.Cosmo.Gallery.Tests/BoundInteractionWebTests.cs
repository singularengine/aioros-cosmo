using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>The Gallery's bound examples on the web: a drag, a click or a key posted to the model and the page following it.</summary>
public sealed class BoundInteractionWebTests(GalleryHost gallery, Browser browser)
{
    [Fact]
    [Trait("AC", "RESIZABLE-06")]
    public async Task ADragOnTheBoundGroupIsPostedToItsSizeAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/resizable").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        const string Text = "[...document.querySelectorAll('main div')].map((d) => d.textContent).find((t) => /^Sidebar: \\d+%$/.test(t)) ?? ''";
        var handle = page.Locator("main [role=separator]:has(input[data-change])");
        await handle.ScrollIntoViewIfNeededAsync();
        var box = (await handle.BoundingBoxAsync())!;
        var (x, y) = (box.X + box.Width / 2, box.Y + box.Height / 2);
        await page.Mouse.MoveAsync(x, y);
        await page.Mouse.DownAsync();
        await page.Mouse.MoveAsync(x + 80, y, new() { Steps = 8 });
        var dragged = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.Mouse.UpAsync();
        await dragged;
        var size = double.Parse((await handle.GetAttributeAsync("aria-valuenow"))!, System.Globalization.CultureInfo.InvariantCulture);
        var expected = $"Sidebar: {Math.Round(size, MidpointRounding.AwayFromZero)}%";
        await page.WaitForFunctionAsync($"({Text}) === '{expected}'");
        var afterDrag = (await page.EvaluateAsync<string>(Text), requests);
        var keyed = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await handle.FocusAsync();
        await page.Keyboard.PressAsync("ArrowRight");
        await keyed;
        var stepped = $"Sidebar: {Math.Round(size + 5, MidpointRounding.AwayFromZero)}%";
        await page.WaitForFunctionAsync($"({Text}) === '{stepped}'");
        var afterKey = (await page.EvaluateAsync<string>(Text), requests);
        await page.Context.CloseAsync();

        Assert.True(size > 30, $"dragged to {size}");
        Assert.Equal(((expected, 1), (stepped, 2)), (afterDrag, afterKey));
    }

    [Fact]
    [Trait("AC", "CAROUSEL-07")]
    public async Task TheBoundCarouselsIndicatorsAndSlidesFollowTheModelOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/carousel").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        const string Text = "[...document.querySelectorAll('main div')].map((d) => d.textContent).find((t) => /^Slide \\d+ of \\d+$/.test(t)) ?? ''";
        var carousel = page.Locator("main [aria-roledescription=carousel]").Last;
        var moved = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await carousel.Locator("button[data-aioros-slide-to]").Nth(3).ClickAsync();
        await moved;
        await page.WaitForFunctionAsync($"({Text}) === 'Slide 4 of 5'");
        var dots = await carousel.EvaluateAsync<string[]>("(c) => [...c.querySelectorAll('button[data-aioros-slide-to]')].map((b) => getComputedStyle(b.firstElementChild ?? b).backgroundColor)");
        var primary = dots.Select(dot => dot == dots[3]).ToArray();
        var indicated = (await page.EvaluateAsync<string>(Text), requests, string.Join(',', primary), dots[3] != dots[0]);
        var added = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.Locator("main button:text-is('Add a slide')").ClickAsync();
        await added;
        await page.WaitForFunctionAsync($"({Text}) === 'Slide 4 of 6'");
        var grown = await carousel.EvaluateAsync<int[]>("(c) => [c.querySelectorAll('[aria-roledescription=slide]').length, c.querySelectorAll('button[data-aioros-slide-to]').length]");
        await page.Context.CloseAsync();

        Assert.Equal((("Slide 4 of 5", 1, "False,False,False,True,False", true), "6,6"), (indicated, string.Join(',', grown)));
    }

    [Fact]
    [Trait("AC", "COMMAND-10")]
    public async Task ADialogWithoutATriggerOpensWithItsKeyOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/command").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        var shortcut = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('dialog[aria-keyshortcuts]')].map((d) => d.getAttribute('aria-keyshortcuts'))");
        await Browser.KeysAsync(page);
        await page.Keyboard.PressAsync("Control+i");
        await page.WaitForSelectorAsync("dialog[open]");
        var opened = await page.Locator("dialog[open]").CountAsync();
        await page.Keyboard.PressAsync("Control+i");
        await page.WaitForSelectorAsync("dialog[open]", new() { State = WaitForSelectorState.Detached });
        var closed = await page.Locator("dialog[open]").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(("Control+I", 1, 0, 0), (string.Join(',', shortcut), opened, closed, requests));
    }

    [Fact]
    [Trait("AC", "COMMAND-11")]
    public async Task TheBoundCommandPostsItsTextAndChosenItemAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/command").ToString());
        var input = page.Locator("main input[placeholder='Search the Gallery...']");
        await input.FillAsync("ar");
        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("ArrowDown");
        var posted = page.WaitForRequestAsync(request => request.Method == "POST");
        await page.Keyboard.PressAsync("Enter");
        var body = (await posted).PostData ?? string.Empty;
        await page.WaitForSelectorAsync("main :text-is('Opened: /components/accordion')");
        var texts = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('main div')].map((d) => d.textContent).filter((t) => /^(Opened|Value): /.test(t))");
        await page.Context.CloseAsync();

        Assert.Equal((true, true, "Opened: /components/accordion|Value: Accordion"), (body.Contains("=ar&", StringComparison.Ordinal), body.Contains("%2Fcomponents%2Faccordion", StringComparison.Ordinal), string.Join('|', texts)));
    }

    [Fact]
    [Trait("AC", "SIDEBAR-10")]
    public async Task ABoundCollapsibleItemIsPostedToTheModelAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/sidebar").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        const string Text = "[...document.querySelectorAll('main div')].map((d) => d.textContent).find((t) => /^Playground: \\w+$/.test(t)) ?? ''";
        var button = page.Locator("main button[aria-expanded]:has-text('Playground')").Last;
        var states = new List<string>();
        foreach (var expected in (string[])["closed", "open"])
        {
            var posted = page.WaitForResponseAsync(response => response.Request.Method == "POST");
            await button.ClickAsync();
            await posted;
            await page.WaitForFunctionAsync($"({Text}) === 'Playground: {expected}'");
            states.Add($"{await button.GetAttributeAsync("aria-expanded")}:{await page.EvaluateAsync<string>(Text)}:{requests}");
        }

        await page.Context.CloseAsync();

        Assert.Equal("false:Playground: closed:1,true:Playground: open:2", string.Join(',', states));
    }
}
