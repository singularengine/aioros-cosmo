using System.Text.Json;
using Aioros.Cosmo.Testing;
using Microsoft.Playwright;

namespace Aioros.Cosmo.Gallery.Tests;

public sealed class QuestionnaireWebTests(GalleryHost gallery, Browser browser)
{
    private const string Elements = """
        () => {
          const root = document.getElementById("fixture").getBoundingClientRect()
          return [...document.querySelectorAll("#fixture *")].map((el) => {
            const r = el.getBoundingClientRect(), s = getComputedStyle(el)
            return { text: el.textContent.trim(), tag: el.tagName.toLowerCase(), placeholder: el.getAttribute("placeholder") ?? "", x: r.x - root.x, y: r.y - root.y, width: r.width, height: r.height, color: s.color }
          })
        }
        """;

    private const string Field = """
        () => [...document.querySelectorAll("#fixture input:not([type=radio]):not([type=checkbox]):not([type=hidden])")].filter((input) => input.checkVisibility()).map((input) => input.placeholder)
        """;

    private const string Inputs = """
        () => [...document.querySelectorAll("#fixture input[type=radio], #fixture input[type=checkbox]")]
          .filter((input) => input.offsetParent !== null || input.closest("label")?.offsetParent !== null)
          .map((input) => ({ type: input.type, label: input.closest("label")?.textContent.trim() ?? "" }))
        """;

    private static readonly string[] Fixtures = ["questionnaire-demo", "questionnaire-multiple", "questionnaire-freeform",
        "questionnaire-animated", "questionnaire-card", "questionnaire-conditional", "questionnaire-controlled", "questionnaire-navigation-state", "questionnaire-progress",
        "questionnaire-resume", "questionnaire-shortcuts", "questionnaire-skip", "questionnaire-validation"];

    private sealed record Box(string Text, string Tag, string Placeholder, double X, double Y, double Width, double Height, string Color);

    private sealed record Input(string Type, string Label);

    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>
    /// Whether an element of the page is shadcn/ui's: its text (none for an input), its box within 1.5 px (3 px for a
    /// width set by its text or an action's place, the progress's width being shadcn/ui's 14ch minimum) and its color (a
    /// shortcut's on its letter).
    /// </summary>
    private static bool Matches(Box box, ElementMetrics element, List<Box> boxes)
    {
        var input = element.Slot is "questionnaire-choice-indicator" or "questionnaire-input";
        var painted = element.Slot is "questionnaire-choice-shortcut" or "questionnaire-progress"
            ? boxes.Any(inner => inner.Text == element.Text.Trim() && inner.X >= box.X && inner.X + inner.Width <= box.X + box.Width && CssColor.Parse(inner.Color).Near(CssColor.Parse(element.Color), 2))
            : CssColor.Parse(box.Color).Near(CssColor.Parse(element.Color), 2);
        var text = input ? box.Tag == "input" : box.Text == element.Text.Trim();
        var across = element.Tag == "button" ? 3 : 1.5;
        var placed = Math.Abs(box.X - element.X) <= across && Math.Abs(box.Y - element.Y) <= 1.5;
        var sized = Math.Abs(box.Height - element.Height) <= 1.5 && (element.Slot == "questionnaire-progress" || Math.Abs(box.Width - element.Width) <= 3);
        return text && placed && sized && (input || painted);
    }

    /// <summary>What differs from shadcn/ui on a fixture's page: the shown question's boxes and colors, its choices' cards, their native inputs and the freeform field, and a choice checked without a request.</summary>
    private async Task<List<string>> CheckAsync(string fixture, string theme)
    {
        var wrong = new List<string>();
        var page = await browser.FixtureAsync(gallery.Address, fixture, theme);
        var boxes = JsonSerializer.Deserialize<List<Box>>(await page.EvaluateAsync<JsonElement>(Elements), Options) ?? [];
        var reference = await ReferenceMetrics.OfAsync(fixture, theme);
        var what = $"{fixture} {theme}";
        wrong.AddRange(reference.Where(static element => element.Width > 0 && element.Slot is "questionnaire-progress" or "questionnaire-title" or "questionnaire-description" or "questionnaire-choice" or "questionnaire-choice-indicator"
                or "questionnaire-choice-shortcut" or "questionnaire-input" or "questionnaire-next" or "questionnaire-submit")
            .Where(element => !boxes.Any(box => Matches(box, element, boxes)))
            .Select(element => $"{what} {element.Slot} {element.Text}: shadcn {element.X:0.##},{element.Y:0.##} {element.Width:0.##}x{element.Height:0.##} {element.Color}"));
        var inputs = JsonSerializer.Deserialize<List<Input>>(await page.EvaluateAsync<JsonElement>(Inputs), Options) ?? [];
        var labels = reference.Where(static element => element.Slot == "questionnaire-choice-label" && element.Width > 0).Select(static element => element.Text.Trim()).ToList();
        var type = fixture is "questionnaire-multiple" or "questionnaire-resume" ? "checkbox" : "radio";
        wrong.AddRange(inputs.Count == labels.Count && inputs.Zip(labels).All(pair => pair.First.Label.StartsWith(pair.Second, StringComparison.Ordinal) && pair.First.Type == type)
            ? []
            : [$"{what} inputs {string.Join(", ", inputs.Select(static input => $"{input.Type} {input.Label}"))}, shadcn {type} {string.Join(", ", labels)}"]);
        var placeholders = (await page.EvaluateAsync<string[]>(Field)).ToList();
        var expected = reference.Count(static element => element.Slot == "questionnaire-input" && element.Width > 0);
        wrong.AddRange(placeholders.Count == expected ? [] : [$"{what} fields {string.Join(", ", placeholders)}"]);
        var address = page.Url;
        var second = $"() => [...document.querySelectorAll('#fixture input[type={type}]')].filter((input) => input.closest('label')?.checkVisibility())[1].checked";
        var was = await page.EvaluateAsync<bool>(second);
        await page.Locator($"#fixture label:has(input[type={type}]):visible").Nth(1).ClickAsync();
        var now = await page.EvaluateAsync<bool>(second);
        wrong.AddRange(now != was && page.Url == address ? [] : [$"{what}: second choice {was} then {now} at {page.Url}"]);
        await page.Context.CloseAsync();
        return wrong;
    }

    /// <summary>Clicks an action and waits for the answer to show <paramref name="shown"/>; the answer must be the page, never an error.</summary>
    private static async Task<string> PressAsync(IPage page, string action, string shown)
    {
        var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
        await page.ClickAsync($"#fixture button:has-text('{action}')");
        var response = await answered;
        await page.Locator("#fixture").GetByText(shown, new() { Exact = true }).First.WaitForAsync(new() { Timeout = 10000 });
        return $"{action} {response.Status}";
    }

    private static Task<bool> ShownAsync(IPage page, string text) => page.Locator("#fixture").GetByText(text, new() { Exact = true }).First.IsVisibleAsync();

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-06")]
    public async Task EveryStepPostsOnTheWebAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "questionnaire-demo", "light");
        var statuses = new List<string> { await PressAsync(page, "Next", "Choose an answer to continue.") };
        var invalid = await ShownAsync(page, "Question 1 of 3");
        await page.ClickAsync("#fixture label:has-text('Approval checkpoints')");
        statuses.Add(await PressAsync(page, "Next", "Question 2 of 3"));
        var second = (await ShownAsync(page, "Previous"), await ShownAsync(page, "Skip"));
        statuses.Add(await PressAsync(page, "Skip", "Question 3 of 3"));
        var third = await ShownAsync(page, "Save plan");
        statuses.Add(await PressAsync(page, "Previous", "Question 2 of 3"));
        statuses.Add(await PressAsync(page, "Next", "Question 3 of 3"));
        await page.ClickAsync("#fixture label:has-text('Start now')");
        statuses.Add(await PressAsync(page, "Save plan", "Question 3 of 3"));
        var submitted = !await ShownAsync(page, "Choose an answer to continue.") && !await ShownAsync(page, "Choose an answer or skip this question.");
        statuses.Add(await PressAsync(page, "Previous", "Question 2 of 3"));
        statuses.Add(await PressAsync(page, "Previous", "Question 1 of 3"));
        var kept = await page.EvaluateAsync<int>("() => [...document.querySelectorAll('#fixture input[type=radio]')].filter((input) => input.offsetParent !== null || input.closest('label')?.offsetParent !== null).findIndex((input) => input.checked)");
        await page.Context.CloseAsync();

        Assert.All(statuses, status => Assert.EndsWith(" 200", status, StringComparison.Ordinal));
        Assert.True(invalid);
        Assert.Equal((true, true), second);
        Assert.True(third);
        Assert.True(submitted);
        Assert.Equal(1, kept);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-10")]
    public async Task TheKeysMoveThroughTheQuestionsOnTheWebAsync()
    {
        var page = await browser.FixtureAsync(gallery.Address, "questionnaire-demo", "light");
        async Task<string> KeyAsync(string focus, string key, string shown)
        {
            var answered = page.WaitForResponseAsync(response => response.Request.Method == "POST");
            await page.FocusAsync(focus);
            await page.Keyboard.PressAsync(key);
            var response = await answered;
            await page.Locator("#fixture").GetByText(shown, new() { Exact = true }).First.WaitForAsync(new() { Timeout = 10000 });
            return $"{key} {response.Status}";
        }

        await page.ClickAsync("#fixture label:has-text('Approval checkpoints')");
        var statuses = new List<string> { await KeyAsync("#fixture input[type=radio]:checked", "Enter", "Question 2 of 3") };
        const string Check = "#fixture label:has-text('Progress') input[type=checkbox]";
        statuses.Add(await KeyAsync(Check, "Control+Enter", "Choose an answer or skip this question."));
        statuses.Add(await KeyAsync(Check, "ArrowLeft", "Question 1 of 3"));
        await page.Context.CloseAsync();

        Assert.Equal(["Enter 200", "Control+Enter 200", "ArrowLeft 200"], statuses);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-02")]
    public async Task TheShownQuestionIsShadcnsOnTheWebAsync()
    {
        var wrong = new List<string>();
        foreach (var theme in (string[])["light", "dark"])
        {
            foreach (var fixture in Fixtures)
            {
                wrong.AddRange(await CheckAsync(fixture, theme));
            }
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
    [Fact]
    [Trait("AC", "QUESTIONNAIRE-08")]
    public async Task TheDocsQuestionnairesBehaveOnTheWebAsync()
    {
        var wrong = new List<string>();
        var waiting = await browser.FixtureAsync(gallery.Address, "questionnaire-navigation-state", "light");
        var posts = 0;
        waiting.Request += (_, request) => posts += Browser.Sends(request);
        const string Next = "() => [...document.querySelectorAll('#fixture button')].find((b) => b.textContent.trim() === 'Next' && b.checkVisibility()).disabled";
        var before = await waiting.EvaluateAsync<bool>(Next);
        await waiting.ClickAsync("#fixture label:has-text('Project files and tests')");
        await waiting.WaitForFunctionAsync($"() => !({Next})()", null, new() { PollingInterval = 20 });
        wrong.AddRange(before && posts == 0 ? [] : [$"navigation state: Next disabled {before}, {posts} requests"]);
        await waiting.Context.CloseAsync();

        var dialog = await browser.FixtureAsync(gallery.Address, "questionnaire-dialog", "light");
        var opened = 0;
        dialog.Request += (_, request) => opened += Browser.Sends(request);
        await dialog.ClickAsync("#fixture button:has-text('Open clarification')");
        await dialog.WaitForSelectorAsync("dialog[open]", new() { State = WaitForSelectorState.Visible });
        var title = await dialog.Locator("dialog[open]").GetByText("Which files are in scope?").IsVisibleAsync();
        wrong.AddRange(title && opened == 0 ? [] : [$"dialog: title {title}, {opened} requests"]);
        await dialog.Context.CloseAsync();

        var controlled = await browser.FixtureAsync(gallery.Address, "questionnaire-controlled", "light");
        await controlled.ClickAsync("#fixture label:has-text('Only the target component')");
        await PressAsync(controlled, "Next", "Current checkpoint: Verification");
        await controlled.Context.CloseAsync();

        var conditional = await browser.FixtureAsync(gallery.Address, "questionnaire-conditional", "light");
        await conditional.ClickAsync("#fixture label:has-text('Cloud workspace')");
        await PressAsync(conditional, "Next", "Which cloud environment should it use?");
        wrong.AddRange(await ShownAsync(conditional, "Question 2 of 3") ? [] : ["conditional: not Question 2 of 3"]);
        await conditional.Context.CloseAsync();

        var validation = await browser.FixtureAsync(gallery.Address, "questionnaire-validation", "light");
        await validation.ClickAsync("#fixture label:has-text('Concise summary')");
        await PressAsync(validation, "Next", "Who will read the answer?");
        await validation.ClickAsync("#fixture label:has-text('Public audience')");
        await PressAsync(validation, "Validate answers", "Public answers need enough context. Choose a complete answer.");
        await validation.Context.CloseAsync();

        var resume = await browser.FixtureAsync(gallery.Address, "questionnaire-resume", "light");
        await resume.ClickAsync("#fixture label:has-text('Perform a manual smoke test')");
        await PressAsync(resume, "Reset changes", "Question 2 of 3");
        var restored = await resume.EvaluateAsync<string>("() => [...document.querySelectorAll('#fixture input[type=checkbox]')].map((i) => i.checked ? '1' : '0').join('')");
        wrong.AddRange(restored == "110" ? [] : [$"resume: {restored} after Reset"]);
        await resume.Context.CloseAsync();

        var shortcuts = await browser.FixtureAsync(gallery.Address, "questionnaire-shortcuts", "light");
        var changed = shortcuts.WaitForResponseAsync(response => response.Request.Method == "POST");
        await shortcuts.Locator("#fixture select").SelectOptionAsync(new SelectOptionValue { Label = "Numbers" });
        await changed;
        await shortcuts.Locator("#fixture").GetByText("1", new() { Exact = true }).First.WaitForAsync(new() { Timeout = 10000 });
        await shortcuts.Context.CloseAsync();

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
