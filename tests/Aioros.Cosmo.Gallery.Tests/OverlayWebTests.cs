using System.Text.RegularExpressions;
using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed partial class OverlayWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    /// <summary>The differences between an open surface's computed look and shadcn/ui's element: background, border, radius and padding.</summary>
    internal static IEnumerable<string> Look(string what, ElementMetrics found, ElementMetrics expected) =>
        new[]
        {
            ("background", found.Background, expected.Background, true), ("border", found.BorderColor, expected.BorderColor, Compare.Px(expected.BorderWidth) > 0),
        }
        .Where(pair => pair.Item4 && !CssColor.Parse(pair.Item2).Near(CssColor.Parse(pair.Item3))).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}")
        .Concat(new[] { ("radius", found.Radius, expected.Radius), ("border width", found.BorderWidth, expected.BorderWidth), ("padding left", found.PaddingLeft, expected.PaddingLeft), ("padding top", found.PaddingTop, expected.PaddingTop) }
            .Where(pair => pair.Item2 != pair.Item3).Select(pair => $"{what}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"));

    [Fact]
    [Trait("AC", "POPOVER-04")]
    public async Task AFlyoutIsAnAnchoredPopoverOpenedWithoutARequestAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/popover", TestContext.Current.CancellationToken);
        var trigger = TriggerTag().Match(html);
        Assert.True(trigger.Success, "no popover trigger");
        var id = trigger.Groups[1].Value;
        Assert.Contains($"anchor-name:--{id}", trigger.Value, StringComparison.Ordinal);
        var content = html[html.IndexOf($"<div popover id=\"{id}\"", StringComparison.Ordinal)..];
        Assert.Contains($"position-anchor:--{id}", content[..content.IndexOf('>', StringComparison.Ordinal)], StringComparison.Ordinal);
        Assert.Matches("<input[^>]* name=\"f\\d+\"[^>]* value=\"100%\"", content[..content.IndexOf("&lt;Button", StringComparison.Ordinal)]);

        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("popover-open", theme, "popover-content"))[0];
            var page = await browser.PageAsync(gallery.Address, theme, 1280);
            var requests = 0;
            page.Request += (_, request) => requests += request.ResourceType == "document" || request.Method == "POST" ? 1 : 0;
            await page.GotoAsync(new Uri(gallery.Address, "components/popover").ToString());
            requests = 0;
            var button = page.Locator($"button[popovertarget='{id}']");
            await button.ClickAsync();
            await page.WaitForSelectorAsync($"#{id}:popover-open");
            var found = (await Browser.PickAsync(page, $"#{id}"))[0];
            wrong.AddRange(Look($"{theme} popover", found, expected));
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForSelectorAsync($"#{id}:not(:popover-open)", new() { State = WaitForSelectorState.Attached });
            var focused = await page.EvaluateAsync<string?>("document.activeElement?.getAttribute('popovertarget')");
            await button.ClickAsync();
            await page.WaitForSelectorAsync($"#{id}:popover-open");
            await page.Mouse.ClickAsync(1260, 880);
            await page.WaitForSelectorAsync($"#{id}:not(:popover-open)", new() { State = WaitForSelectorState.Attached });
            wrong.AddRange(focused == id && requests == 0 ? [] : [$"{theme}: focus back on {focused}, {requests} requests"]);
            await page.Context.CloseAsync();
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "KBD-06")]
    public async Task AKeyInATipIsShadcnsTooltipKeyOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("kbd-tooltip-open", theme, "kbd"))[0];
            var content = (await ReferenceMetrics.SlotsAsync("kbd-tooltip-open", theme, "tooltip-content"))[0];
            var page = await browser.OverlayAsync(gallery.Address, "kbd-tooltip-open", theme, new("hover", "button", null, false));
            var found = (await Browser.PickAsync(page, "[role=tooltip] kbd > div")).Single();
            var bubble = (await Browser.PickAsync(page, "[role=tooltip]")).Single();
            var text = (await Browser.PickAsync(page, "[role=tooltip] kbd div:not(:has(*))")).Single();
            await page.Context.CloseAsync();
            wrong.AddRange(Compare.Box($"{theme} key in its bubble", found with { X = found.X - bubble.X, Y = found.Y - bubble.Y }, expected with { X = expected.X - content.X, Y = expected.Y - content.Y }, 1));
            wrong.AddRange(CssColor.Parse(found.Background).Near(CssColor.Parse(expected.Background)) ? [] : [$"{theme} key: background {found.Background}, shadcn {expected.Background}"]);
            wrong.AddRange(CssColor.Parse(text.Color).Near(CssColor.Parse(expected.Color)) ? [] : [$"{theme} key: color {text.Color}, shadcn {expected.Color}"]);
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOOLTIP-04")]
    public async Task ATipIsShownOnHoverAndFocusWithShadcnsLookAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("tooltip-open", theme, "tooltip-content"))[0];
            var page = await browser.PageAsync(gallery.Address, theme, 1280);
            await page.GotoAsync(new Uri(gallery.Address, "components/tooltip").ToString());
            await page.AddStyleTagAsync(new() { Content = "*,*::before,*::after{transition:none!important}" });
            var trigger = page.Locator(".tip > button").First;
            var tip = page.Locator(".tip > [role=tooltip]").First;
            var hidden = await tip.EvaluateAsync<string>("el => getComputedStyle(el).visibility");
            await trigger.HoverAsync();
            var hovered = await tip.EvaluateAsync<string>("el => getComputedStyle(el).visibility + ' ' + el.textContent");
            var found = (await Browser.PickAsync(page, ".tip > [role=tooltip]"))[0];
            await page.Mouse.MoveAsync(1260, 880);
            await trigger.FocusAsync();
            var focused = await tip.EvaluateAsync<string>("el => getComputedStyle(el).visibility");
            await page.Context.CloseAsync();
            wrong.AddRange(Look($"{theme} tip", found, expected));
            wrong.AddRange(new[] { ("color", found.Color, expected.Color) }.Where(pair => !CssColor.Parse(pair.Item2).Near(CssColor.Parse(pair.Item3))).Select(pair => $"{theme}: {pair.Item1} {pair.Item2}, shadcn {pair.Item3}"));
            wrong.AddRange(found.FontSize == expected.FontSize ? [] : [$"{theme}: font {found.FontSize}, shadcn {expected.FontSize}"]);
            wrong.AddRange((hidden, hovered, focused) == ("hidden", "visible Add to library", "visible") ? [] : [$"{theme}: {hidden} / {hovered} / {focused}"]);
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "HOVERCARD-04")]
    public async Task TheCardIsAHintPopoverOpenedOnHoverAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/hover-card", TestContext.Current.CancellationToken);
        var main = html[html.IndexOf("<main", StringComparison.Ordinal)..];
        var trigger = Regex.Match(main, "<span class=\"hv\" data-aioros-overlay data-aioros-hover=\"(m\\d+)\" data-open=\"700\" data-close=\"300\"><a href=\"https://nextjs.org/\"[^>]*>@nextjs</a><div popover=\"hint\" id=\"\\1\" role=\"dialog\"", RegexOptions.None, TimeSpan.FromSeconds(1));
        Assert.True(trigger.Success, "no hover trigger with its card");
        var id = trigger.Groups[1].Value;

        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/hover-card").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.ResourceType == "document" || request.Method == "POST" ? 1 : 0;
        var closed = await page.Locator($"#{id}:popover-open").CountAsync();
        await page.Locator("main a[href='https://nextjs.org/']").First.HoverAsync();
        await page.WaitForSelectorAsync($"#{id}:popover-open");
        var text = await page.Locator($"#{id}").InnerTextAsync();
        await page.Context.CloseAsync();

        Assert.Equal((0, 0), (closed, requests));
        Assert.Contains("The React Framework", text, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "DROPDOWN-05")]
    public async Task AMenuFlyoutIsAPopoverOfPostingItemsAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/dropdown-menu", TestContext.Current.CancellationToken);
        var trigger = Regex.Match(html, "<button type=\"button\" popovertarget=\"(m\\d+)\"[^>]*>Actions</button>", RegexOptions.None, TimeSpan.FromSeconds(1));
        Assert.True(trigger.Success, "no menu trigger");
        var id = trigger.Groups[1].Value;
        var menu = html[html.IndexOf($"<div popover id=\"{id}\"", StringComparison.Ordinal)..];
        menu = menu[..(menu.IndexOf("</div>", StringComparison.Ordinal) + 6)];
        Assert.Contains($"position-anchor:--{id}", menu, StringComparison.Ordinal);
        Assert.Matches($"<div popover id=\"{id}\" role=\"menu\"", menu);
        Assert.Matches("<button type=\"submit\" name=\"_btn\" value=\"\\d+\\|Duplicate\" role=\"menuitem\" tabindex=\"-1\" class=\"mi[^\"]*\"><span class=\"mh\">Duplicate</span><kbd class=\"kg\"[^>]*>Ctrl\\+D</kbd></button>", menu);
        Assert.Matches("<button type=\"submit\" name=\"_btn\" value=\"\\d+\\|Archive\"[^>]*><span class=\"mh\">Archive</span></button><hr[^>]*><button[^>]* disabled><span class=\"mh\">Delete</span></button>", menu);
        Assert.Matches("<button type=\"button\" role=\"menuitem\" aria-haspopup=\"menu\"[^>]* class=\"mi[^\"]*\" popovertarget=\"(m\\d+)\"[^>]*><span class=\"mh\">Invite users</span>", html);
        Assert.Matches("role=\"menuitemcheckbox\" aria-checked=\"true\"[^>]*><svg[^>]*>(?:(?!</svg>).)*<path d=\"M13.333 4L6 11.333L2.667 8\"[^>]*/>(?:(?!</svg>).)*</svg><span class=\"mh\">Status Bar</span>", html);

        var expected = (await ReferenceMetrics.SlotsAsync("dropdown-menu-open", "light", "dropdown-menu-content"))[0];
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/dropdown-menu").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.ResourceType == "document" || request.Method == "POST" ? 1 : 0;
        await page.Locator($"button[popovertarget='{id}']").ClickAsync();
        await page.WaitForSelectorAsync($"#{id}:popover-open");
        var found = (await Browser.PickAsync(page, $"#{id}"))[0];
        var opened = requests;
        await page.Locator($"#{id} button", new() { HasText = "Archive" }).ClickAsync();
        await page.WaitForSelectorAsync("text=You chose Archive");
        await page.Context.CloseAsync();

        Assert.Equal(0, opened);
        Assert.Empty(new[] { ("background", found.Background, expected.Background), ("border", found.BorderColor, expected.BorderColor) }
            .Where(pair => !CssColor.Parse(pair.Item2).Near(CssColor.Parse(pair.Item3))).Select(pair => $"{pair.Item1} {pair.Item2}, shadcn {pair.Item3}"));
    }

    [Fact]
    [Trait("AC", "CONTEXTMENU-04")]
    public async Task TheAreaOpensItsMenuOnRightClickAsync()
    {
        var html = await gallery.Client.GetStringAsync("components/context-menu", TestContext.Current.CancellationToken);
        var main = html[html.IndexOf("<main", StringComparison.Ordinal)..];
        var area = Regex.Match(main, "<span class=\"ctx\" data-aioros-overlay data-aioros-context=\"(m\\d+)\">(?:(?!</span>).)*>Right click here</div>", RegexOptions.None, TimeSpan.FromSeconds(1));
        Assert.True(area.Success, "no context area");
        var id = area.Groups[1].Value;
        var menu = main[main.IndexOf($"<div popover id=\"{id}\" role=\"menu\"", StringComparison.Ordinal)..];
        Assert.Contains(">Show Bookmarks</span>", menu, StringComparison.Ordinal);
        Assert.Matches("<button type=\"submit\" name=\"_btn\" value=\"\\d+\\|Copy\" role=\"menuitem\"", menu);

        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/context-menu").ToString());
        var requests = 0;
        page.Request += (_, request) => requests += request.ResourceType == "document" || request.Method == "POST" ? 1 : 0;
        await page.Locator("main div", new() { HasTextRegex = new Regex("^Right click here$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync(new() { Button = MouseButton.Right });
        await page.WaitForSelectorAsync($"#{id}:popover-open");
        await page.Context.CloseAsync();

        Assert.Equal(0, requests);
    }

    /// <summary>Presses a button that opens a dialog (a post whose command waits on the dialog) and waits for the modal dialog.</summary>
    private static async Task<ILocator> PromptAsync(IPage page, string trigger)
    {
        await page.Locator("main button", new() { HasTextRegex = new Regex($"^{Regex.Escape(trigger)}$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync();
        await page.WaitForSelectorAsync("dialog.prompt:modal");
        await Browser.AtRestAsync(page);
        return page.Locator("dialog.prompt[open]");
    }

    /// <summary>The computed look of the dialog's surface: the element inside the prompt with the surface's radius.</summary>
    private static async Task<ElementMetrics> SurfaceAsync(IPage page, string radius)
    {
        var all = await Browser.PickAsync(page, "dialog.prompt[open] div");
        return all.First(element => element.Radius == radius && element.Background != "rgba(0, 0, 0, 0)");
    }

    /// <summary>Presses a button of the open dialog (a post replaying the steps that opened it) and waits for the answer.</summary>
    private static async Task AnswerAsync(IPage page, string button)
    {
        await page.Locator("dialog.prompt[open] button", new() { HasTextRegex = new Regex($"^{Regex.Escape(button)}$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync();
        await page.WaitForLoadStateAsync(LoadState.Load);
    }

    [Fact]
    [Trait("AC", "DIALOG-08")]
    public async Task TheDialogsTriggerOpensItInTheBrowserAsync() =>
        Assert.Equal(["in", "About", "outside", "closed", 0], await InTheBrowserAsync("components/dialog", "About", "Done", dismisses: true));

    [Fact]
    [Trait("AC", "ALERTDIALOG-07")]
    public async Task TheAlertDialogsTriggerOpensItInTheBrowserAsync() =>
        Assert.Equal(["in", "Discard draft", "kept", "closed", 0], await InTheBrowserAsync("components/alert-dialog", "Discard draft", "Keep", dismisses: false));

    [Fact]
    [Trait("AC", "SHEET-07")]
    public async Task TheSheetsTriggerOpensItInTheBrowserAsync() =>
        Assert.Equal(["in", "Settings", "outside", "closed", 0], await InTheBrowserAsync("components/sheet", "Settings", "Done", dismisses: true));

    [Fact]
    [Trait("AC", "DRAWER-07")]
    public async Task TheDrawersTriggerOpensItInTheBrowserAsync() =>
        Assert.Equal(["in", "Shortcuts", "outside", "closed", 0], await InTheBrowserAsync("components/drawer", "Shortcuts", "Done", dismisses: true));

    /// <summary>
    /// Opens an overlay page's browser example by its trigger and closes it by Escape, a click outside and its closing
    /// button, as the focus inside it, the focus after Escape, what the click outside did, whether the closing button
    /// closed it, and the requests the page sent (the closed content found by role counts as one).
    /// </summary>
    private async Task<List<object>> InTheBrowserAsync(string path, string trigger, string close, bool dismisses)
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, path).ToString());
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        var opener = page.Locator("main button", new() { HasTextRegex = new Regex($"^{Regex.Escape(trigger)}$", RegexOptions.None, TimeSpan.FromSeconds(1)) });
        var dialog = page.Locator($"dialog[id='{await opener.GetAttributeAsync("commandfor")}']");
        var hidden = await page.GetByRole(AriaRole.Button, new() { Name = close, Exact = true }).CountAsync();
        await opener.ClickAsync();
        await page.WaitForFunctionAsync("d => d.matches(':modal')", await dialog.ElementHandleAsync());
        await Browser.AtRestAsync(page);
        var focus = await dialog.EvaluateAsync<bool>("d => d.contains(document.activeElement)") ? "in" : "out";
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForFunctionAsync("d => !d.open", await dialog.ElementHandleAsync());
        await Browser.AtRestAsync(page);
        var back = await page.EvaluateAsync<string>("() => document.activeElement?.textContent.trim() ?? ''");
        await opener.ClickAsync();
        await page.WaitForFunctionAsync("d => d.matches(':modal')", await dialog.ElementHandleAsync());
        await Browser.AtRestAsync(page);
        await page.Mouse.ClickAsync(4, 4);
        var outside = await dialog.EvaluateAsync<bool>("d => d.open") ? "kept" : "outside";
        if (outside == "outside" != dismisses)
        {
            outside += " (unexpected)";
        }

        if (await dialog.EvaluateAsync<bool>("d => !d.open"))
        {
            await opener.ClickAsync();
            await page.WaitForFunctionAsync("d => d.matches(':modal')", await dialog.ElementHandleAsync());
            await Browser.AtRestAsync(page);
        }

        await dialog.Locator("button", new() { HasTextRegex = new Regex($"^{Regex.Escape(close)}$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).ClickAsync();
        await page.WaitForFunctionAsync("d => !d.open", await dialog.ElementHandleAsync());
        await Browser.AtRestAsync(page);
        var closed = await dialog.EvaluateAsync<bool>("d => d.open") ? "open" : "closed";
        await page.Context.CloseAsync();
        return [focus, back, outside, closed, requests + hidden];
    }

    [Fact]
    [Trait("AC", "DIALOG-04")]
    public async Task APendingCommandsDialogIsAModalDialogElementAsync()
    {
        var expected = (await ReferenceMetrics.SlotsAsync("dialog-open", "light", "dialog-content"))[0];
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/dialog").ToString());
        var prompt = await PromptAsync(page, "Open Dialog");
        var mask = await prompt.Locator("div").First.EvaluateAsync<string>("el => getComputedStyle(el).backgroundColor");
        var fields = await prompt.Locator("input[name^='f']:not([hidden])").CountAsync();
        var found = await SurfaceAsync(page, "10px");
        await AnswerAsync(page, "Cancel");
        await page.WaitForSelectorAsync("text=Edit profile: cancel");
        var left = await page.Locator("dialog.prompt[open]").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal("rgba(0, 0, 0, 0.5)", mask);
        Assert.Equal(2, fields);
        Assert.Empty(Look("dialog", found, expected));
        Assert.Equal(0, left);
    }

    [Fact]
    [Trait("AC", "DIALOG-05")]
    public async Task TheDialogKeepsItsMarginOnAPhoneAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 390);
        await page.GotoAsync(new Uri(gallery.Address, "components/dialog").ToString());
        await PromptAsync(page, "Open Dialog");
        var edges = await page.EvaluateAsync<double[]>("(() => { const d = document.querySelector('dialog.prompt[open]').getBoundingClientRect(); const s = [...document.querySelectorAll('dialog.prompt[open] div')].find((el) => getComputedStyle(el).borderTopLeftRadius === '10px' && getComputedStyle(el).backgroundColor !== 'rgba(0, 0, 0, 0)').getBoundingClientRect(); return [s.left - d.left, d.right - s.right, s.left, document.documentElement.clientWidth - s.right] })()");
        await page.Context.CloseAsync();

        Assert.All(edges, edge => Assert.InRange(edge, 15.5, 16.5));
    }

    [Fact]
    [Trait("AC", "ALERTDIALOG-04")]
    public async Task TheAlertDialogAsksAndAnswersOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/alert-dialog").ToString());
        var prompt = await PromptAsync(page, "Show Dialog");
        var text = await prompt.InnerTextAsync();
        var buttons = await prompt.Locator("button").AllInnerTextsAsync();
        var close = await prompt.Locator("button[aria-label='Close']").CountAsync();
        var role = await prompt.GetAttributeAsync("role");
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForSelectorAsync("text=Delete account: closed");
        var escaped = await page.Locator("dialog.prompt[open]").CountAsync();
        await PromptAsync(page, "Show Dialog");
        await AnswerAsync(page, "Continue");
        await page.WaitForSelectorAsync("text=Delete account: continue");
        var left = await page.Locator("dialog.prompt[open]").CountAsync();
        await page.Context.CloseAsync();

        Assert.Contains("Are you absolutely sure?", text, StringComparison.Ordinal);
        Assert.Contains("This action cannot be undone.", text, StringComparison.Ordinal);
        Assert.Equal(["Cancel", "Continue"], buttons.Select(button => button.Trim()).Where(button => button.Length > 0));
        Assert.Equal(("alertdialog", 0, 0, 0), (role, close, escaped, left));
    }

    [Fact]
    [Trait("AC", "SHEET-04")]
    public async Task ASheetIsAModalDialogAtItsSideAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/sheet").ToString());
        var prompt = await PromptAsync(page, "Open");
        var dialog = await prompt.BoundingBoxAsync();
        var surface = await page.EvaluateAsync<double[]>("() => { const el = [...document.querySelectorAll('dialog.prompt[open] div')].find(d => getComputedStyle(d).borderLeftWidth === '1px' && getComputedStyle(d).backgroundColor !== 'rgba(0, 0, 0, 0)'); const r = el.getBoundingClientRect(); return [r.x, r.y, r.width, r.height] }");
        var fields = await prompt.Locator("input[name^='f']:not([hidden])").CountAsync();
        await AnswerAsync(page, "Close");
        await page.WaitForSelectorAsync("text=Sheet (Right): close");
        var left = await page.Locator("dialog.prompt[open]").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(dialog!.X + dialog.Width, surface[0] + surface[2], 1);
        Assert.Equal(dialog.Height, surface[3], 1);
        Assert.Equal(384, surface[2], 1);
        Assert.Equal((2, 0), (fields, left));
    }

    [Fact]
    [Trait("AC", "DRAWER-04")]
    public async Task ADrawerIsAModalDialogAtTheBottomAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/drawer").ToString());
        var prompt = await PromptAsync(page, "Open Drawer");
        var dialog = await prompt.BoundingBoxAsync();
        var surface = await page.EvaluateAsync<double[]>("() => { const el = [...document.querySelectorAll('dialog.prompt[open] div')].find(d => getComputedStyle(d).borderTopWidth === '1px' && getComputedStyle(d).borderTopLeftRadius === '10px'); const r = el.getBoundingClientRect(); return [r.x, r.y, r.width, r.height] }");
        var buttons = await prompt.Locator("button").AllInnerTextsAsync();
        await page.Locator("dialog.prompt[open] button[aria-label='Increase']").ClickAsync();
        await page.WaitForSelectorAsync("dialog.prompt:modal >> text=360");
        await AnswerAsync(page, "Cancel");
        await page.WaitForSelectorAsync("text=Move goal: closed");
        var left = await page.Locator("dialog.prompt[open]").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(dialog!.Y + dialog.Height, surface[1] + surface[3], 1);
        Assert.Equal(dialog.Width, surface[2], 1);
        Assert.Contains("Submit", buttons.Select(button => button.Trim()));
        Assert.Equal(0, left);
    }

    [Fact]
    [Trait("AC", "SONNER-04")]
    public async Task APostsToastIsANoticeAtTheCornerAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/sonner").ToString());
        await page.Locator("main button", new() { HasTextRegex = new Regex("^Show Toast$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).ClickAsync();
        await page.WaitForSelectorAsync(".notice:popover-open");
        var notice = page.Locator(".notice:popover-open").First;
        var corner = await notice.GetAttributeAsync("style");
        var card = await notice.Locator("[role=status]").GetAttributeAsync("style");
        var status = await notice.Locator("[role=status]").InnerTextAsync();
        var action = await notice.Locator("button").AllInnerTextsAsync();
        await page.Context.CloseAsync();

        Assert.StartsWith("bottom:24px;right:24px;width:356px;", corner?.Replace(" ", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.StartsWith("--expire:4000ms;", card?.Replace(" ", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Contains("Event has been created", status, StringComparison.Ordinal);
        Assert.Contains("Sunday, December 03, 2023 at 9:00 AM", status, StringComparison.Ordinal);
        Assert.Equal(["Undo"], action);
    }

    [GeneratedRegex("<button type=\"button\" popovertarget=\"(m\\d+)\"[^>]*>Open popover</button>")]
    private static partial Regex TriggerTag();
}
