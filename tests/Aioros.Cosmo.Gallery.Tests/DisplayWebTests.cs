using System.Text.Json;
using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class DisplayWebTests(GalleryHost gallery, Browser browser)
{
    private static readonly string[] Themes = ["light", "dark"];

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Reads elements of the fixture: <c>text:</c> finds the innermost element whose text is the given one, optionally
    /// walking up to the first ancestor matching a CSS selector-free test (<c>up</c>, a JavaScript predicate on the computed
    /// style); <c>where:</c> filters every fixture element with a predicate.
    /// </summary>
    private const string Find = """
        ({ texts, up, where }) => {
          const fixture = document.getElementById("fixture"), root = fixture.getBoundingClientRect()
          const pick = (el) => {
            const s = getComputedStyle(el), r = el.getBoundingClientRect()
            return { tag: el.tagName.toLowerCase(), text: el.textContent.trim(), x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height,
              background: s.backgroundColor, color: s.color, borderColor: s.borderTopColor, borderWidth: s.borderTopWidth, radius: s.borderTopLeftRadius,
              shadow: s.boxShadow, fontSize: s.fontSize, fontWeight: s.fontWeight, opacity: s.opacity, decoration: s.fontStyle,
              paddingLeft: s.paddingLeft, paddingTop: s.paddingTop, slot: "", lineHeight: s.lineHeight }
          }
          const test = (source) => source ? new Function("el", "s", "return " + source) : null
          const climb = test(up), filter = test(where)
          const all = [...fixture.querySelectorAll("*")]
          if (filter) return all.filter((el) => filter(el, getComputedStyle(el))).map(pick)
          return texts.map((text) => {
            let el = all.filter((e) => e.textContent.trim() === text).pop()
            if (!el) return null
            while (climb && el !== fixture && !climb(el, getComputedStyle(el))) el = el.parentElement
            return pick(el)
          })
        }
        """;

    private static async Task<List<ElementMetrics?>> TextsAsync(IPage page, IEnumerable<string> texts, string? up = null) =>
        JsonSerializer.Deserialize<List<ElementMetrics?>>(await page.EvaluateAsync<JsonElement>(Find, new { texts = texts.ToArray(), up, where = (string?)null }), Options) ?? [];

    private static async Task<string[]> SpacingsAsync(IPage page, IEnumerable<string> texts) =>
        await page.EvaluateAsync<string[]>("(texts) => texts.map((text) => [...document.querySelectorAll('#fixture *')].filter((e) => e.textContent.trim() === text).pop()).map((el) => el ? getComputedStyle(el).letterSpacing : 'missing')", texts.ToArray());

    private static async Task<List<ElementMetrics>> WhereAsync(IPage page, string where) =>
        JsonSerializer.Deserialize<List<ElementMetrics>>(await page.EvaluateAsync<JsonElement>(Find, new { texts = Array.Empty<string>(), up = (string?)null, where }), Options) ?? [];

    private static List<string> Text(string what, ElementMetrics? found, ElementMetrics expected, double tolerance = 1, bool box = true)
    {
        if (found is null)
        {
            return [$"{what}: no element reads {expected.Text}"];
        }

        var wrong = new List<string>();
        if (box)
        {
            wrong.AddRange(Compare.Box(what, found, expected, tolerance));
        }

        wrong.AddRange(CssColor.Parse(found.Color).Near(CssColor.Parse(expected.Color)) ? [] : [$"{what}: color {found.Color}, shadcn {expected.Color}"]);
        wrong.AddRange(found.FontSize == expected.FontSize && found.LineHeight == expected.LineHeight ? [] : [$"{what}: font {found.FontSize}/{found.LineHeight}, shadcn {expected.FontSize}/{expected.LineHeight}"]);
        return wrong;
    }

    private static IEnumerable<string> Placed(string what, ElementMetrics? found, ElementMetrics expected, double tolerance) =>
        found is null ? [$"{what}: missing"] : Compare.Box(what, found, expected, tolerance).Where(error => !error.Contains(": width ", StringComparison.Ordinal) && !error.Contains(": x ", StringComparison.Ordinal));

    private static IEnumerable<string> Boxes(string what, IReadOnlyList<ElementMetrics> found, IReadOnlyList<ElementMetrics> expected, double tolerance, bool colors = true) =>
        found.Count != expected.Count
            ? [$"{what}: {found.Count} elements, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{what} #{i}", pair.First, pair.Second, tolerance)
                .Concat(colors && !CssColor.Parse(pair.First.Background).Near(CssColor.Parse(pair.Second.Background)) ? [$"{what} #{i}: background {pair.First.Background}, shadcn {pair.Second.Background}"] : []));

    private async Task<List<string>> EachThemeAsync(string fixture, Func<IPage, string, Task<IEnumerable<string>>> check)
    {
        var wrong = new List<string>();
        foreach (var theme in Themes)
        {
            var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
            wrong.AddRange(await check(page, theme));
            await page.Context.CloseAsync();
        }

        return wrong;
    }

    [Fact]
    [Trait("AC", "SEPARATOR-01")]
    public async Task ASeparatorIsAnHrComputedAsShadcnsAsync() =>
        Assert.Empty(await EachThemeAsync("separator-demo", async (page, theme) =>
        {
            var found = await Browser.PickAsync(page, "#fixture hr");
            var borders = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture hr')].map((hr) => getComputedStyle(hr).borderTopStyle)");
            var expected = await ReferenceMetrics.SlotsAsync("separator-demo", theme, "separator");
            return Boxes(theme, found, expected, 1).Concat(borders.All(border => border == "none") ? [] : [$"{theme}: borders {string.Join(',', borders)}"]);
        }));

    /// <summary>The CSS animations a fixture's elements run: duration, timing function, iteration count and the last key frame's text.</summary>
    private async Task<List<string>> AnimationsAsync(string fixture)
    {
        var page = await browser.PageAsync(gallery.Address, "light");
        await page.GotoAsync(new Uri(gallery.Address, "fidelity/" + fixture).ToString());
        var found = await page.EvaluateAsync<string[]>("""
            [...document.querySelectorAll('#fixture *')].map((el) => getComputedStyle(el)).filter((s) => s.animationName !== 'none').map((s) => {
              const rule = [...document.styleSheets].flatMap((sheet) => [...sheet.cssRules]).find((r) => r.type === CSSRule.KEYFRAMES_RULE && r.name === s.animationName)
              return [s.animationDuration, s.animationTimingFunction, s.animationIterationCount, rule ? rule.cssRules[rule.cssRules.length - 1].cssText : ''].join(' | ')
            })
            """);
        await page.Context.CloseAsync();
        return [.. found];
    }

    [Fact]
    [Trait("AC", "SKELETON-01")]
    public async Task SkeletonsAreAccentBoxesAtShadcnsPlacesAsync()
    {
        var pulses = await AnimationsAsync("skeleton-demo");
        Assert.Equal(6, pulses.Count);
        Assert.All(pulses, pulse => Assert.Equal("2s | cubic-bezier(0.4, 0, 0.6, 1) | infinite | 100% { opacity: 1; }", pulse));
        Assert.Empty(await EachThemeAsync("skeleton-demo", async (page, theme) =>
        {
            var found = await WhereAsync(page, "s.backgroundColor !== 'rgba(0, 0, 0, 0)' && el.children.length === 0");
            var expected = await ReferenceMetrics.SlotsAsync("skeleton-demo", theme, "skeleton");
            return Boxes(theme, found, expected, 1).Concat(found.Zip(expected).Where(pair => Math.Min(Compare.Px(pair.First.Radius), 24) != Math.Min(Compare.Px(pair.Second.Radius), 24)).Select(pair => $"{theme}: radius {pair.First.Radius}, shadcn {pair.Second.Radius}"));
        }));
    }

    [Fact]
    [Trait("AC", "SPINNER-01")]
    public async Task SpinnersAreNamedLoaderCirclesAsync()
    {
        var turns = await AnimationsAsync("spinner-demo");
        Assert.Equal(9, turns.Count);
        Assert.All(turns, turn => Assert.Matches(@"^1s \| linear \| infinite \| 100% \{ transform: .*rotate\(360deg\)", turn));
        Assert.Empty(await EachThemeAsync("spinner-demo", async (page, theme) =>
        {
            var spinners = (await Browser.PickAsync(page, "#fixture [aria-label=Loading]")).Select(spinner => spinner with { Width = Math.Round(spinner.Width), Height = Math.Round(spinner.Height) }).ToList();
            var offsets = await page.EvaluateAsync<double[][]>("[...document.querySelectorAll('#fixture [aria-label=Loading]')].map((el) => [el.offsetWidth, el.offsetHeight])");
            spinners = [.. spinners.Zip(offsets).Select(pair => pair.First with { Width = pair.Second[0], Height = pair.Second[1], X = pair.First.X + (pair.First.Width - pair.Second[0]) / 2, Y = pair.First.Y + (pair.First.Height - pair.Second[1]) / 2 })];
            var svgs = await page.EvaluateAsync<int>("document.querySelectorAll('#fixture [aria-label=Loading] svg path').length");
            var reference = await ReferenceMetrics.OfAsync("spinner-demo", theme);
            var sizes = reference.Where(element => element.Tag == "svg").ToList();
            var holders = reference.Where(element => element.Tag == "button" || element.Slot == "badge").ToList();
            var ours = await TextsAsync(page, ["Loading...", "Please wait", "Syncing", "Updating", "Processing"], "s.borderTopWidth === '1px' || s.backgroundColor !== 'rgba(0, 0, 0, 0)' || el.tagName === 'BUTTON'");
            var wrong = Boxes(theme, spinners.Take(4).ToList(), sizes, 1, colors: false).ToList();
            wrong.AddRange(spinners.Skip(4).Select(spinner => spinner.Width).SequenceEqual([16.0, 16, 12, 12, 12]) ? [] : [$"{theme}: inner spinners {string.Join(',', spinners.Skip(4).Select(spinner => spinner.Width))}"]);
            wrong.AddRange(ours.Zip(holders).SelectMany((pair, i) => pair.First is null ? [$"{theme}: holder {i} missing"] : Compare.Box($"{theme} holder {i}", pair.First, pair.Second, 1)));
            wrong.AddRange(svgs == 9 ? [] : [$"{theme}: {svgs} loader paths"]);
            var statuses = await page.Locator("#fixture [role='status'][aria-label='Loading']").CountAsync();
            wrong.AddRange(statuses == 9 ? [] : [$"{theme}: {statuses} spinners with role=status"]);
            return wrong;
        }));
    }

    /// <summary>
    /// The keys' boxes: every key's vertical position and height, the widths of the keys written in letters, and the
    /// horizontal positions of the keys no symbol precedes in their row (⌘ ⇧ ⌥ ⌃ ⏎ come from the machine's fallback font,
    /// Geist has none of them, so their widths are the machine's).
    /// </summary>
    internal static IEnumerable<string> Keys(string theme, IReadOnlyList<ElementMetrics> found, IReadOnlyList<ElementMetrics> expected) =>
        found.Count != expected.Count
            ? [$"{theme}: {found.Count} keys, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Compare.Box($"{theme} key {i}", pair.First, pair.Second, 1)
                .Where(error => !error.Contains(": width ", StringComparison.Ordinal) || pair.Second.Text.All(char.IsAscii))
                .Where(error => !error.Contains(": x ", StringComparison.Ordinal) || i is >= 4 and <= 7));

    [Fact]
    [Trait("AC", "KBD-01")]
    public async Task KeysAreComputedAsShadcnsAsync() =>
        Assert.Empty(await EachThemeAsync("kbd-demo", async (page, theme) =>
        {
            var found = await WhereAsync(page, "s.backgroundColor !== 'rgba(0, 0, 0, 0)' && s.borderTopLeftRadius === '6px' && el.getBoundingClientRect().height === 20");
            var expected = await ReferenceMetrics.SlotsAsync("kbd-demo", theme, "kbd");
            var texts = await TextsAsync(page, expected.Select(key => key.Text));
            var kbds = await page.Locator("#fixture kbd").CountAsync();
            return Keys(theme, found, expected)
                .Concat(kbds == expected.Count ? [] : [$"{theme}: {kbds} kbd elements, shadcn {expected.Count}"])
                .Concat(texts.Zip(expected).SelectMany((pair, i) => pair.First is null ? [$"{theme} key {i}: missing"] : Text($"{theme} key {i}", pair.First, pair.Second, box: false)));
        }));

    [Fact]
    [Trait("AC", "AVATAR-01")]
    public async Task AvatarsAreImagesAndFallbacksComputedAsShadcnsAsync() =>
        Assert.Empty(await EachThemeAsync("avatar-demo", async (page, theme) =>
        {
            var clips = await WhereAsync(page, "el.parentElement && el.parentElement.firstElementChild === el && [24, 32, 40].includes(el.parentElement.getBoundingClientRect().width) && parseFloat(s.borderTopLeftRadius) > 0 && getComputedStyle(el.parentElement).borderTopLeftRadius === '0px' && getComputedStyle(el.parentElement).display !== 'grid'");
            var images = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture img')].map((img) => img.src.slice(0, 22) + img.alt)");
            var rings = await WhereAsync(page, "s.boxShadow.includes('0px 0px 0px 2px')");
            var expected = await ReferenceMetrics.SlotsAsync("avatar-demo", theme, "avatar");
            var fallbacks = await ReferenceMetrics.SlotsAsync("avatar-demo", theme, "avatar-fallback");
            var wrong = Boxes(theme, clips, expected, 1, colors: false).ToList();
            wrong.AddRange(clips.Zip(expected).Where(pair => Math.Min(Compare.Px(pair.First.Radius), 99) != Math.Min(Compare.Px(pair.Second.Radius), 99)).Select(pair => $"{theme}: radius {pair.First.Radius}, shadcn {pair.Second.Radius}"));
            wrong.AddRange(images.SequenceEqual(["data:image/png;base64,@shadcn", "data:image/png;base64,@evilrabbit", "data:image/png;base64,@shadcn", "data:image/png;base64,@maxleiter", "data:image/png;base64,@evilrabbit"]) ? [] : [$"{theme}: images {string.Join(',', images)}"]);
            wrong.AddRange(rings.Count == 3 ? [] : [$"{theme}: {rings.Count} rings"]);
            var clipped = await page.EvaluateAsync<int>("[...document.querySelectorAll('#fixture img')].filter((img) => { let el = img.parentElement; while (el && getComputedStyle(el).overflow !== 'hidden') el = el.parentElement; return el && parseFloat(getComputedStyle(el).borderTopLeftRadius) >= 10 }).length");
            wrong.AddRange(clipped == 5 ? [] : [$"{theme}: {clipped} images clipped by a rounded part"]);
            var initials = await WhereAsync(page, "el.children.length === 0 && el.textContent.trim() === 'CN'");
            wrong.AddRange(initials.Zip(fallbacks.Skip(fallbacks.Count - 3)).Where(pair => pair.First.FontSize != pair.Second.FontSize || !CssColor.Parse(pair.First.Color).Near(CssColor.Parse(pair.Second.Color))).Select(pair => $"{theme}: fallback {pair.First.FontSize} {pair.First.Color}, shadcn {pair.Second.FontSize} {pair.Second.Color}"));
            wrong.AddRange(initials.Count == 3 ? [] : [$"{theme}: {initials.Count} initials"]);
            return wrong;
        }));

    [Fact]
    [Trait("AC", "AVATAR-06")]
    public async Task ABadgeAndAGroupCountFollowTheirAvatarsOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["avatar-badge", "avatar-group-count"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme) =>
            {
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var all = await Browser.PickAsync(page, "#fixture *");
                var parts = reference.Where(element => element.Slot is "avatar-badge" or "avatar-group-count" or "avatar-fallback").ToList();
                return Compare.Standing($"{fixture} {theme}", all, reference.Where(element => element.Slot == "avatar"), 1, colors: false)
                    .Concat(Compare.Standing($"{fixture} {theme}", all, parts, 1))
                    .Concat(parts.Where(element => element.Text.Length > 0).Where(element => !all.Any(found => found.Text == element.Text && CssColor.Parse(found.Color).Near(CssColor.Parse(element.Color))))
                        .Select(element => $"{fixture} {theme}: no {element.Text} in {element.Color}"));
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "AVATAR-04")]
    public async Task APostedCommandAddsAMemberToTheBoundGroupAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/avatar").ToString());
        var before = await page.Locator("main img").CountAsync();
        await page.ClickAsync("button:text-is('Add member')");
        await page.WaitForSelectorAsync("text=4 members");
        var after = await page.Locator("main img").CountAsync();
        var initials = await page.Locator("main :text-is('RP')").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(before, after);
        Assert.Equal(1, initials);
    }

    [Fact]
    [Trait("AC", "ALERT-01")]
    public async Task AlertsAreComputedAsShadcnsAsync() =>
        Assert.Empty(await EachThemeAsync("alert-demo", async (page, theme) =>
        {
            var reference = await ReferenceMetrics.OfAsync("alert-demo", theme);
            var titles = reference.Where(element => element.Slot == "alert-title").ToList();
            var descriptions = reference.Where(element => element.Slot == "alert-description").Take(1).ToList();
            var alerts = reference.Where(element => element.Slot == "alert").ToList();
            var foundTitles = await TextsAsync(page, titles.Select(title => title.Text));
            var foundAlerts = await TextsAsync(page, titles.Select(title => title.Text), "s.borderTopWidth === '1px'");
            var foundDescriptions = await TextsAsync(page, descriptions.Select(description => description.Text));
            var destructive = (await TextsAsync(page, ["Please verify your billing information and try again."])).Single();
            var wrong = new List<string>();
            wrong.AddRange(foundTitles.Zip(titles).SelectMany((pair, i) => Text($"{theme} title {i}", pair.First, pair.Second, 1)));
            wrong.AddRange(foundDescriptions.Zip(descriptions).SelectMany((pair, i) => Text($"{theme} description {i}", pair.First, pair.Second, 1)));
            wrong.AddRange(foundAlerts.Zip(alerts).SelectMany((pair, i) => pair.First is null ? [$"{theme} alert {i}: missing"] : Compare.Box($"{theme} alert {i}", pair.First, pair.Second, 1).Concat(Compare.Colors($"{theme} alert {i}", pair.First with { Color = pair.Second.Color }, pair.Second))
                .Concat(pair.First.Radius == pair.Second.Radius && pair.First.PaddingLeft == pair.Second.PaddingLeft && pair.First.PaddingTop == pair.Second.PaddingTop ? [] : [$"{theme} alert {i}: {pair.First.Radius} {pair.First.PaddingLeft} {pair.First.PaddingTop}"])));
            wrong.AddRange(await AlertSemanticsAsync(page, theme, titles, alerts.Count));
            var shadcnDestructive = reference.Single(element => element.Slot == "alert-description" && element.Text.StartsWith("Please", StringComparison.Ordinal));
            wrong.AddRange(destructive is not null && CssColor.Parse(destructive.Color).Near(CssColor.Parse(shadcnDestructive.Color)) ? [] : [$"{theme}: destructive description {destructive?.Color}, shadcn {shadcnDestructive.Color}"]);
            return wrong;
        }));

    private static async Task<IEnumerable<string>> AlertSemanticsAsync(IPage page, string theme, IEnumerable<ElementMetrics> titles, int alerts)
    {
        var spacings = await SpacingsAsync(page, titles.Select(title => title.Text));
        var roles = await page.Locator("#fixture [role='alert']").CountAsync();
        return spacings.All(spacing => spacing == "-0.35px") && roles == alerts ? [] : [$"{theme}: title tracking {string.Join(',', spacings)}, {roles} role=alert, shadcn {alerts}"];
    }

    [Fact]
    [Trait("AC", "ALERT-04")]
    public async Task APostedSaveTurnsTheBoundAlertDestructiveAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/alert").ToString());
        await page.ClickAsync("button:text-is('Save')");
        var error = page.Locator("main :text-is('Please enter your name and try again.')");
        await error.WaitForAsync();
        var color = await error.EvaluateAsync<string>("(el) => getComputedStyle(el).color");
        await page.FillAsync("input[aria-label='Name']", "Riley");
        await page.ClickAsync("button:text-is('Save')");
        await page.WaitForSelectorAsync("text=Saved as Riley.");
        await page.Context.CloseAsync();
        var expected = (await ReferenceMetrics.SlotsAsync("alert-demo", "light", "alert-description"))[^1].Color;

        Assert.True(CssColor.Parse(color).Near(CssColor.Parse(expected)), $"{color}, shadcn {expected}");
    }

    [Fact]
    [Trait("AC", "ASPECTRATIO-01")]
    public async Task TheAspectRatioImageHasShadcnsBoxAsync() =>
        Assert.Empty(await EachThemeAsync("aspect-ratio-demo", async (page, theme) =>
        {
            var image = (await Browser.PickAsync(page, "#fixture img")).Single();
            var clip = (await WhereAsync(page, "s.overflow === 'hidden' && el.querySelector('img')"))[^1];
            var expected = (await ReferenceMetrics.SlotsAsync("aspect-ratio-demo", theme, "aspect-ratio")).Single();
            return Compare.Box(theme, image, expected, 1).Concat(clip.Radius == "10px" ? [] : [$"{theme}: radius {clip.Radius}"]);
        }));

    [Fact]
    [Trait("AC", "TYPOGRAPHY-01")]
    public async Task TypographyIsComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["typography-headings", "typography-text"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme) =>
            {
                var reference = (await ReferenceMetrics.OfAsync(fixture, theme)).Where(element => element.Text.Length > 0 && element.Tag != "ul").ToList();
                var found = await TextsAsync(page, reference.Select(element => element.Text));
                var errors = found.Zip(reference).SelectMany(pair => TextStyle(theme, pair.First, pair.Second)).ToList();
                errors.AddRange(await TypographyElementsAsync(page, theme, fixture, reference));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    /// <summary>The elements typography writes (<c>h1</c> to <c>h3</c> with their tracking, <c>blockquote</c> and <c>code</c>).</summary>
    private static async Task<IEnumerable<string>> TypographyElementsAsync(IPage page, string theme, string fixture, IEnumerable<ElementMetrics> reference)
    {
        var tags = await page.EvaluateAsync<string[]>("[...document.querySelectorAll('#fixture h1, #fixture h2, #fixture h3, #fixture h4, #fixture blockquote, #fixture code')].map((h) => h.tagName)");
        if (fixture == "typography-text")
        {
            return tags.Contains("BLOCKQUOTE") && tags.Contains("CODE") ? [] : [$"{theme}: elements {string.Join(',', tags)}"];
        }

        var spacings = await SpacingsAsync(page, reference.Where(element => element.Tag is "h1" or "h2" or "h3").Select(element => element.Text));
        return tags.SequenceEqual(["H1", "H2", "H3", "H4"]) && spacings.SequenceEqual(["-0.9px", "-0.75px", "-0.6px"]) ? [] : [$"{theme}: headings {string.Join(',', tags)} tracking {string.Join(',', spacings)}"];
    }

    private static List<string> TextStyle(string theme, ElementMetrics? found, ElementMetrics expected)
    {
        var errors = Text($"{theme} {expected.Tag}", found, expected, 1, box: expected.Tag is not ("h1" or "h2" or "code" or "blockquote"));
        var weight = expected.Tag == "h1" ? "700" : expected.FontWeight;
        errors.AddRange(found is not null && found.FontWeight == weight && found.Decoration == (expected.Tag == "blockquote" ? "italic" : "normal") ? [] : [$"{theme} {expected.Tag}: weight {found?.FontWeight} style {found?.Decoration}, shadcn {weight}"]);
        return errors;
    }

    [Fact]
    [Trait("AC", "EMPTY-01")]
    public async Task EmptyStatesAreComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["empty-demo", "empty-outline"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme) =>
            {
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var title = reference.Single(element => element.Slot == "empty-title");
                var description = reference.Single(element => element.Slot == "empty-description");
                var tile = reference.Single(element => element.Slot == "empty-icon");
                var buttons = reference.Where(element => element.Tag == "button").ToList();
                var found = await TextsAsync(page, [title.Text, description.Text]);
                var foundTile = (await WhereAsync(page, "el.getBoundingClientRect().width === 40 && el.getBoundingClientRect().height === 40 && s.backgroundColor !== 'rgba(0, 0, 0, 0)'")).FirstOrDefault();
                var foundButtons = await TextsAsync(page, buttons.Select(button => button.Text), "el.tagName === 'BUTTON' || el.tagName === 'A'");
                var errors = Text($"{theme} title", found[0], title, box: false).Concat(Placed($"{theme} title", found[0], title, 1.5)).Concat(Text($"{theme} description", found[1], description, box: false)).ToList();
                errors.AddRange(found[1] is not null && Math.Abs(found[1]!.Y - description.Y) <= 1.5 ? [] : [$"{theme}: description at {found[1]?.Y}, shadcn {description.Y}"]);
                errors.AddRange(foundTile is not null ? Compare.Box($"{theme} tile", foundTile, tile, 1.5).Concat(Compare.Colors($"{theme} tile", foundTile, tile)) : [$"{theme}: no tile"]);
                errors.AddRange(foundButtons.Zip(buttons).SelectMany((pair, i) => pair.First is null ? [$"{theme} button {i}: missing"] : Compare.Box($"{theme} button {i}", pair.First, pair.Second, 1.5)));
                errors.AddRange(await EmptyLooksAsync(page, theme, fixture, title.Text));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    /// <summary>The empty state's title tracking and, in the outline one only, its dashed border.</summary>
    private static async Task<IEnumerable<string>> EmptyLooksAsync(IPage page, string theme, string fixture, string title)
    {
        var spacing = (await SpacingsAsync(page, [title]))[0];
        var dashed = await page.EvaluateAsync<int>("[...document.querySelectorAll('#fixture *')].filter((el) => getComputedStyle(el).borderTopStyle === 'dashed').length");
        return spacing == "-0.45px" && dashed == (fixture == "empty-outline" ? 1 : 0) ? [] : [$"{theme} {fixture}: title tracking {spacing}, {dashed} dashed borders"];
    }

    [Fact]
    [Trait("AC", "ITEM-01")]
    public async Task ItemsAreComputedAsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["item-demo", "item-variant"])
        {
            wrong.AddRange(await EachThemeAsync(fixture, async (page, theme) =>
            {
                var reference = await ReferenceMetrics.OfAsync(fixture, theme);
                var items = reference.Where(element => element.Slot == "item").ToList();
                var titles = reference.Where(element => element.Slot == "item-title").ToList();
                var descriptions = reference.Where(element => element.Slot == "item-description").ToList();
                var buttons = reference.Where(element => element.Tag == "button").ToList();
                var foundItems = await TextsAsync(page, titles.Select(title => title.Text), "s.borderTopWidth === '1px' && s.borderTopLeftRadius === '8px'");
                var errors = (await TextsAsync(page, titles.Select(title => title.Text))).Zip(titles).SelectMany((pair, i) => Text($"{theme} title {i}", pair.First, pair.Second, box: false).Concat(Placed($"{theme} title {i}", pair.First, pair.Second, 1))).ToList();
                errors.AddRange((await TextsAsync(page, descriptions.Select(description => description.Text))).Zip(descriptions).SelectMany((pair, i) => Text($"{theme} description {i}", pair.First, pair.Second, box: false).Concat(Placed($"{theme} description {i}", pair.First, pair.Second, 1))));
                errors.AddRange(foundItems.Zip(items).SelectMany((pair, i) => pair.First is null ? [$"{theme} item {i}: missing"] : Compare.Box($"{theme} item {i}", pair.First, pair.Second, 1).Concat(Compare.Colors($"{theme} item {i}", pair.First, pair.Second)).Concat(pair.First.PaddingLeft == pair.Second.PaddingLeft && pair.First.PaddingTop == pair.Second.PaddingTop ? [] : [$"{theme} item {i}: padding {pair.First.PaddingLeft} {pair.First.PaddingTop}"])));
                errors.AddRange(Boxes($"{theme} buttons", await Browser.PickAsync(page, "#fixture button"), buttons, 1, colors: false));
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TYPOGRAPHY-04")]
    public async Task TypographyTextsAreWrittenWithTheirHtmlElementsAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "typography-text", "light");
        var tags = await page.EvaluateAsync<string>("""
            () => ["A modal dialog", "The king", "Email address", "Enter your email", "\"After all", "@radix-ui"].map((start) =>
              [...document.querySelectorAll('#fixture p, #fixture small, #fixture blockquote, #fixture code')].find((el) => el.textContent.trim().startsWith(start))?.tagName ?? 'none').join()
              + '|' + [...document.querySelectorAll('#fixture ul > li')].length + ':' + document.querySelectorAll('#fixture ul').length
            """);
        await page.Context.CloseAsync();

        Assert.Equal("P,P,SMALL,P,BLOCKQUOTE,CODE|3:1", tags);
    }

    [Fact]
    [Trait("AC", "TYPOGRAPHY-06")]
    public async Task BalancedTextsAreWrittenWithTextWrapBalanceAsync()
    {
        var wrong = new List<string>();
        foreach (var (fixture, slots) in (IEnumerable<(string, string[])>)[("empty-demo", ["empty-title", "empty-description"]), ("field-content", ["field-description"]), ("item-header-footer", ["item-description"])])
        {
            var reference = (await ReferenceMetrics.OfAsync(fixture, "light")).Where(element => slots.Contains(element.Slot)).ToList();
            var page = await browser.FixtureAsync(gallery.Address, fixture, "light");
            var found = await page.EvaluateAsync<string[]>("""
                (texts) => texts.map((text) => {
                  const el = [...document.querySelectorAll('#fixture *')].filter((e) => e.textContent.trim() === text).pop()
                  return el ? getComputedStyle(el).textWrapStyle + ' ' + el.getBoundingClientRect().height : 'missing'
                })
                """, reference.Select(element => element.Text).ToArray());
            await page.Context.CloseAsync();
            wrong.AddRange(found.Zip(reference).Where(pair => !pair.First.StartsWith("balance ", StringComparison.Ordinal) || Math.Abs(double.Parse(pair.First[8..], System.Globalization.CultureInfo.InvariantCulture) - pair.Second.Height) > 1)
                .Select(pair => $"{fixture} '{pair.Second.Text}': {pair.First}, shadcn balance {pair.Second.Height}"));
        }

        var tip = await browser.OverlayAsync(gallery.Address, "tooltip-open", "light", new("hover", "button", null, false));
        var wrap = await tip.EvaluateAsync<string>("() => { const el = [...document.querySelectorAll('*')].filter((e) => e.textContent.trim() === 'Add to library').pop(); return el ? getComputedStyle(el).textWrapStyle : 'missing' }");
        await tip.Context.CloseAsync();

        Assert.Empty(wrong);
        Assert.Equal("balance", wrap);
    }

    [Fact]
    [Trait("AC", "ITEM-06")]
    public async Task AnItemsHeaderAndFooterAreLaidOutAsShadcnsAsync() =>
        Assert.Empty(await EachThemeAsync("item-header-footer", async (page, theme) =>
        {
            var reference = await ReferenceMetrics.OfAsync("item-header-footer", theme);
            ElementMetrics Slot(string slot) => reference.First(element => element.Slot == slot);
            var found = await TextsAsync(page, ["Invoice #1042Paid", "Paid", "Acme Inc.", "Design system subscription, October.", "View", "Due Oct 31$249.00"]);
            var item = (await TextsAsync(page, ["Invoice #1042Paid"], "s.borderTopWidth === '1px'"))[0];
            found[1] = (await TextsAsync(page, ["Paid"], "s.borderTopWidth === '1px'"))[0];
            return (item is null ? [$"{theme}: item missing"] : Compare.Box($"{theme} item", item, Slot("item"), 1.5))
                .Concat(found[0] is null ? [$"{theme}: header missing"] : Compare.Box($"{theme} header", found[0]!, Slot("item-header"), 1.5))
                .Concat(found[1] is null ? [$"{theme}: badge missing"] : Compare.Box($"{theme} badge", found[1]!, Slot("badge"), 1.5))
                .Concat(Placed($"{theme} title", found[2], Slot("item-title"), 1.5))
                .Concat(Placed($"{theme} description", found[3], Slot("item-description"), 1.5))
                .Concat(found[4] is null ? [$"{theme}: action missing"] : Compare.Box($"{theme} action", found[4]!, Slot("button"), 1.5))
                .Concat(found[5] is null ? [$"{theme}: footer missing"] : Compare.Box($"{theme} footer", found[5]!, Slot("item-footer"), 1.5));
        }));

    [Fact]
    [Trait("AC", "ITEM-08")]
    public async Task AnItemSeparatorIsALineBetweenGroupedItemsOnTheWebAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/item").ToString());
        var line = await page.EvaluateAsync<string>("""
            () => {
              const inbox = [...document.querySelectorAll('main *')].filter((e) => e.textContent.trim().startsWith('Inbox') && e.textContent.includes('Archive')).pop()
              if (!inbox) return 'missing'
              const [first, separator, second] = [...inbox.children], a = first.getBoundingClientRect(), b = separator.getBoundingClientRect(), c = second.getBoundingClientRect()
              return [b.height, b.width === inbox.getBoundingClientRect().width, b.top === a.bottom, c.top === b.bottom].join() + '|' + getComputedStyle(separator).backgroundColor
            }
            """);
        await page.Context.CloseAsync();

        Assert.Equal("1,true,true,true", line.Split('|')[0]);
        Assert.True(CssColor.Parse(line.Split('|')[^1]).Near(CssColor.Parse("oklch(0.922 0 0)")), line);
    }

    [Fact]
    [Trait("AC", "ITEM-04")]
    public async Task APostedActionRemovesItsItemAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/item").ToString());
        await page.Locator("main button:text-is('Remove')").Nth(1).ClickAsync();
        await page.WaitForSelectorAsync("text=2 items");
        var pictor = await page.Locator("main :text-is('Pictor')").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(0, pictor);
    }

    [Fact]
    [Trait("AC", "KBD-03")]
    public async Task APostedToggleTurnsTheBoundKeysIntoMacKeysAsync()
    {
        var page = await browser.PageAsync(gallery.Address, "light", 1280);
        await page.GotoAsync(new Uri(gallery.Address, "components/kbd").ToString());
        var before = await page.Locator("main :text-is('Ctrl')").CountAsync();
        await page.CheckAsync("label:has-text('Use Mac keys') input");
        await page.ClickAsync("button:text-is('Apply')");
        await page.WaitForSelectorAsync("text=Showing Mac keys");
        var after = await page.Locator("main :text-is('Ctrl')").CountAsync();
        var mac = await page.Locator("main :text-is('⌘')").CountAsync();
        await page.Context.CloseAsync();

        Assert.Equal(before - 1, after);
        Assert.Equal(2, mac);
    }
}
