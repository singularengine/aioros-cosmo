using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class ChatWebTests(GalleryHost gallery, Browser browser)
{
    private const string Leaves = """
        ([text, up]) => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          const all = [...document.querySelectorAll("#fixture *")]
          return all
            .filter((el) => el.textContent.trim() === text && ![...el.children].some((child) => child.textContent.trim() === text))
            .map((el) => {
              for (let i = 0; i < up; i++) el = el.parentElement
              while (up < 0 && el.parentElement && getComputedStyle(el).backgroundColor === "rgba(0, 0, 0, 0)") el = el.parentElement
              const r = el.getBoundingClientRect(), s = getComputedStyle(el)
              return { tag: el.tagName.toLowerCase(), text, x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height, background: s.backgroundColor, color: s.color,
                borderColor: s.borderTopColor, borderWidth: s.borderTopWidth, radius: s.borderTopLeftRadius, shadow: s.boxShadow, fontSize: s.fontSize, fontWeight: s.fontWeight,
                opacity: s.opacity, decoration: s.textDecorationLine, slot: "" }
            })
        }
        """;

    /// <summary>The elements whose text, spaces aside, is a given text, as Chromium lays them out.</summary>
    private const string Holding = """
        (text) => {
          const bare = (value) => value.replace(/\s+/g, "")
          const root = document.getElementById("fixture").getBoundingClientRect()
          return [...document.querySelectorAll("#fixture *")].filter((el) => bare(el.textContent) === bare(text)).map((el) => {
            const r = el.getBoundingClientRect(), s = getComputedStyle(el)
            return { tag: el.tagName.toLowerCase(), text, x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height, background: s.backgroundColor, color: s.color, slot: "" }
          })
        }
        """;

    private static readonly System.Text.Json.JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// The innermost elements holding a text (or the element <paramref name="up"/> levels above each, or, with a negative
    /// <paramref name="up"/>, the nearest one with a background), as Chromium lays them out.
    /// </summary>
    private static async Task<List<ElementMetrics>> LeavesAsync(IPage page, string text, int up = 0)
    {
        var json = await page.EvaluateAsync<System.Text.Json.JsonElement>(Leaves, new object[] { text, up });
        return System.Text.Json.JsonSerializer.Deserialize<List<ElementMetrics>>(json, Options) ?? [];
    }

    private static IEnumerable<string> Stands(string what, ElementMetrics found, ElementMetrics expected) =>
        Math.Abs(found.X - expected.X) <= 1.5 && Math.Abs(found.Y - expected.Y) <= 1.5 && Math.Abs(found.Height - expected.Height) <= 1.5 && Math.Abs(found.Width - expected.Width) <= 3
            ? []
            : [$"{what}: {found.X:0.##},{found.Y:0.##} {found.Width:0.##}x{found.Height:0.##}, shadcn {expected.X:0.##},{expected.Y:0.##} {expected.Width:0.##}x{expected.Height:0.##}"];

    /// <summary>Compares each of shadcn/ui's elements of a slot with the element of Cosmo's page holding its text (<paramref name="up"/> levels above it).</summary>
    private async Task<List<string>> SlotsAsync(string[] fixtures, string slot, int up, Func<ElementMetrics, ElementMetrics, string, IEnumerable<string>>? look = null)
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in fixtures)
            {
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                foreach (var (expected, i) in (await ReferenceMetrics.SlotsAsync(fixture, theme, slot)).Select((element, i) => (element, i)))
                {
                    var found = await LeavesAsync(page, expected.Text, up);
                    var what = $"{fixture} {theme} {slot} #{i} {expected.Text}";
                    var best = found.OrderBy(element => Math.Abs(element.Y - expected.Y) + Math.Abs(element.X - expected.X)).FirstOrDefault();
                    wrong.AddRange(best is null ? [$"{what}: not found"] : Stands(what, best, expected).Concat(look?.Invoke(best, expected, what) ?? []));
                }

                await page.Context.CloseAsync();
            }
        }

        return wrong;
    }

    private static IEnumerable<string> Colors(ElementMetrics found, ElementMetrics expected, string what) =>
        CssColor.Parse(found.Color).Near(CssColor.Parse(expected.Color), 2) && found.FontSize == expected.FontSize ? [] : [$"{what}: {found.Color} {found.FontSize}, shadcn {expected.Color} {expected.FontSize}"];

    private static IEnumerable<string> Backgrounds(ElementMetrics found, ElementMetrics expected, string what) =>
        CssColor.Parse(found.Background).Near(CssColor.Parse(expected.Background), 2) ? [] : [$"{what}: background {found.Background}, shadcn {expected.Background}"];

    [Fact]
    [Trait("AC", "MARKER-02")]
    public async Task EveryMarkerIsShadcnsOnTheWebAsync()
    {
        var wrong = await SlotsAsync(["marker-demo", "marker-variants", "marker-icon", "marker-border"], "marker-content", 0, Colors);

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BUBBLE-02")]
    public async Task EveryBubbleIsShadcnsOnTheWebAsync()
    {
        string[] fixtures = ["bubble-demo", "bubble-variants", "bubble-alignment", "bubble-group-demo"];
        var wrong = await SlotsAsync(fixtures, "bubble-content", 1, Backgrounds);
        wrong.AddRange(await SlotsAsync(fixtures, "bubble-reactions", -1));

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "MESSAGE-02")]
    public async Task EveryMessageIsShadcnsOnTheWebAsync()
    {
        string[] fixtures = ["message-demo", "message-group", "message-header-footer", "message-avatar"];
        var wrong = await SlotsAsync(fixtures, "bubble-content", 1, Backgrounds);
        wrong.AddRange(await SlotsAsync(fixtures, "message-header", 0));
        wrong.AddRange(await SlotsAsync(fixtures, "message-footer", 0));

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "ATTACHMENT-03")]
    public async Task EveryAttachmentIsShadcnsOnTheWebAsync()
    {
        string[] fixtures = ["attachment-demo", "attachment-states", "attachment-sizes", "attachment-group"];
        var wrong = await SlotsAsync(fixtures, "attachment-title", 0);
        wrong.AddRange(await SlotsAsync(fixtures, "attachment-description", 0, Colors));

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private const string Scroller = """
        () => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          const scrollers = [...document.querySelectorAll("#fixture *")].filter((el) => getComputedStyle(el).overflowY === "auto" && el.scrollHeight > el.clientHeight)
          const button = [...document.querySelectorAll("#fixture button")].find((el) => el.getAttribute("aria-label") === "Scroll to end")
          const box = (el) => { const r = el.getBoundingClientRect(); return { x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height } }
          return { scrollers: scrollers.map((el) => ({ ...box(el), text: el.textContent })), button: button ? box(button) : null }
        }
        """;

    [Fact]
    [Trait("AC", "MESSAGESCROLLER-03")]
    public async Task TheTranscriptScrollsInItsViewportOnTheWebAsync()
    {
        string[] messages = ["Deploying to prod real quick.", "It's 4:55 PM. On a Friday.", "It's a one-line change.", "It's always a one-line change. Alright, let me take a look.",
            "The build failed during dependency installation.", "Can you share the exact error?", "Something went wrong with the build. The libraries are not installed correctly.",
            "Try running the build again. I cleared the cache.", "It worked. Thanks!"];
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in (string[])["message-scroller-end", "message-scroller-start"])
            {
                var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
                var found = await page.EvaluateAsync<System.Text.Json.JsonElement>(Scroller);
                var viewport = (await ReferenceMetrics.SlotsAsync(fixture, theme, "message-scroller-viewport")).Single();
                var scrollers = found.GetProperty("scrollers").EnumerateArray().ToList();
                var what = $"{fixture} {theme}";
                if (scrollers.Count != 1)
                {
                    wrong.Add($"{what}: {scrollers.Count} scroll containers");
                }
                else
                {
                    var box = Box(scrollers[0], viewport);
                    wrong.AddRange(Stands($"{what} viewport", box, viewport));
                    var text = scrollers[0].GetProperty("text").GetString() ?? "";
                    wrong.AddRange(messages.Where(message => !text.Contains(message, StringComparison.Ordinal)).Select(message => $"{what}: {message} not in the viewport"));
                }

                wrong.AddRange(await OpeningAsync(page, what, fixture == "message-scroller-end", found.GetProperty("button")));
                await page.Context.CloseAsync();
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    /// <summary>Where a fixture opens: the end fixture with its last bubble 24 px above the viewport's bottom and its button hidden, the start fixture with its 32 px button shown.</summary>
    private static async Task<List<string>> OpeningAsync(Microsoft.Playwright.IPage page, string what, bool end, System.Text.Json.JsonElement button)
    {
        var opened = await page.EvaluateAsync<double[]>(Opened);
        var (last, hidden) = (opened[0], opened[1] > 0);
        var wrong = end == hidden && (!end || Math.Abs(last - 24) <= 1.5) ? new List<string>() : [$"{what}: last bubble {last} px above the viewport's bottom, button hidden {hidden}"];
        if (button.ValueKind == System.Text.Json.JsonValueKind.Null)
        {
            wrong.Add($"{what}: no Scroll to end button");
        }
        else if (!end && (Math.Abs(button.GetProperty("width").GetDouble() - 32) > 0.5 || Math.Abs(button.GetProperty("height").GetDouble() - 32) > 0.5))
        {
            wrong.Add($"{what} button {button}");
        }

        return wrong;
    }

    private const string Opened = "() => { const v = document.querySelector('#fixture [data-aioros-scroll-name]'), b = document.querySelector('#fixture button[aria-label=\\'Scroll to end\\']'), bubbles = [...v.querySelectorAll('div')].filter((d) => d.textContent === 'It worked. Thanks!'); return [Math.round(v.getBoundingClientRect().bottom - bubbles[0].getBoundingClientRect().bottom), getComputedStyle(b).display === 'none' ? 1 : 0] }";

    [Fact]
    [Trait("AC", "MESSAGESCROLLER-04")]
    public async Task TheTranscriptFollowsItsEndOnTheWebAsync()
    {
        const string Viewer = "document.querySelector('#fixture [data-aioros-scroll-name]')";
        const string Button = "document.querySelector('#fixture button[aria-label=\\'Scroll to end\\']')";
        var start = await browser.FixtureAsync(gallery.Address, "message-scroller-start", "light");
        var posts = 0;
        start.Request += (_, request) => posts += request.Method == "POST" ? 1 : 0;
        await start.ClickAsync("#fixture button[aria-label='Scroll to end']");
        await start.WaitForFunctionAsync($"() => getComputedStyle({Button}).display === 'none'", null, new() { PollingInterval = 20 });
        var scrolled = await start.EvaluateAsync<bool>($"() => {{ const v = {Viewer}; return v.scrollHeight - v.clientHeight - v.scrollTop <= 1 }}");
        await start.Context.CloseAsync();

        var end = await browser.FixtureAsync(gallery.Address, "message-scroller-end", "light");
        await end.EvaluateAsync($"() => {{ const v = {Viewer}; v.scrollTop = v.scrollTop - 120 }}");
        await end.WaitForFunctionAsync($"() => getComputedStyle({Button}).display !== 'none'", null, new() { PollingInterval = 20 });
        await end.EvaluateAsync($"() => {{ const v = {Viewer}; v.scrollTop = 0 }}");
        await end.WaitForFunctionAsync($"() => getComputedStyle({Button}).display === 'none'", null, new() { PollingInterval = 20 });
        await end.EvaluateAsync($"() => {{ const list = {Viewer}.firstElementChild; const copy = list.lastElementChild.cloneNode(true); copy.querySelector('div div div div div div').textContent = 'Shipped.'; list.append(copy) }}");
        var followed = await end.EvaluateAsync<bool>($"() => {{ const v = {Viewer}, last = [...v.querySelectorAll('div')].filter((d) => d.textContent === 'Shipped.').pop(); return v.scrollTop === 0 && last.getBoundingClientRect().bottom <= v.getBoundingClientRect().bottom }}");
        await end.Context.CloseAsync();

        Assert.Equal((true, 0, true), (scrolled, posts, followed));
    }

    private static ElementMetrics Box(System.Text.Json.JsonElement box, ElementMetrics like) =>
        like with { X = box.GetProperty("x").GetDouble(), Y = box.GetProperty("y").GetDouble(), Width = box.GetProperty("width").GetDouble(), Height = box.GetProperty("height").GetDouble() };
    /// <summary>Whether the element holding a text, or one of its ancestors up to the fixture, carries the shimmer utility.</summary>
    private const string Shimmers = """
        (text) => {
          const leaves = [...document.querySelectorAll("#fixture *")].filter((el) => el.textContent.trim() === text && ![...el.children].some((child) => child.textContent.trim() === text))
          return leaves.some((el) => { for (; el && el.id !== "fixture"; el = el.parentElement) if (el.classList.contains("shm") || el.classList.contains("shmd")) return getComputedStyle(el).backgroundClip === "text"; return false })
        }
        """;

    private static async Task<List<string>> ShimmerAsync(IPage page, string what, IEnumerable<(string Text, bool Shimmers)> texts)
    {
        var wrong = new List<string>();
        foreach (var (text, expected) in texts)
        {
            var found = await page.EvaluateAsync<bool>(Shimmers, text);
            wrong.AddRange(found == expected ? [] : [$"{what} {text}: shimmers {found}"]);
        }

        return wrong;
    }

    [Fact]
    [Trait("AC", "MARKER-04")]
    public async Task ShimmeringLinkedAndPressedMarkersAreShadcnsOnTheWebAsync()
    {
        var wrong = await SlotsAsync(["marker-shimmer", "marker-status", "marker-link-button"], "marker-content", 0, Colors);
        foreach (var theme in (string[])["light", "dark"])
        {
            var shimmer = await browser.FixtureAsync(gallery.Address, "marker-shimmer", theme);
            wrong.AddRange(await ShimmerAsync(shimmer, $"marker-shimmer {theme}", [("Thinking...", true), ("Reading 4 files", true)]));
            await shimmer.Context.CloseAsync();
            var status = await browser.FixtureAsync(gallery.Address, "marker-status", theme);
            wrong.AddRange(await ShimmerAsync(status, $"marker-status {theme}", [("Compacting conversation", false), ("Running tests", false)]));
            await status.Context.CloseAsync();
        }

        var page = await browser.FixtureAsync(gallery.Address, "marker-link-button", "light");
        var link = await page.EvaluateAsync<string[]>("""
            () => {
              const a = [...document.querySelectorAll("#fixture a")].find((el) => el.textContent.trim() === "View the pull request")
              const text = [...a.querySelectorAll("*")].find((el) => el.textContent.trim() === "View the pull request" && !el.children.length) ?? a
              const button = [...document.querySelectorAll("#fixture button")].find((el) => el.textContent.trim() === "Revert this change")
              const s = getComputedStyle(text)
              return [a.getAttribute("href") ?? "", String(a.querySelectorAll("svg").length), s.textDecorationLine, s.textUnderlineOffset, button ? "button" : "none"]
            }
            """);
        await page.Context.CloseAsync();
        wrong.AddRange(link[0].Length > 0 && link[1] != "0" && link[2] == "underline" && link[3] == "3px" && link[4] == "button" ? [] : [$"marker-link-button: {string.Join(' ', link)}"]);

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "BUBBLE-05")]
    public async Task ABubbleThatIsAButtonOrCollapsibleIsShadcnsOnTheWebAsync()
    {
        var wrong = await SlotsAsync(["bubble-link-button"], "bubble-content", -1, Backgrounds);
        foreach (var theme in (string[])["light", "dark"])
        {
            var shown = await browser.FixtureAsync(gallery.Address, "bubble-markdown", theme);
            foreach (var expected in await ReferenceMetrics.SlotsAsync("bubble-markdown", theme, "bubble-content"))
            {
                var json = await shown.EvaluateAsync<System.Text.Json.JsonElement>(Holding, expected.Text);
                var found = (System.Text.Json.JsonSerializer.Deserialize<List<ElementMetrics>>(json, Options) ?? [])
                    .OrderBy(element => Math.Round(Math.Abs(element.X - expected.X) + Math.Abs(element.Y - expected.Y) + Math.Abs(element.Width - expected.Width) + Math.Abs(element.Height - expected.Height)))
                    .ThenBy(static element => CssColor.Parse(element.Background).A == 0).FirstOrDefault();
                wrong.AddRange(found is null ? [$"bubble-markdown {theme} {expected.Text}: not found"] : Stands($"bubble-markdown {theme}", found, expected).Concat(Backgrounds(found, expected, $"bubble-markdown {theme}")));
            }

            await shown.Context.CloseAsync();
        }

        foreach (var theme in (string[])["light", "dark"])
        {
            var shown = await browser.FixtureAsync(gallery.Address, "bubble-collapsible", theme);
            var expected = (await ReferenceMetrics.SlotsAsync("bubble-collapsible", theme, "bubble-content"))[1];
            var preview = (await LeavesAsync(shown, expected.Text[..(expected.Text.IndexOf("...", StringComparison.Ordinal) + 3)], -1)).FirstOrDefault();
            wrong.AddRange(preview is null ? [$"bubble-collapsible {theme}: no preview"] : Stands($"bubble-collapsible {theme} bubble", preview, expected).Concat(Backgrounds(preview, expected, $"bubble-collapsible {theme}")));
            await shown.Context.CloseAsync();
        }

        var page = await browser.FixtureAsync(gallery.Address, "bubble-link-button", "light");
        var buttons = await page.EvaluateAsync<int>("() => [...document.querySelectorAll('#fixture button')].filter((el) => el.textContent.trim().startsWith('I ') || el.textContent.trim().startsWith('Something')).length");
        await page.Context.CloseAsync();
        wrong.AddRange(buttons == 3 ? [] : [$"bubble-link-button: {buttons} buttons"]);

        var collapsible = await browser.FixtureAsync(gallery.Address, "bubble-collapsible", "light");
        var posts = 0;
        collapsible.Request += (_, request) => posts += request.Method == "POST" ? 1 : 0;
        const string State = """
            () => {
              const button = [...document.querySelectorAll("#fixture button")].find((el) => /Show (more|less)/.test(el.innerText))
              const shown = [...document.querySelectorAll("#fixture *")].filter((el) => el.checkVisibility() && !el.children.length).map((el) => el.textContent.trim())
              return [button.innerText.trim(), button.getAttribute("aria-expanded") ?? "", String(shown.some((text) => text.includes("I also recommend")))]
            }
            """;
        var closed = string.Join('|', await collapsible.EvaluateAsync<string[]>(State));
        await collapsible.GetByRole(AriaRole.Button, new() { Name = "Show more" }).ClickAsync();
        await collapsible.WaitForFunctionAsync("() => [...document.querySelectorAll('#fixture button')].some((el) => el.innerText.trim() === 'Show less')", null, new() { PollingInterval = 20 });
        var open = string.Join('|', await collapsible.EvaluateAsync<string[]>(State));
        await collapsible.GetByRole(AriaRole.Button, new() { Name = "Show less" }).ClickAsync();
        await collapsible.WaitForFunctionAsync("() => [...document.querySelectorAll('#fixture button')].some((el) => el.innerText.trim() === 'Show more')", null, new() { PollingInterval = 20 });
        var again = string.Join('|', await collapsible.EvaluateAsync<string[]>(State));
        await collapsible.Context.CloseAsync();

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
        Assert.Equal(("Show more|false|false", "Show less|true|true", "Show more|false|false", 0), (closed, open, again, posts));
    }

    [Fact]
    [Trait("AC", "ATTACHMENT-05")]
    public async Task AnAttachmentWithATriggerOrAnImageIsShadcnsOnTheWebAsync()
    {
        var wrong = await SlotsAsync(["attachment-trigger", "attachment-image"], "attachment-title", 0);
        foreach (var theme in (string[])["light", "dark"])
        {
            var states = await browser.FixtureAsync(gallery.Address, "attachment-states", theme);
            var titles = (await ReferenceMetrics.SlotsAsync("attachment-states", theme, "attachment-title")).Select(static title => title.Text).ToList();
            var descriptions = (await ReferenceMetrics.SlotsAsync("attachment-states", theme, "attachment-description")).Select(static description => description.Text).ToList();
            wrong.AddRange(await ShimmerAsync(states, $"attachment-states {theme}", titles.Zip(descriptions, (title, description) => (title, description.Contains("Uploading", StringComparison.Ordinal) || description.Contains("Processing", StringComparison.Ordinal)))));
            await states.Context.CloseAsync();
        }

        var page = await browser.FixtureAsync(gallery.Address, "attachment-trigger", "light");
        var posts = 0;
        page.Request += (_, request) => posts += request.Method == "POST" ? 1 : 0;
        await page.GetByRole(AriaRole.Button, new() { Name = "Preview research-summary.pdf" }).ClickAsync();
        await page.WaitForSelectorAsync("dialog[open]", new() { State = WaitForSelectorState.Visible });
        var title = await page.EvaluateAsync<string>("() => document.querySelector('dialog[open]').innerText");
        await page.Context.CloseAsync();
        wrong.AddRange(title.Contains("The attachment trigger fills the card", StringComparison.Ordinal) && posts == 0 ? [] : [$"attachment-trigger: dialog {title}, {posts} posts"]);

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "TRACE-05")]
    public async Task ARunningTitleShimmersOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            var page = await browser.FixtureAsync(gallery.Address, "trace-status", theme);
            wrong.AddRange(await ShimmerAsync(page, $"trace-status {theme}", [("Ran pnpm install", false), ("Failed to run pnpm lint", false), ("Running pnpm build", true), ("Run pnpm test", false)]));
            await page.Context.CloseAsync();
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
    [Fact]
    [Trait("AC", "MESSAGESCROLLER-06")]
    public async Task TheTranscriptFadesAtItsBottomOnTheWebAsync()
    {
        var fades = new List<string>();
        foreach (var fixture in (string[])["message-scroller-start", "message-scroller-end"])
        {
            var page = await browser.FixtureAsync(gallery.Address, fixture, "light");
            fades.Add(await page.EvaluateAsync<string>("() => { const v = document.querySelector('#fixture [data-aioros-scroll-name]'); return [...v.classList].some(c => c === 'sfb' || c === 'sfbr') + ' ' + !/^0(px|%)?$/.test(getComputedStyle(v).getPropertyValue('--sf-b').trim()) }"));
            await page.Context.CloseAsync();
        }

        Assert.Equal(["true true", "true false"], fades);
    }
}
