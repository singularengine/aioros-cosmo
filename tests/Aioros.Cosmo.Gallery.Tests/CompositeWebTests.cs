using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class CompositeWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    private static readonly string[] DemoSeparators = ["vertical:1:50:0", "horizontal:1:25:0"];

    private static readonly string[] HorizontalSeparator = ["horizontal:1:25:0"];

    private static readonly string[] VerticalSeparator = ["vertical:1:25:0"];

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

    private static IEnumerable<string> Boxes(string what, IReadOnlyList<ElementMetrics> found, List<ElementMetrics> expected, double tolerance) =>
        found.Count != expected.Count
            ? [$"{what}: {found.Count} elements, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{what} #{i}", pair.First, pair.Second, tolerance));

    [Fact]
    [Trait("AC", "RESIZABLE-01")]
    public async Task ResizableGroupsAreGridsWithSeparatorsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["resizable-demo", "resizable-handle", "resizable-vertical"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme, reference) =>
            {
                var handles = await Browser.PickAsync(page, "#fixture [role=separator]");
                var panels = await Browser.PickAsync(page, "#fixture [role=separator] ~ *:not([role=separator]), #fixture *:has(+ [role=separator])");
                var expectedHandles = reference.Where(element => element.Slot == "resizable-handle").ToList();
                var expectedPanels = reference.Where(element => element.Slot == "resizable-panel").ToList();
                var errors = Boxes($"{theme} {fixture} handles", handles, expectedHandles, 1).ToList();
                errors.AddRange(Boxes($"{theme} {fixture} panels", panels, expectedPanels, 1));
                errors.AddRange(handles.Zip(expectedHandles).Where(pair => !CssColor.Parse(pair.First.Background).Near(CssColor.Parse(pair.Second.Background))).Select(pair => $"{theme} {fixture} handle {pair.First.Background}"));
                var written = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture [role=separator]')].map((el) => `${el.getAttribute('aria-orientation')}:${el.querySelectorAll(':scope > :not(input)').length}:${el.getAttribute('aria-valuenow')}:${el.tabIndex}`)");
                var expected = fixture switch
                {
                    "resizable-demo" => DemoSeparators,
                    "resizable-vertical" => HorizontalSeparator,
                    _ => VerticalSeparator,
                };
                errors.AddRange(written.SequenceEqual(expected) ? [] : [$"{theme} {fixture}: separators {string.Join(',', written)}"]);
                errors.AddRange(await StepAsync(page, $"{theme} {fixture}", fixture == "resizable-vertical" ? "ArrowDown" : "ArrowRight", fixture == "resizable-demo" ? "55" : "30"));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    /// <summary>An arrow on the first handle, moving it by shadcn's 5% without a request.</summary>
    private static async Task<IEnumerable<string>> StepAsync(IPage page, string what, string key, string expected)
    {
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        var handle = page.Locator("#fixture [role=separator]").First;
        await handle.FocusAsync();
        await page.Keyboard.PressAsync(key);
        var moved = await handle.GetAttributeAsync("aria-valuenow");
        return moved == expected && requests == 0 ? [] : [$"{what}: {key} moved the first handle to {moved}, {requests} requests"];
    }

    [Fact]
    [Trait("AC", "CAROUSEL-05")]
    public async Task ABoundCarouselPostsEachMoveOnceAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/carousel").ToString());
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(Uri.UnescapeDataString(request.PostData ?? string.Empty));
            }
        };
        var carousel = page.Locator("[aria-roledescription=carousel]").Last;
        var next = carousel.Locator("button[data-aioros-slide='1']");
        async Task SettledAsync(string text)
        {
            await page.Locator($"text={text}").WaitForAsync();
            await carousel.EvaluateAsync("c => new Promise((done) => { const s = c.querySelector('[data-aioros-strip]'); let last = -1; const look = () => { const now = s.scrollLeft; if (now === last) { done(0); return } last = now; requestAnimationFrame(() => requestAnimationFrame(look)) }; look() })");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        await next.ClickAsync();
        await SettledAsync("Slide 2 of 5");
        var once = posts.Count;
        await next.ClickAsync();
        await next.ClickAsync();
        await SettledAsync("Slide 4 of 5");
        var last = posts[^1];
        var index = await carousel.EvaluateAsync<int>("c => [...c.querySelector('[data-aioros-strip]').children].findIndex(x => x.classList.contains('sel'))");
        await page.Context.CloseAsync();

        Assert.Equal(1, once);
        Assert.Matches("f[0-9]+=3", last);
        Assert.Equal(3, index);
    }

    [Fact]
    [Trait("AC", "CAROUSEL-01")]
    public async Task CarouselsAreScrollSnapStripsWithTheirButtonsAsync()
    {
        var wrong = new List<string>();
        foreach (var (fixture, axis) in new[] { ("carousel-demo", "x"), ("carousel-api", "x"), ("carousel-orientation", "y") })
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme, _) =>
            {
                var strip = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture *')].filter((el) => /^[xy]/.test(getComputedStyle(el).scrollSnapType)).map((el) => `${getComputedStyle(el).scrollSnapType[0]}:${el.children.length}:${el.scrollLeft + el.scrollTop}:${el.children[0].textContent.trim()}`)");
                var role = await page.Locator("#fixture [aria-roledescription=carousel]").CountAsync();
                var buttons = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture button')].map((b) => `${b.getAttribute('aria-label')}:${b.disabled}`)");
                var square = await page.EvaluateAsync<bool[]>("[...document.querySelectorAll('#fixture *')].filter((el) => getComputedStyle(el).aspectRatio !== 'auto').map((el) => Math.abs(el.getBoundingClientRect().width - el.getBoundingClientRect().height) < 1)");
                var sizes = await page.EvaluateAsync<int[]>("(() => { const s = [...document.querySelectorAll('#fixture *')].find((el) => /^y/.test(getComputedStyle(el).scrollSnapType)); return s ? [Math.round(s.getBoundingClientRect().height), Math.round(s.children[0].getBoundingClientRect().height)] : [] })()");
                var shape = fixture switch
                {
                    "carousel-demo" => square.Length == 5 && square.All(each => each),
                    "carousel-orientation" => sizes.SequenceEqual([196, 146]),
                    _ => true,
                };
                return strip.SequenceEqual([$"{axis}:5:0:1"]) && role == 1 && buttons.SequenceEqual(["Previous slide:true", "Next slide:false"]) && shape
                    ? []
                    : [$"{theme} {fixture}: strips {string.Join(',', strip)}, role {role}, buttons {string.Join(',', buttons)}, square {string.Join(',', square)}, sizes {string.Join(',', sizes)}"];
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "COMMAND-01")]
    public async Task TheCommandIsAnInputOverItsItemsAsync()
    {
        var wrong = await EachThemeAsync("command-demo", CommandAsync);
        var palette = await browser.PageAsync(gallery.Address, "light", 1280);
        await palette.GotoAsync(new Uri(gallery.Address, "components/command").ToString());
        var shortcut = await palette.Locator("main button:text-is('Open the palette')").GetAttributeAsync("aria-keyshortcuts");
        await Browser.KeysAsync(palette);
        var posted = palette.WaitForResponseAsync(response => response.Request.Method == "POST");
        await palette.Keyboard.PressAsync("Control+j");
        await posted;
        await palette.WaitForSelectorAsync("dialog[open]");
        var opened = await palette.Locator("dialog[open]").CountAsync();
        await palette.Context.CloseAsync();

        Assert.Empty(wrong);
        Assert.Equal(("Control+J", 1), (shortcut, opened));
    }


    private static async Task<IEnumerable<string>> CommandAsync(IPage page, string theme, IReadOnlyList<ElementMetrics> reference)
    {
        var inputs = await Browser.PickAsync(page, "#fixture input[placeholder='Type a command or search...']");
        var buttons = await Browser.PickAsync(page, "#fixture button");
        var frame = (await Browser.PickAsync(page, "#fixture div")).FirstOrDefault(element => element.Radius == "10px" && element.BorderWidth == "1px");
        var expected = reference.Single(element => element.Slot == "command");
        var headers = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture div:not(button *)')].filter((el) => el.children.length === 0 && ['Suggestions', 'Settings'].includes(el.textContent.trim())).map((el) => el.textContent.trim())");
        var input = reference.Single(element => element.Slot == "command-input");
        var errors = inputs.Count == 1 ? Compare.Box($"{theme} input", inputs[0], input with { Y = inputs[0].Y }, 1).Concat(Math.Abs(inputs[0].Y - input.Y) <= 3 ? [] : [$"{theme}: input at {inputs[0].Y}"]).ToList() : [$"{theme}: {inputs.Count} inputs"];
        errors.AddRange(frame is null ? [$"{theme}: no frame"] : Compare.Box($"{theme} frame", frame, expected with { Height = frame.Height }, 1));
        var items = reference.Where(element => element.Slot == "command-item").ToList();
        errors.AddRange(buttons.Select(button => button.Text).SequenceEqual(items.Select(item => item.Text), new Shortcut()) && headers.SequenceEqual(["Suggestions", "Settings"]) ? [] : [$"{theme}: {string.Join(',', buttons.Select(button => button.Text))} {string.Join(',', headers)}"]);
        errors.AddRange(buttons.Select(button => button.Opacity).SequenceEqual(["1", "1", "0.5", "1", "1", "1"]) ? [] : [$"{theme}: opacities {string.Join(',', buttons.Select(button => button.Opacity))}"]);
        errors.AddRange(buttons.Count == items.Count ? buttons.Zip(items).SelectMany((pair, i) => Compare.Box($"{theme} item {i}", pair.First, pair.Second, 1).Concat(Compare.Colors($"{theme} item {i}", pair.First with { Color = pair.Second.Color }, pair.Second))) : [$"{theme}: {buttons.Count} items"]);
        errors.AddRange(await FilteredAsync(page, theme));
        return errors;
    }

    /// <summary>The command filtered in the browser: its roles, `se` typed showing Search Emoji and Settings, the separator hidden, Down moving the selection, no request.</summary>
    private static async Task<IEnumerable<string>> FilteredAsync(IPage page, string theme)
    {
        var requests = 0;
        page.Request += (_, request) => requests += request.Method == "POST" ? 1 : 0;
        var roles = await page.EvaluateAsync<int[]>("[document.querySelectorAll('#fixture input[role=combobox]').length, document.querySelectorAll('#fixture [role=listbox]').length, document.querySelectorAll('#fixture [role=option]').length]");
        await page.Locator("#fixture input").FillAsync("se");
        const string Selected = "document.querySelector('#fixture [role=option][aria-selected=true]')?.textContent.trim() ?? ''";
        var shown = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture [role=option]')].filter((o) => !o.hidden).map((o) => o.textContent.trim())");
        var separators = await page.EvaluateAsync<bool>("[...document.querySelectorAll('#fixture [role=listbox] hr, #fixture [role=listbox] [role=separator]')].every((x) => x.hidden || getComputedStyle(x).display === 'none')");
        var first = await page.EvaluateAsync<string>(Selected);
        await page.Keyboard.PressAsync("ArrowDown");
        var next = await page.EvaluateAsync<string>(Selected);
        var errors = new List<string>();
        errors.AddRange(roles.SequenceEqual([1, 1, 6]) ? [] : [$"{theme}: roles {string.Join(',', roles)}"]);
        errors.AddRange(shown.Length == 2 && shown[0] == "Search Emoji" && shown[1].Contains("Settings", StringComparison.Ordinal) && separators ? [] : [$"{theme}: typing `se` shows {string.Join(',', shown)}, separators hidden {separators}"]);
        errors.AddRange(first == "Search Emoji" && next.Contains("Settings", StringComparison.Ordinal) && requests == 0 ? [] : [$"{theme}: selected {first} then {next}, {requests} requests"]);
        return errors;
    }

    /// <summary>An item's text with its shortcut, whichever order the shortcut is written in.</summary>
    private sealed class Shortcut : IEqualityComparer<string>
    {
        public bool Equals(string? x, string? y) => string.Concat((x ?? string.Empty).Order()) == string.Concat((y ?? string.Empty).Order());

        public int GetHashCode(string obj) => string.Concat(obj.Order()).GetHashCode(StringComparison.Ordinal);
    }

    [Fact]
    [Trait("AC", "COMBOBOX-01")]
    public async Task TheComboboxTriggerOpensAnAnchoredPopoverAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["combobox-demo", "combobox-open"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme, reference) =>
            {
                var trigger = (await Browser.PickAsync(page, "#fixture button:not([popover] button)")).Single();
                var expected = reference.Single(element => element.Slot == "popover-trigger");
                var errors = Compare.Box($"{theme} {fixture} trigger", trigger, expected, 1).ToList();
                errors.AddRange(trigger.Text == "Select framework..." && trigger.BorderWidth == "1px" && trigger.Radius == "8px" && trigger.FontWeight == "500" && CssColor.Parse(trigger.BorderColor).Near(CssColor.Parse(expected.BorderColor)) ? [] : [$"{theme} {fixture}: trigger {trigger.Text} {trigger.BorderWidth} {trigger.Radius} {trigger.FontWeight} {trigger.BorderColor}"]);
                var requests = 0;
                page.Request += (_, request) => requests += Browser.Sends(request);
                await page.Locator("#fixture button:not([popover] button)").ClickAsync();
                await page.WaitForSelectorAsync("[popover]:popover-open");
                var popover = page.Locator("[popover]:popover-open");
                var inputs = await popover.Locator("input").CountAsync();
                var text = await popover.InnerTextAsync();
                errors.AddRange(requests == 0 && inputs == 1 && text.Contains("Next.js", StringComparison.Ordinal) && text.Contains("Astro", StringComparison.Ordinal) ? [] : [$"{theme} {fixture}: popover {requests} requests, {inputs} inputs, {text}"]);
                errors.AddRange(await ChoicesFilteredAsync(popover, $"{theme} {fixture}", () => requests));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "COMBOBOX-06")]
    public async Task TheComboboxPopoverIsAsWideAsItsTriggerOnTheWebAsync() =>
        Assert.Empty(await EachThemeAsync("combobox-open", async (page, theme, reference) =>
        {
            await page.Locator("#fixture button:not([popover] button)").ClickAsync();
            await page.WaitForSelectorAsync("[popover]:popover-open");
            var popover = (await Browser.PickAsync(page, "[popover]:popover-open")).Single();
            var options = await Browser.PickAsync(page, "[popover]:popover-open [role=option]");
            var labels = await page.EvaluateAsync<double[]>("[...document.querySelectorAll('[popover]:popover-open [role=option]')].map((o) => { const walker = document.createTreeWalker(o, NodeFilter.SHOW_TEXT); let text; while ((text = walker.nextNode()) && !text.textContent.trim()); const range = document.createRange(); range.selectNodeContents(text); return range.getBoundingClientRect().left - o.getBoundingClientRect().left })");
            var items = reference.Where(element => element.Slot == "command-item").ToList();
            var errors = Compare.Box($"{theme} popover", popover, reference.Single(element => element.Slot == "popover-content"), 1).ToList();
            errors.AddRange(options.Count == items.Count ? options.Zip(items).SelectMany((pair, i) => Compare.Box($"{theme} option #{i}", pair.First, pair.Second, 1)) : [$"{theme}: {options.Count} options, shadcn {items.Count}"]);
            errors.AddRange(labels.Where(left => Math.Abs(left - 8) > 1).Select(left => $"{theme}: a label {left:0.#} px into its option, shadcn 8"));
            return errors;
        }));

    /// <summary>The combobox's options filtered in the browser: its roles, and `as` typed showing Astro alone, selected.</summary>
    private static async Task<IEnumerable<string>> ChoicesFilteredAsync(ILocator popover, string what, Func<int> requests)
    {
        var roles = await popover.EvaluateAsync<int[]>("(p) => [p.querySelectorAll('input[role=combobox]').length, p.querySelectorAll('[role=listbox]').length, p.querySelectorAll('[role=option]').length]");
        await popover.Locator("input").FillAsync("as");
        var shown = await popover.EvaluateAsync<string[]>("(p) => [...p.querySelectorAll('[role=option]')].filter((o) => !o.hidden).map((o) => o.textContent.trim())");
        var selected = await popover.EvaluateAsync<string>("(p) => p.querySelector('[role=option][aria-selected=true]')?.textContent.trim() ?? ''");
        return roles.SequenceEqual([1, 1, 5]) && shown.SequenceEqual(["Astro"]) && selected == "Astro" && requests() == 0 ? [] : [$"{what}: roles {string.Join(',', roles)}, `as` shows {string.Join(',', shown)} ({selected}), {requests()} requests"];
    }

    [Fact]
    [Trait("AC", "DATATABLE-01")]
    public async Task TheDataTableIsATableWithItsControlsAsync() =>
        Assert.Empty(await EachThemeAsync("data-table-demo", async (page, theme, reference) =>
        {
            var filter = (await Browser.PickAsync(page, "#fixture input[placeholder='Filter emails...']")).Single();
            var columns = (await Browser.PickAsync(page, "#fixture button")).First(button => button.Text == "Columns");
            var errors = Compare.Box($"{theme} filter", filter, reference.Single(element => element.Slot == "input"), 1).Concat(Compare.Box($"{theme} columns", columns, reference.Single(element => element.Slot == "dropdown-menu-trigger" && element.Text == "Columns"), 1)).ToList();
            var widths = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture col')].map((col) => col.style.width)");
            var cells = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture tbody tr')].map((row) => [...row.children].map((cell, i) => i === 0 ? String(!!cell.querySelector('input[type=checkbox]')) : i === 4 ? cell.querySelector('button[popovertarget]')?.getAttribute('aria-label') : cell.textContent.trim()).join('|'))");
            var headers = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture thead th')].map((cell) => cell.textContent.trim())");
            var amount = await page.EvaluateAsync<string[]>("(() => { const el = document.querySelector('#fixture tbody tr td:nth-child(4) div'); const s = getComputedStyle(el); return [s.fontWeight, String(el.getBoundingClientRect().right > el.parentElement.getBoundingClientRect().right - 12)] })()");
            var pager = (await Browser.PickAsync(page, "#fixture button")).Where(button => button.Text is "Previous" or "Next").ToList();
            var disabled = await page.EvaluateAsync<bool>("[...document.querySelectorAll('#fixture button')].filter((b) => ['Previous', 'Next'].includes(b.textContent.trim())).every((b) => b.disabled)");
            var summary = await page.Locator("#fixture").InnerTextAsync();
            errors.AddRange(widths.SequenceEqual(["32.84px", "119.66px", "271.55px", "100.23px", "65.72px"]) ? [] : [$"{theme}: widths {string.Join(',', widths)}"]);
            errors.AddRange(cells.SequenceEqual(["true|Success|ken99@example.com|$316.00|Open menu", "true|Success|abe45@example.com|$242.00|Open menu", "true|Processing|monserrat44@example.com|$837.00|Open menu", "true|Success|silas22@example.com|$874.00|Open menu", "true|Failed|carmella@example.com|$721.00|Open menu"]) ? [] : [$"{theme}: cells {string.Join(';', cells)}"]);
            errors.AddRange(amount.SequenceEqual(["500", "true"]) && pager.Count == 2 && disabled && summary.Contains("0 of 5 row(s) selected.", StringComparison.Ordinal) ? [] : [$"{theme}: amount {string.Join(',', amount)}, pager {pager.Count} {disabled}"]);
            var written = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture thead th')].map((cell) => `${!!cell.querySelector('input[type=checkbox]')}:${!!cell.querySelector('button')}:${cell.textContent.trim()}`)");
            var right = await page.EvaluateAsync<bool>("(() => { const th = document.querySelectorAll('#fixture thead th')[3]; const text = [...th.querySelectorAll('*')].find((el) => el.childElementCount === 0 && el.textContent.trim() === 'Amount'); return text.getBoundingClientRect().right > th.getBoundingClientRect().right - 12 })()");
            errors.AddRange(headers.SequenceEqual(["", "Status", "Email", "Amount", ""]) && written.SequenceEqual(["true:false:", "false:false:Status", "false:true:Email", "false:false:Amount", "false:false:"]) && right ? [] : [$"{theme}: headers {string.Join(',', written)}, Amount right-aligned {right}"]);
            return errors;
        }));

    [Fact]
    [Trait("AC", "COMBOBOX-05")]
    public async Task ChoicesAreMadeInTheBrowserAsync()
    {
        const string Triggers = "main button[popovertarget][aria-label='Select framework...']";
        const string Shown = "[...document.querySelectorAll(\"" + Triggers + "\")].map((button) => button.innerText.trim() + ':' + document.getElementById(button.getAttribute('popovertarget')).matches(':popover-open') + ':' + [...document.getElementById(button.getAttribute('popovertarget')).querySelectorAll('[aria-checked=true]')].map((option) => option.innerText.trim()).join('+'))";
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/combobox").ToString());
        await page.WaitForFunctionAsync("() => [...document.scripts].length > 0 && performance.getEntriesByType('resource').some((entry) => entry.name.includes('/_aioros/choice/'))", null, new() { Timeout = 10000 });
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(request.PostData ?? string.Empty);
            }
        };
        var triggers = page.Locator(Triggers);
        await triggers.Nth(0).ClickAsync();
        await page.Locator("[popover]:popover-open [role=option]:has-text('SvelteKit')").ClickAsync();
        var chosen = await page.EvaluateAsync<string[]>(Shown);
        await triggers.Nth(0).ClickAsync();
        await page.Locator("[popover]:popover-open [role=option]:has-text('SvelteKit')").ClickAsync();
        var cleared = await page.EvaluateAsync<string[]>(Shown);
        var quiet = posts.Count;
        await triggers.Nth(0).ClickAsync();
        await page.Locator("[popover]:popover-open [role=option]:has-text('SvelteKit')").ClickAsync();
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await triggers.Nth(1).ClickAsync();
        await page.Locator("[popover]:popover-open [role=option]:has-text('Nuxt.js')").ClickAsync();
        await answered;
        await page.WaitForSelectorAsync("main :text-is('Selected: Nuxt.js')", new() { Timeout = 10000 });
        var after = await page.EvaluateAsync<string[]>(Shown);
        await page.Context.CloseAsync();

        Assert.Equal(["SvelteKit:false:SvelteKit", "Remix:false:Remix"], chosen);
        Assert.Equal(["Select framework...:false:", "Remix:false:Remix"], cleared);
        Assert.Equal(0, quiet);
        Assert.Single(posts);
        Assert.Equal(["SvelteKit:false:SvelteKit", "Nuxt.js:false:Nuxt.js"], after);
    }

    [Fact]
    [Trait("AC", "DATATABLE-02")]
    public async Task PostedCommandsCarryTheFilterAndTheSelectionAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/data-table").ToString());
        await page.Locator("main tbody input[type=checkbox]").First.CheckAsync();
        await page.WaitForSelectorAsync("text=1 of 12 row(s) selected.");
        await page.Locator("main button:has-text('Next')").Last.ClickAsync();
        await page.WaitForFunctionAsync("() => document.querySelector('main tbody tr td:nth-child(3)')?.textContent.trim().startsWith('olivia')", null, new() { PollingInterval = 20 });
        var second = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('main tbody tr td:nth-child(3)')].map((cell) => cell.textContent.trim().split('@')[0])");
        await page.Locator("main input[placeholder='Filter emails...']").First.FillAsync("ia");
        await page.WaitForSelectorAsync("text=0 of 3 row(s) selected.");
        var filtered = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('main tbody tr td:nth-child(3)')].map((cell) => cell.textContent.trim().split('@')[0])");
        await page.Context.CloseAsync();

        Assert.Equal(("olivia,liam,noah,emma,ava", "olivia,liam,mia"), (string.Join(",", second), string.Join(",", filtered)));
    }

    [Fact]
    [Trait("AC", "SIDEBAR-01")]
    public async Task TheSidebarIsAColumnBesideItsInsetThatFoldsInTheBrowserAsync() =>
        Assert.Empty((string[])[.. await EachThemeAsync("sidebar-demo", async (page, theme, reference) =>
        {
            var trigger = (await Browser.PickAsync(page, "#fixture button[data-aioros-flips]")).Where(button => button.Width == 28).ToList();
            var icon = await page.Locator("#fixture button[data-aioros-flips][aria-label='Toggle Sidebar'] svg").CountAsync();
            var rule = (await Browser.PickAsync(page, "#fixture hr")).Single();
            var text = await page.Locator("#fixture").InnerTextAsync();
            const string Column = "(() => { const root = document.getElementById('fixture').getBoundingClientRect(); const nav = document.querySelector('#fixture nav'); const r = nav.getBoundingClientRect(); return [Math.round(r.width), Math.round(document.querySelector('#fixture main').getBoundingClientRect().x - root.x)] })()";
            var open = await page.EvaluateAsync<int[]>(Column);
            var background = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture nav div')].filter((el) => getComputedStyle(el).backgroundColor !== 'rgba(0, 0, 0, 0)').map((el) => getComputedStyle(el).backgroundColor)");
            var hit = await page.EvaluateAsync<bool>("(() => { const root = document.getElementById('fixture').getBoundingClientRect(); const hit = document.elementFromPoint(root.x + 152, root.y + 144); return !!hit.closest('button') && hit.closest('button').textContent.includes('Playground') })()");
            await Browser.KeysAsync(page, flags: true);
            var requests = 0;
            page.Request += (_, request) => requests += Browser.Sends(request);
            await page.HoverAsync("#fixture nav button:has-text('Playground')");
            var tipOpen = await page.EvaluateAsync<string>(Tip);
            await page.Mouse.MoveAsync(0, 0);
            await page.Keyboard.PressAsync("Control+b");
            await page.WaitForFunctionAsync("() => Math.round(document.querySelector('#fixture nav').getBoundingClientRect().width) === 48", null, new() { PollingInterval = 20 });
            var folded = (await page.EvaluateAsync<int[]>(Column))[0];
            await page.HoverAsync("#fixture nav button:has-text('Playground')");
            await page.WaitForFunctionAsync(Tip + " === 'visible'", null, new() { PollingInterval = 20 });
            var expanded = await page.Locator("#fixture button[data-aioros-flips]").Last.GetAttributeAsync("aria-expanded");
            var button = await page.EvaluateAsync<int>("Math.round([...document.querySelectorAll('#fixture nav button')].find((b) => b.textContent.includes('Playground')).getBoundingClientRect().width)");
            await page.Keyboard.PressAsync("Control+b");
            await page.WaitForFunctionAsync("() => Math.round(document.querySelector('#fixture nav').getBoundingClientRect().width) === 256", null, new() { PollingInterval = 20 });
            var errors = trigger.Count == 1 && icon > 0 && rule.Width == 1 && rule.Height == 16 && text.Contains("Platform", StringComparison.Ordinal) && text.Contains("Playground", StringComparison.Ordinal) ? new List<string>() : [$"{theme}: trigger {trigger.Count} icon {icon}, rule {rule.Width}x{rule.Height}, {text}"];
            errors.AddRange(background.Length > 0 && CssColor.Parse(background[0]).Near(CssColor.Parse(reference.Single(element => element.Slot == "sidebar-inner").Background)) ? [] : [$"{theme}: column {string.Join(',', background)}"]);
            errors.AddRange(open.SequenceEqual([256, 280]) && hit ? [] : [$"{theme}: column {string.Join(',', open)}, Playground hit {hit}"]);
            errors.AddRange(folded == 48 && expanded == "false" && button == 32 && requests == 0 ? [] : [$"{theme}: folded {folded}, aria-expanded {expanded}, {requests} requests, Playground {button} px"]);
            errors.AddRange(tipOpen == "hidden" ? [] : [$"{theme}: the Playground tip is {tipOpen} while the column is open"]);
            return errors;
        }), .. await BoundAsync()]);

    [Fact]
    [Trait("AC", "SIDEBAR-07")]
    public async Task TheSidebarsStateIsKeptInItsCookieOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1200, 900);
        var workspace = new Uri(gallery.Address, "examples/workspace").ToString();
        await page.GotoAsync(workspace);
        var states = new List<string>();
        foreach (var expected in (string[])["false", "true"])
        {
            await Browser.KeysAsync(page, flags: true);
            var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
            await page.Keyboard.PressAsync("Control+b");
            await answered;
            var cookie = (await page.Context.CookiesAsync()).SingleOrDefault(cookie => cookie.Name == "sidebar_state");
            await page.GotoAsync(workspace);
            var served = await page.EvaluateAsync<bool>("document.querySelector('[data-aioros-flag]').hasAttribute('data-aioros-on')");
            states.Add($"{cookie?.Value}/{cookie?.Path}/{served}");
        }

        await page.Context.CloseAsync();

        Assert.Equal(["false///False", "true///True"], states);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-09")]
    public async Task CollapsibleMenuItemsToggleInTheBrowserAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "sidebar-demo", "light");
        await page.AddStyleTagAsync(new() { Content = "*,*::before,*::after{transition:none!important;animation:none!important}" });
        await Browser.KeysAsync(page, flags: true);
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        const string State = """
            (() => {
              const button = (text) => [...document.querySelectorAll('#fixture nav button')].find((b) => b.textContent.trim() === text);
              const playground = button('Playground'), chevron = playground.querySelector('svg');
              const turn = chevron.getClientRects().length === 0 ? 'none' : getComputedStyle(chevron).transform !== 'none' ? 'turned' : 'flat';
              return [['History', 'Starred', 'Settings'].map((text) => button(text)?.getClientRects().length > 0 ? 'shown' : 'hidden').join('/'), playground.getAttribute('aria-expanded'), turn, button('Models').getAttribute('aria-expanded')].join()
            })()
            """;
        var playground = page.Locator("#fixture nav button[data-aioros-flips='IsExpanded']", new() { HasText = "Playground" });
        var states = new List<string> { await page.EvaluateAsync<string>(State) };
        for (var click = 0; click < 6; click++)
        {
            await playground.ClickAsync();
            states.Add(await page.EvaluateAsync<string>(State));
        }

        await page.Locator("#fixture nav button[data-aioros-flips='IsExpanded']", new() { HasText = "Models" }).ClickAsync();
        states.Add(await page.EvaluateAsync<string>(State));
        await page.Keyboard.PressAsync("Control+b");
        states.Add(await page.EvaluateAsync<string>(State));
        await playground.ClickAsync();
        states.Add(await page.EvaluateAsync<string>(State));
        await page.Keyboard.PressAsync("Control+b");
        states.Add(await page.EvaluateAsync<string>(State));
        await playground.ClickAsync();
        states.Add(await page.EvaluateAsync<string>(State));
        await page.Context.CloseAsync();

        const string Open = "shown/shown/shown,true,turned,false";
        const string Closed = "hidden/hidden/hidden,false,flat,false";
        Assert.Equal([Open, Closed, Open, Closed, Open, Closed, Open, "shown/shown/shown,true,turned,true", "hidden/hidden/hidden,true,none,true", "hidden/hidden/hidden,false,none,true", "hidden/hidden/hidden,false,flat,true", "shown/shown/shown,true,turned,true"], states);
        Assert.Equal(0, requests);
    }

    [Fact]
    [Trait("AC", "SIDEBAR-08")]
    public async Task TheSidebarIsASheetOnNarrowViewportsAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 390, 844);
        await page.GotoAsync(new Uri(gallery.Address, "examples/workspace").ToString());
        await page.AddStyleTagAsync(new() { Content = "*,*::before,*::after{transition:none!important;animation:none!important}" });
        await Browser.KeysAsync(page, flags: true);
        var requests = 0;
        page.Request += (_, request) => requests += Browser.Sends(request);
        const string Sheet = """
            (() => {
              const sheet = document.querySelector('[data-aioros-flag] dialog[data-aioros-sheet]'), provider = document.querySelector('[data-aioros-flag]').getBoundingClientRect(), main = document.querySelector('[data-aioros-flag] main').getBoundingClientRect();
              const column = [...sheet.querySelectorAll('div')].find((div) => Math.round(div.getBoundingClientRect().width) === 288 && Math.round(div.getBoundingClientRect().x) === 0);
              const mask = sheet.open ? document.elementFromPoint(350, 400) : null;
              return [sheet.open && sheet.matches(':modal'), Math.round(main.width - provider.width), column ? Math.round(column.getBoundingClientRect().height) : 0, mask ? getComputedStyle(mask).backgroundColor : 'none'].join()
            })()
            """;
        var trigger = page.Locator("[data-aioros-flag] main button[data-aioros-flips]").First;

        var closed = await page.EvaluateAsync<string>(Sheet);
        await trigger.ClickAsync();
        var opened = await page.EvaluateAsync<string>(Sheet);
        var sent = requests;
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForFunctionAsync("() => !document.querySelector('dialog[data-aioros-sheet]').open", null, new() { PollingInterval = 20 });
        await trigger.ClickAsync();
        await page.Mouse.ClickAsync(350, 400);
        await page.WaitForFunctionAsync("() => !document.querySelector('dialog[data-aioros-sheet]').open", null, new() { PollingInterval = 20 });
        await page.SetViewportSizeAsync(1200, 900);
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await trigger.ClickAsync();
        await answered;
        var wide = await page.EvaluateAsync<string>("[document.querySelector('dialog[data-aioros-sheet]').open, document.querySelector('[data-aioros-flag]').hasAttribute('data-aioros-on')].join()");
        await page.Context.CloseAsync();

        Assert.Equal("false,0,0,none", closed);
        Assert.Equal("true,0,844,rgba(0, 0, 0, 0.5)", opened);
        Assert.Equal(0, sent);
        Assert.Equal("false,false", wide);
    }

    private const string Tip = "(() => { const tip = [...document.querySelectorAll('#fixture [role=tooltip]')].find((t) => t.textContent === 'Playground'); return !tip ? 'missing' : getComputedStyle(tip).display === 'none' ? 'hidden' : getComputedStyle(tip).visibility })()";

    private async Task<List<string>> BoundAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light");
        await page.GotoAsync(new Uri(gallery.Address, "examples/workspace").ToString());
        var posts = new List<string>();
        page.Request += (_, request) =>
        {
            if (request.Method == "POST")
            {
                posts.Add(request.PostData ?? string.Empty);
            }
        };
        await Browser.KeysAsync(page, flags: true);
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.Keyboard.PressAsync("Control+b");
        await answered;
        var open = await page.EvaluateAsync<bool>("document.querySelector('[data-aioros-flag]').hasAttribute('data-aioros-on')");
        await page.Context.CloseAsync();
        return posts is [var post] && post.Contains("_chg=", StringComparison.Ordinal) && !open ? [] : [$"workspace: posts {string.Join(" / ", posts)}, open after the answer {open}"];
    }
}
