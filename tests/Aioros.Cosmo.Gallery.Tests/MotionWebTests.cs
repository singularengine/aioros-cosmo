using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>
/// An element's entrance as Chromium reads it: its starting look, its look while its popover is closed and the length of
/// its transitions there, its transform's origin, and its computed transitions.
/// </summary>
internal sealed record WebEntrance(string Name, string Start, string Closed, string Leaves, string Origin, string Property, string Duration, string Timing, string Behavior);

public sealed class MotionWebTests(GalleryHost gallery, Browser browser)
{
    private const string Ease = "cubic-bezier(0.25, 0.1, 0.25, 1)";

    private const string Pops = "opacity, transform, display, overlay";

    private const string Discrete = "normal, normal, allow-discrete, allow-discrete";

    private const string Out = "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0);";

    private const string Read = """
        (scope) => {
          const root = document.querySelector(scope), starts = [], closed = [], plain = [];
          const walk = (rules) => { for (const rule of rules) { if (rule instanceof CSSStartingStyleRule) starts.push(...rule.cssRules); else if (rule.selectorText?.includes(':not(:popover-open)')) closed.push(rule); else if (rule.selectorText) plain.push(rule); if (rule.cssRules && !(rule instanceof CSSStartingStyleRule)) walk(rule.cssRules) } };
          for (const sheet of document.styleSheets) walk(sheet.cssRules);
          const look = (style) => ['opacity', 'transform'].filter((name) => style.getPropertyValue(name)).map((name) => name + ': ' + style.getPropertyValue(name) + ';').join(' ');
          return starts.filter((rule) => /^\.en/.test(rule.selectorText) && !rule.selectorText.includes('[data-motion')).flatMap((rule) => {
            const name = rule.selectorText.split('.')[1];
            return [root, ...root.querySelectorAll('*')].filter((element) => element.classList.contains(name) && (!root.matches('[popover]') || element.closest('[popover]') === root)).map((element) => {
              const style = getComputedStyle(element), shut = closed.find((candidate) => candidate.selectorText.includes('.' + name + ':')), origin = plain.find((candidate) => candidate.selectorText === '.' + name);
              return { name, start: rule.style.cssText, closed: shut ? look(shut.style) : '', leaves: shut ? shut.style.transitionDuration : '', origin: origin ? origin.style.transformOrigin : '', property: style.transitionProperty, duration: style.transitionDuration, timing: style.transitionTimingFunction, behavior: style.transitionBehavior };
            });
          });
        }
        """;

    private static readonly string[] PickerRules =
    [
        ".$::picker(select) {opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0); transition-duration: 150ms, 150ms, 150ms, 150ms; transition-property: opacity, transform, display, overlay; transition-behavior: normal, normal, allow-discrete, allow-discrete}",
        ".$:open::picker(select) {opacity: 1; transform: none}",
        "starting .$:open::picker(select) {opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0)}",
        "(prefers-reduced-motion: reduce) .$::picker(select) {transition-duration: 0s; transition-property: none; transition-behavior: normal}",
    ];

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    private static async Task<List<WebEntrance>> EntrancesAsync(IPage page, string scope) =>
        JsonSerializer.Deserialize<List<WebEntrance>>(await page.EvaluateAsync<JsonElement>(Read, scope), Options) ?? [];

    /// <summary>
    /// What differs on a fixture's popover from shadcn/ui's tw-animate-css motion: <paramref name="start"/> as its
    /// <c>@starting-style</c>, <paramref name="closed"/> as its look while closed (<c>animate-out</c>, no slide) with its
    /// 150 ms there, its origin, its opacity and transform transitions over 150 ms with <c>ease</c>, and its display and
    /// top layer kept while it leaves (<c>allow-discrete</c>).
    /// </summary>
    private async Task<List<string>> PopsAsync(string fixture, string popover, string start, string closed, string origin, int milliseconds = 150)
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/" + fixture).ToString());
        var found = await EntrancesAsync(page, popover);
        await page.Context.CloseAsync();
        var seconds = $"{milliseconds / 1000.0}s";
        var expected = new WebEntrance(found.FirstOrDefault()?.Name ?? string.Empty, start, closed, string.Join(", ", Enumerable.Repeat($"{milliseconds}ms", 4)), origin, Pops, string.Join(", ", Enumerable.Repeat(seconds, 4)), $"{Ease}, {Ease}, ease, ease", Discrete);
        return found.Count == 1 && found[0] == expected ? [] : [$"{fixture} {popover}: {string.Join("; ", found)}, expected {expected}"];
    }

    /// <summary>What differs in the modal <c>&lt;dialog&gt;</c> a page's button opens (a post) from its surface's <paramref name="expected"/> entrance and its mask's 150 ms fade.</summary>
    private const string Leaves = """
        () => {
          const dialog = document.querySelector('dialog.prompt:modal'), found = [];
          const walk = (rules, within) => { for (const rule of rules) { const where = rule instanceof CSSMediaRule ? rule.conditionText + ' ' : within;
            if (rule.selectorText?.startsWith('#' + dialog.id + ':not([open]) ')) { const part = dialog.querySelector(rule.selectorText.split(' ').pop()); if (part) found.push((part.classList.contains('pm') ? 'mask ' : 'surface ') + ['opacity', 'transform', 'transition-duration'].filter((key) => rule.style.getPropertyValue(key)).map((key) => key + ': ' + rule.style.getPropertyValue(key)).join('; ')) }
            else if (rule.selectorText === '#' + dialog.id) found.push(where + 'dialog ' + rule.style.getPropertyValue('transition-duration') + ' ' + rule.style.getPropertyValue('transition-behavior'));
            if (rule.cssRules) walk(rule.cssRules, where) } };
          for (const sheet of document.styleSheets) walk(sheet.cssRules, '');
          return [...new Set(found)].sort();
        }
        """;

    /// <summary>
    /// What differs in the modal <c>&lt;dialog&gt;</c> a page's button opens (a post) from its surface's <paramref name="expected"/>
    /// entrance and its mask's 150 ms fade, and, while it is closed, from its parts' <paramref name="leaves"/> (Cosmo's exits,
    /// the dialog kept displayed for the longest), none of it for a visitor who asks for less motion. The page's stylesheet
    /// also holds its plan's rules for the parts other states show (a sheet's other sides), which are left out.
    /// </summary>
    private async Task<List<string>> DialogAsync(string route, string trigger, WebEntrance expected, params string[] leaves)
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, route).ToString());
        await page.Locator("main button", new() { HasTextRegex = new Regex($"^{Regex.Escape(trigger)}$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).First.ClickAsync();
        await page.WaitForSelectorAsync("dialog.prompt:modal");
        var found = (await EntrancesAsync(page, "dialog.prompt")).Select(entrance => entrance with { Name = string.Empty }).ToList();
        var left = await page.EvaluateAsync<string[]>(Leaves);
        await page.Context.CloseAsync();
        var mask = new WebEntrance(string.Empty, "opacity: 0;", string.Empty, string.Empty, string.Empty, "opacity", "0.15s", Ease, "normal");
        var wanted = leaves.Order(StringComparer.Ordinal).ToArray();
        return [.. found.Count == 2 && found.Contains(expected) && found.Contains(mask) ? [] : new[] { $"{route}: {string.Join("; ", found)}, expected {expected} and {mask}" },
            .. left.SequenceEqual(wanted) ? [] : new[] { $"{route} leaves: {string.Join(" | ", left)}, expected {string.Join(" | ", wanted)}" }];
    }

    private static void None(IEnumerable<string> wrong)
    {
        var all = wrong.ToList();
        Assert.True(all.Count == 0, string.Join(Environment.NewLine, all));
    }

    private static WebEntrance Zooms(int milliseconds) =>
        new(string.Empty, "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0);", string.Empty, string.Empty, string.Empty, "opacity, transform", $"{milliseconds / 1000.0}s, {milliseconds / 1000.0}s", $"{Ease}, {Ease}", "normal, normal");

    [Fact]
    [Trait("AC", "POPOVER-06")]
    public async Task APopoverFadesZoomsAndSlidesInAndOutOnTheWebAsync() =>
        None((await PopsAsync("popover-open", "[popover][role=dialog]", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, -8);", Out, "50% 0%"))
            .Concat(await PopsAsync("combobox-open", "[popover][role=dialog]", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, -8);", Out, "50% 0%")));

    [Fact]
    [Trait("AC", "HOVERCARD-06")]
    public async Task AHoverCardFadesZoomsAndSlidesInAndOutOnTheWebAsync() =>
        None(await PopsAsync("hover-card-open", "[popover=hint]", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, -8);", Out, "50% 0%"));

    [Fact]
    [Trait("AC", "DROPDOWN-07")]
    public async Task ADropdownMenuAndItsSubMenuFadeZoomAndSlideInAndOutOnTheWebAsync() =>
        None((await PopsAsync("dropdown-menu-open", "#m1", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, -8);", Out, "0% 0%"))
            .Concat(await PopsAsync("dropdown-menu-open", "#m2", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, -8, 0);", Out, "0% 0%")));

    [Fact]
    [Trait("AC", "CONTEXTMENU-06")]
    public async Task AContextMenuFadesZoomsAndSlidesInAndOutOnTheWebAsync() =>
        None(await PopsAsync("context-menu-open", "[popover].cm", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, -8, 0);", Out, "0% 0%"));

    [Fact]
    [Trait("AC", "MENUBAR-07")]
    public async Task AMenubarsMenusFadeZoomAndSlideInAndOutOnTheWebAsync() =>
        None(await PopsAsync("menubar-open", "#m1", "opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, -8);", Out, "0% 0%"));

    [Fact]
    [Trait("AC", "NAVMENU-07")]
    public async Task ANavigationMenusContentFadesAndZoomsOnTheWebAsync() =>
        None(await PopsAsync("navigation-menu-open", "[popover][role=dialog]", Out, Out, string.Empty, 200));

    [Fact]
    [Trait("AC", "NAVMENU-09")]
    public async Task ANavigationMenusContentSlidesBetweenItemsOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/navigation-menu-demo").ToString());
        var triggers = page.Locator("main [data-hover-open]");
        var switches = await page.EvaluateAsync<bool[]>("() => [...document.querySelectorAll('main [data-hover-open]')].map((button) => button.hasAttribute('data-switch'))");
        await triggers.Nth(0).HoverAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('main [data-hover-open] + [popover]').matches(':popover-open')");
        await triggers.Nth(1).HoverAsync();
        var motions = await page.EvaluateAsync<string[]>("""
            () => {
              const [home, components] = [...document.querySelectorAll('main [data-hover-open]')].slice(0, 2).map((button) => button.nextElementSibling);
              const name = [...components.classList].find((value) => value.startsWith('en')), rules = [];
              const walk = (list, within) => { for (const rule of list) { if (rule instanceof CSSStartingStyleRule) walk(rule.cssRules, 'starting '); else if (rule.selectorText?.includes('.' + name + '[data-motion')) rules.push(within + rule.selectorText.replaceAll('.' + name, '.$') + ' {' + rule.style.getPropertyValue('transform') + '}'); if (rule.cssRules && !(rule instanceof CSSStartingStyleRule)) walk(rule.cssRules, within) } };
              for (const sheet of document.styleSheets) walk(sheet.cssRules, '');
              return [home.dataset.motion + ' ' + home.matches(':popover-open'), components.dataset.motion + ' ' + components.matches(':popover-open'), ...rules.sort()];
            }
            """);
        await page.Context.CloseAsync();

        Assert.All(switches, Assert.True);
        Assert.Equal(
            [
                "to-start false", "from-end true",
                "[popover].$[data-motion=\"to-end\"]:not(:popover-open) {translateX(208px) matrix(0.95, 0, 0, 0.95, 0, 0)}",
                "[popover].$[data-motion=\"to-start\"]:not(:popover-open) {translateX(-208px) matrix(0.95, 0, 0, 0.95, 0, 0)}",
                "starting .$.$.$.$[data-motion=\"from-end\"] {translateX(208px) matrix(0.95, 0, 0, 0.95, 0, 0)}",
                "starting .$.$.$.$[data-motion=\"from-start\"] {translateX(-208px) matrix(0.95, 0, 0, 0.95, 0, 0)}",
            ],
            motions);
    }

    [Fact]
    [Trait("AC", "TOOLTIP-07")]
    public async Task ATooltipEntersAndLeavesAsShadcnsOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/tooltip").ToString());
        var rules = await page.EvaluateAsync<string[]>("""
            () => {
              const tip = document.querySelector('main .tt'), name = [...tip.classList].find((value) => value.startsWith('tm')), rules = [];
              const walk = (list, within) => { for (const rule of list) {
                if (rule instanceof CSSKeyframesRule) { if (rule.name === 'k' + name) rules.push('keyframes ' + rule.cssRules[0].style.cssText) }
                else if (rule.selectorText?.includes('.' + name)) rules.push((within + rule.selectorText + ' {' + ['opacity', 'transform', 'transform-origin', 'transition', 'animation'].filter((key) => rule.style.getPropertyValue(key)).map((key) => key + ': ' + rule.style.getPropertyValue(key)).join('; ') + '}').replaceAll(name, '$'));
                if (rule.cssRules && !(rule instanceof CSSKeyframesRule)) walk(rule.cssRules, rule.conditionText ? rule.conditionText + ' ' : within) } };
              for (const sheet of document.styleSheets) walk(sheet.cssRules, '');
              return rules.sort();
            }
            """);
        var trigger = page.Locator("main .tip").First;
        await trigger.HoverAsync();
        await page.WaitForFunctionAsync("() => getComputedStyle(document.querySelector('main .tip > .tt')).visibility === 'visible'");
        var animation = await page.EvaluateAsync<string>("() => getComputedStyle(document.querySelector('main .tip > .tt')).animationName");
        await page.Context.CloseAsync();

        Assert.Equal(
            [
                "(prefers-reduced-motion: reduce) .tt.$, .tip:hover > .tt.$, .tip:has(:focus-visible) > .tt.$ {transition: visibility; animation: auto ease 0s 1 normal none running none}",
                ".tip:hover > .tt.$, .tip:has(:focus-visible) > .tt.$ {opacity: 1; transform: none; transition: visibility 0s linear var(--tip-delay,.4s); animation: k$ 150ms cubic-bezier(0.25,0.1,0.25,1) var(--tip-delay,.4s) backwards}",
                ".tip[data-off] > .tt.$ {transition: none; animation: auto ease 0s 1 normal none running none}",
                ".tt.$ {opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0); transform-origin: 50% 100%; transition: opacity 150ms cubic-bezier(0.25, 0.1, 0.25, 1), transform 150ms cubic-bezier(0.25, 0.1, 0.25, 1), visibility linear 150ms}",
                "keyframes opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 8);",
            ],
            rules);
        Assert.StartsWith("ktm", animation, StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "DIALOG-07")]
    public async Task ADialogFadesAndZoomsInOverItsFadingMaskOnTheWebAsync() =>
        None(await DialogAsync("components/dialog", "Open Dialog", Zooms(200), "surface opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0); transition-duration: 200ms, 200ms", "mask opacity: 0; transition-duration: 150ms", "dialog 200ms, 200ms allow-discrete, allow-discrete", "(prefers-reduced-motion: reduce) dialog 0s normal"));

    [Fact]
    [Trait("AC", "ALERTDIALOG-06")]
    public async Task AnAlertDialogFadesAndZoomsInOnTheWebAsync() =>
        None(await DialogAsync("components/alert-dialog", "Show Dialog", Zooms(200), "surface opacity: 0; transform: matrix(0.95, 0, 0, 0.95, 0, 0); transition-duration: 200ms, 200ms", "mask opacity: 0; transition-duration: 150ms", "dialog 200ms, 200ms allow-discrete, allow-discrete", "(prefers-reduced-motion: reduce) dialog 0s normal"));

    [Fact]
    [Trait("AC", "SHEET-06")]
    public async Task ASheetSlidesInFromItsSideOnTheWebAsync() =>
        None(await DialogAsync("components/sheet", "Open", new(string.Empty, "transform: matrix(1, 0, 0, 1, 384, 0);", string.Empty, string.Empty, string.Empty, "transform", "0.5s", "cubic-bezier(0.4, 0, 0.2, 1)", "normal"), "surface transform: matrix(1, 0, 0, 1, 384, 0); transition-duration: 300ms", "mask opacity: 0; transition-duration: 150ms", "dialog 300ms, 300ms allow-discrete, allow-discrete", "(prefers-reduced-motion: reduce) dialog 0s normal"));

    [Fact]
    [Trait("AC", "DRAWER-06")]
    public async Task ADrawerSlidesUpOnTheWebAsync() =>
        None(await DialogAsync("components/drawer", "Open Drawer", new(string.Empty, "transform: matrix(1, 0, 0, 1, 0, 317);", string.Empty, string.Empty, string.Empty, "transform", "0.5s", "cubic-bezier(0.32, 0.72, 0, 1)", "normal"), "surface transform: matrix(1, 0, 0, 1, 0, 317); transition-duration: 500ms", "mask opacity: 0; transition-duration: 150ms", "dialog 500ms, 500ms allow-discrete, allow-discrete", "(prefers-reduced-motion: reduce) dialog 0s normal"));

    [Fact]
    [Trait("AC", "SELECT-06")]
    public async Task ASelectsListFadesAndZoomsInAndOutOnTheWebAsync()
    {
        const string Picker = """
            () => {
              const name = [...document.querySelector('select').classList].find((value) => value.startsWith('pk')), found = [];
              const walk = (rules, within) => { for (const rule of rules) { const where = rule instanceof CSSStartingStyleRule ? 'starting ' : rule instanceof CSSMediaRule ? rule.conditionText + ' ' : within; if (rule.selectorText?.startsWith('.' + name + ':open::picker') || rule.selectorText === '.' + name + '::picker(select)') found.push(where + rule.selectorText.replace(name, '$') + ' {' + ['opacity', 'transform', 'transition-duration', 'transition-property', 'transition-behavior'].filter((key) => rule.style.getPropertyValue(key)).map((key) => key + ': ' + rule.style.getPropertyValue(key)).join('; ') + '}'); if (rule.cssRules) walk(rule.cssRules, where) } };
              for (const sheet of document.styleSheets) walk(sheet.cssRules, '');
              return found;
            }
            """;
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/select-trigger").ToString());
        var rules = await page.EvaluateAsync<string[]>(Picker);
        await page.Context.CloseAsync();

        None(PickerRules.Where(rule => !rules.Contains(rule)).Select(rule => $"missing {rule} in {string.Join(" | ", rules)}"));
    }

    [Fact]
    [Trait("AC", "SONNER-06")]
    public async Task AToastRisesAndFallsAsSonnersOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/sonner").ToString());
        await page.Locator("main button", new() { HasTextRegex = new Regex("^Show Toast$", RegexOptions.None, TimeSpan.FromSeconds(1)) }).ClickAsync();
        await page.WaitForSelectorAsync(".notice:popover-open");
        var html = await page.EvaluateAsync<string>("() => document.documentElement.outerHTML");
        var height = await page.EvaluateAsync<double>("() => document.querySelector('.notice:popover-open .nt > *').getBoundingClientRect().height");
        await page.Context.CloseAsync();
        var leaving = Regex.Match(html, "<div class=\"nt (n[0-9a-f]+)[^\"]*\" role=\"status\"").Groups[1].Value;
        var fall = Regex.Match(html, $"@keyframes {leaving}\\{{to\\{{opacity:0;transform:matrix\\(1,0,0,1,0,(?<y>[0-9.]+)\\);visibility:hidden\\}}\\}}");
        var rise = Regex.Match(html, "@starting-style\\{\\.(?<name>en[0-9a-f]+)\\.\\k<name>\\.\\k<name>\\.\\k<name>\\{opacity:0;transform:matrix\\(1,0,0,1,0,(?<y>[0-9.]+)\\);\\}\\}");

        Assert.Contains($".notice>.nt.{leaving}{{animation:{leaving} 400ms cubic-bezier(0.25,0.1,0.25,1) forwards;animation-delay:var(--expire)}}", html, StringComparison.Ordinal);
        Assert.True(fall.Success && rise.Success, html[html.IndexOf("@keyframes n", StringComparison.Ordinal)..][..200]);
        Assert.InRange(double.Parse(fall.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture), height - 2, height + 2);
        Assert.InRange(double.Parse(rise.Groups["y"].Value, System.Globalization.CultureInfo.InvariantCulture), height - 2, height + 2);
    }

    [Fact]
    [Trait("AC", "THEME-07")]
    public async Task OverlaysStandStillForAVisitorWhoAsksForLessMotionAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.EmulateMediaAsync(new() { ReducedMotion = ReducedMotion.Reduce });
        await page.GotoAsync(new Uri(gallery.Address, "components/popover").ToString());
        var trigger = page.Locator("main button[popovertarget]").First;
        var id = await trigger.GetAttributeAsync("popovertarget");
        await trigger.ClickAsync();
        await page.WaitForSelectorAsync($"#{id}:popover-open");
        const string Moving = "id => document.getAnimations().filter((animation) => document.getElementById(id).contains(animation.effect?.target)).length";
        var moving = await page.EvaluateAsync<int>(Moving, id);
        var durations = await page.EvaluateAsync<string>("id => getComputedStyle(document.getElementById(id)).transitionDuration", id);
        await page.Keyboard.PressAsync("Escape");
        var gone = await page.EvaluateAsync<bool>($"id => !document.getElementById(id).matches(':popover-open') && ({Moving})(id) === 0", id);
        await page.Context.CloseAsync();

        Assert.Equal((0, "0s", true), (moving, durations, gone));
    }
}
