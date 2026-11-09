using System.Globalization;
using System.Windows.Input;
using Aioros.Cosmo.Chat;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Chat;

public sealed class QuestionnaireTests(DesktopSession desktop)
{
    internal static readonly string[] Questionnaires = ["questionnaire-demo", "questionnaire-multiple", "questionnaire-freeform",
        "questionnaire-animated", "questionnaire-card", "questionnaire-conditional", "questionnaire-controlled", "questionnaire-navigation-state", "questionnaire-progress",
        "questionnaire-resume", "questionnaire-shortcuts", "questionnaire-skip", "questionnaire-validation"];

    /// <summary>Whether a box is shadcn/ui's: an action's x may move with its text's width, as the action stands at the end.</summary>
    private static IEnumerable<string> Within(string what, Rect found, ElementMetrics expected, double width) =>
        Math.Abs(found.X - expected.X) <= (expected.Tag == "button" ? width : 1.5) && Math.Abs(found.Width - expected.Width) <= width && Math.Abs(found.Y - expected.Y) <= 3 && Math.Abs(found.Height - expected.Height) <= 3
            ? []
            : [$"{what}: {found}, shadcn {Of(expected)}"];

    private static IEnumerable<string> Boxes(string what, Control root, IReadOnlyList<ElementMetrics> reference, string slot, List<Control> found, double width = 1.5)
    {
        var expected = reference.Where(element => element.Slot == slot && element.Width > 0).ToList();
        return found.Count != expected.Count
            ? [$"{what}: {found.Count} {slot}, shadcn {expected.Count}"]
            : found.Zip(expected).SelectMany((pair, i) => Within($"{what} {slot} #{i} {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second, width));
    }

    private static List<Control> Shown<T>(Control root, Func<T, Control> part)
        where T : Control =>
        [.. All<T>(root).Where(static control => control.IsEffectivelyVisible).Select(part).Where(static control => control.IsEffectivelyVisible)];

    private static List<ToggleButton> Choices(Control root) => [.. All<ToggleButton>(root).Where(static choice => choice is QuestionnaireChoice or QuestionnaireCheck && choice.IsEffectivelyVisible)];

    private static IEnumerable<string> Looks(string what, IReadOnlyList<ElementMetrics> reference, List<ToggleButton> choices)
    {
        var expected = reference.Where(element => element.Slot == "questionnaire-choice" && element.Width > 0).ToList();
        var shortcuts = reference.Where(element => element.Slot == "questionnaire-choice-shortcut" && element.Width > 0).Select(static element => element.Text).ToList();
        var errors = choices.Count == expected.Count
            ? choices.Zip(expected).SelectMany(pair => Paints($"{what} {pair.Second.Text} border", pair.First.BorderBrush, pair.Second.BorderColor)
                .Concat(pair.First.CornerRadius.TopLeft == 18 ? [] : [$"{what} {pair.Second.Text} radius {pair.First.CornerRadius}"]))
            : [];
        var letters = choices.Select(static choice => choice.GetValue(QuestionnaireChoice.ShortcutProperty)).OfType<string>().ToList();
        return errors.Concat(letters.SequenceEqual(shortcuts) ? [] : [$"{what} shortcuts {string.Join(",", letters)}, shadcn {string.Join(",", shortcuts)}"]);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-01")]
    public async Task EveryQuestionnaireIsShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in Questionnaires)
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (_, root, reference, theme) =>
            {
                var what = $"{fixture} {theme}";
                var choices = Choices(root);
                var errors = Boxes(what, root, reference, "questionnaire", [.. All<Questionnaire>(root)]).ToList();
                errors.AddRange(Boxes(what, root, reference, "questionnaire-item", Shown<QuestionnaireItem>(root, static item => item)));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-title", Shown<QuestionnaireItem>(root, static item => Windows.Part<ContentPresenter>(item, "PART_Title")), 3));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-description", Shown<QuestionnaireItem>(root, static item => Windows.Part<ContentPresenter>(item, "PART_Description"))));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-choice", [.. choices]));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-choice-indicator", [.. choices.Select(static choice => (Control)Windows.Part<Border>(choice, "PART_Indicator"))]));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-choice-label", [.. choices.Select(static choice => (Control)Windows.Part<ContentPresenter>(choice, "PART_ContentPresenter").Parent!)]));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-choice-shortcut", [.. choices.Select(static choice => (Control)Windows.Part<Border>(choice, "PART_Shortcut")).Where(static part => part.IsEffectivelyVisible)]));
                errors.AddRange(Boxes(what, root, reference, "questionnaire-input", Shown<QuestionnaireInput>(root, static input => input)));
                foreach (var action in (string[])["previous", "skip", "next", "submit"])
                {
                    var part = "PART_" + char.ToUpperInvariant(action[0]) + action[1..];
                    errors.AddRange(Boxes(what, root, reference, "questionnaire-" + action, Shown<Questionnaire>(root, questionnaire => Windows.Part<Button>(questionnaire, part)), 3));
                }

                errors.AddRange(Looks(what, reference, choices));
                errors.AddRange(Shown<Questionnaire>(root, static questionnaire => Windows.Part<Button>(questionnaire, "PART_Next")).Concat(Shown<Questionnaire>(root, static questionnaire => Windows.Part<Button>(questionnaire, "PART_Submit")))
                    .SelectMany(button => Paints($"{what} action", ((Button)button).Background, reference.First(element => element.Slot is "questionnaire-next" or "questionnaire-submit" && element.Width > 0).Background)));
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    private static (Window Window, Questionnaire Questionnaire) Show(string fixture)
    {
        var window = DisplayFixtures.Show(fixture, ThemeVariant.Light);
        return (window, All<Questionnaire>((Control)window.Content!).Single());
    }

    private static Button Action(Questionnaire questionnaire, string name) => Windows.Part<Button>(questionnaire, name);

    /// <summary>Clicks a button after moving the pointer off the buttons, as a button shown under a still pointer takes no click.</summary>
    private static void Press(Window window, Button button)
    {
        Windows.Away(window, new Point(1, 1));
        Windows.Click(window, button);
    }

    private static (int Index, string Progress, bool Invalid, string? Error, bool Previous, bool Skip, bool Next, bool Submit) State(Questionnaire questionnaire)
    {
        var item = questionnaire.Questions[questionnaire.CurrentIndex];
        var error = Windows.Part<TextBlock>(item, "PART_Error");
        return (questionnaire.CurrentIndex, questionnaire.Progress, item.IsInvalid, error.IsEffectivelyVisible ? error.Text : null,
            Action(questionnaire, "PART_Previous").IsEffectivelyVisible, Action(questionnaire, "PART_Skip").IsEffectivelyVisible,
            Action(questionnaire, "PART_Next").IsEffectivelyVisible, Action(questionnaire, "PART_Submit").IsEffectivelyVisible);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-03")]
    public async Task TheQuestionsAreAnsweredOneAtATimeAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var (window, questionnaire) = Show("questionnaire-demo");
            Windows.Click(window, Action(questionnaire, "PART_Next"));
            var unanswered = State(questionnaire);
            var error = Windows.Paint(Windows.Part<TextBlock>(questionnaire.Questions[0], "PART_Error").Foreground);
            Windows.Click(window, Choices(questionnaire)[1]);
            var answered = State(questionnaire);
            Windows.Click(window, Action(questionnaire, "PART_Next"));
            var second = State(questionnaire);
            Windows.Click(window, Action(questionnaire, "PART_Next"));
            var optional = State(questionnaire);
            Windows.Click(window, Action(questionnaire, "PART_Skip"));
            var third = State(questionnaire);
            Windows.Click(window, Action(questionnaire, "PART_Previous"));
            var back = State(questionnaire);
            var destructive = Windows.Paint((IBrush)window.FindResource(window.ActualThemeVariant, "CosmoDestructiveBrush")!);
            window.Close();
            return (unanswered, error == destructive, answered, second, optional, third, back);
        });

        Assert.Equal((0, "Question 1 of 3", true, "Choose an answer to continue.", false, false, true, false), found.unanswered);
        Assert.True(found.Item2);
        Assert.Equal((0, "Question 1 of 3", false, null, false, false, true, false), found.answered);
        Assert.Equal((1, "Question 2 of 3", false, null, true, true, true, false), found.second);
        Assert.Equal((1, "Question 2 of 3", true, "Choose an answer or skip this question.", true, true, true, false), found.optional);
        Assert.Equal((2, "Question 3 of 3", false, null, true, false, false, true), found.third);
        Assert.Equal((1, "Question 2 of 3", false, null, true, true, true, false), found.back);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-04")]
    public async Task LettersCheckChoicesButTypeInTheInputAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var (window, questionnaire) = Show("questionnaire-freeform");
            Windows.Key(window, Key.B);
            var pressed = Choices(questionnaire).Select(static choice => choice.IsChecked == true).ToList();
            var input = All<QuestionnaireInput>(questionnaire).Single();
            input.Focus();
            Windows.Settle();
            foreach (var letter in "a b c")
            {
                Windows.Key(window, letter == ' ' ? Key.Space : Key.A + (letter - 'a'));
                Windows.Type(window, letter.ToString());
            }

            var typed = (input.Text, Choices(questionnaire).Select(static choice => choice.IsChecked == true).ToList(), questionnaire.CurrentIndex);
            window.Close();
            return (pressed, typed);
        });

        Assert.Equal([false, true, false], found.pressed);
        Assert.Equal("a b c", found.typed.Text);
        Assert.Equal([false, true, false], found.typed.Item2);
        Assert.Equal(0, found.typed.CurrentIndex);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-09")]
    public async Task TheKeysMoveThroughTheQuestionsAsShadcnsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var (window, questionnaire) = Show("questionnaire-demo");
            var steps = new List<string>();
            void Step(Key key, RawInputModifiers modifiers = RawInputModifiers.None)
            {
                Windows.Key(window, key, modifiers);
                var focused = TopLevel.GetTopLevel(questionnaire)!.FocusManager?.GetFocusedElement();
                var choices = Choices(questionnaire);
                var at = focused is ToggleButton toggle ? choices.IndexOf(toggle).ToString(CultureInfo.InvariantCulture) : focused is QuestionnaireItem ? "item" : "-";
                steps.Add($"{questionnaire.CurrentIndex} {string.Concat(choices.Select(static choice => choice.IsChecked == true ? '1' : '0'))} {at} {questionnaire.Questions[questionnaire.CurrentIndex].IsInvalid}");
            }

            Choices(questionnaire)[0].Focus();
            Windows.Settle();
            Step(Key.Down);
            Step(Key.Right);
            Step(Key.Left);
            Step(Key.Enter);
            Step(Key.Right);
            Step(Key.Down);
            Step(Key.Enter);
            Choices(questionnaire)[0].IsChecked = true;
            Step(Key.Enter);
            Step(Key.Left);
            Step(Key.Enter, RawInputModifiers.Control);
            Step(Key.Enter, RawInputModifiers.Control);
            window.Close();

            var (free, freeform) = Show("questionnaire-freeform");
            var submitted = 0;
            freeform.Submitted += (_, _) => submitted++;
            var input = All<QuestionnaireInput>(freeform).Single();
            input.Focus();
            Windows.Settle();
            Windows.Type(free, "Another approach");
            Windows.Key(free, Key.Left, RawInputModifiers.None);
            var stayed = (input.IsFocused, input.CaretIndex, submitted);
            Windows.Key(free, Key.Enter, RawInputModifiers.None);
            free.Close();
            return (steps, stayed, submitted);
        });

        Assert.Equal([
            "0 010 1 False", "0 001 2 False", "0 010 1 False", "1 0000 item False", "1 0000 item False", "1 0000 0 False", "1 0000 0 False",
            "2 000 item False", "1 1000 item False", "2 000 item False", "2 000 item True",
        ], found.steps);
        Assert.Equal((true, 15, 0), found.stayed);
        Assert.Equal(1, found.submitted);
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-05")]
    public async Task SubmitChecksEveryQuestionThenGivesTheAnswersAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var command = new Recorder();
            var required = new QuestionnaireItem { Name = "plan", IsRequired = true, Items = { new QuestionnaireChoice { Value = "now", Content = "Now" }, new QuestionnaireInput() } };
            var optional = new QuestionnaireItem { Name = "extras", Items = { new QuestionnaireCheck { Value = "docs", Content = "Docs" } } };
            var questionnaire = new Questionnaire { Width = 400, CurrentIndex = 1, SubmitCommand = command, Items = { required, optional } };
            var submitted = 0;
            questionnaire.Submitted += (_, _) => submitted++;
            var window = Windows.Show(questionnaire, ThemeVariant.Light);
            Press(window, Action(questionnaire, "PART_Submit"));
            var refused = (questionnaire.CurrentIndex, required.IsInvalid, submitted, command.Runs.Count);
            ((QuestionnaireChoice)required.Items[0]!).IsChecked = true;
            ((QuestionnaireInput)required.Items[1]!).Text = "Tomorrow";
            Press(window, Action(questionnaire, "PART_Next"));
            Press(window, Action(questionnaire, "PART_Skip"));
            Windows.Settle();
            Press(window, Action(questionnaire, "PART_Submit"));
            window.Close();
            return (refused, submitted, command.Runs);
        });

        Assert.Equal((0, true, 0, 0), found.refused);
        Assert.Equal(1, found.submitted);
        var answers = Assert.Single(found.Runs);
        Assert.Equal(["plan"], answers.Keys);
        Assert.Equal(["now", "Tomorrow"], answers["plan"]);
    }

    private sealed class Recorder : ICommand
    {
        public event EventHandler? CanExecuteChanged
        {
            add { }
            remove { }
        }

        public List<IReadOnlyDictionary<string, IReadOnlyList<string>>> Runs { get; } = [];

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => Runs.Add((IReadOnlyDictionary<string, IReadOnlyList<string>>)parameter!);
    }
    private static (Window Window, Questionnaire Questionnaire) Tall(string fixture)
    {
        var window = DisplayFixtures.Show(fixture, ThemeVariant.Light);
        window.Height = 1200;
        Windows.Settle();
        return (window, All<Questionnaire>((Control)window.Content!).Single());
    }

    private static List<string> Conditional()
    {
        var (window, questionnaire) = Tall("questionnaire-conditional");
        var states = new List<string> { questionnaire.Progress };
        Press(window, Choices(questionnaire)[1]);
        states.Add(questionnaire.Progress);
        Press(window, Action(questionnaire, "PART_Next"));
        states.Add($"{questionnaire.Progress} {questionnaire.Questions[questionnaire.CurrentIndex].Title}");
        window.Close();
        string[] expected = ["Question 1 of 2", "Question 1 of 3", "Question 2 of 3 Which cloud environment should it use?"];
        return states.SequenceEqual(expected) ? [] : [$"conditional {string.Join(" | ", states)}"];
    }

    private static List<string> Controlled()
    {
        var (window, questionnaire) = Tall("questionnaire-controlled");
        var line = All<TextBlock>((Control)window.Content!).First(static text => text.Text?.StartsWith("Current checkpoint", StringComparison.Ordinal) == true);
        var states = new List<string> { line.Text! };
        Press(window, Choices(questionnaire)[0]);
        Press(window, Action(questionnaire, "PART_Next"));
        states.Add(line.Text!);
        window.Close();
        return states.SequenceEqual(["Current checkpoint: Change scope", "Current checkpoint: Verification"]) ? [] : [$"controlled {string.Join(" | ", states)}"];
    }

    private static List<string> Waits()
    {
        var (window, questionnaire) = Tall("questionnaire-navigation-state");
        var next = Action(questionnaire, "PART_Next");
        var before = next.IsEffectivelyEnabled;
        Press(window, Choices(questionnaire)[0]);
        var after = next.IsEffectivelyEnabled;
        window.Close();
        return (before, after) == (false, true) ? [] : [$"navigation state: Next enabled {before} then {after}"];
    }

    private static List<string> Segments()
    {
        var (window, questionnaire) = Tall("questionnaire-progress");
        List<Border> Bars() => [.. All<Border>(questionnaire).Where(static bar => bar.Classes.Contains("questionnaire-segment"))];
        var primary = Windows.Paint((IBrush)window.FindResource(ThemeVariant.Light, "CosmoPrimaryBrush")!);
        string Lit() => string.Concat(Bars().Select(bar => Windows.Paint(bar.Background) == primary ? "1" : "0"));
        var states = new List<string> { $"{questionnaire.Progress} {Lit()}" };
        Press(window, Choices(questionnaire)[0]);
        Press(window, Action(questionnaire, "PART_Next"));
        states.Add($"{questionnaire.Progress} {Lit()}");
        window.Close();
        return states.SequenceEqual(["Checkpoint 1 of 4 1000", "Checkpoint 2 of 4 1100"]) ? [] : [$"segments {string.Join(" | ", states)}"];
    }

    private static List<string> Resumes()
    {
        var (window, questionnaire) = Tall("questionnaire-resume");
        string Checked() => string.Concat(Choices(questionnaire).Select(static choice => choice.IsChecked == true ? "1" : "0"));
        var states = new List<string> { $"{questionnaire.Progress} {Checked()}" };
        Press(window, Choices(questionnaire)[2]);
        Press(window, Choices(questionnaire)[0]);
        states.Add(Checked());
        Press(window, Action(questionnaire, "PART_Reset"));
        states.Add(Checked());
        window.Close();
        return states.SequenceEqual(["Question 2 of 3 110", "011", "110"]) ? [] : [$"resume {string.Join(" | ", states)}"];
    }

    private static List<string> ShortcutStyles()
    {
        var (window, questionnaire) = Tall("questionnaire-shortcuts");
        var select = All<ComboBox>((Control)window.Content!).Single();
        string Keys() => string.Join(',', Choices(questionnaire).Select(static choice => choice.GetValue(QuestionnaireChoice.ShortcutProperty) ?? "-"));
        var states = new List<string> { Keys() };
        select.SelectedIndex = 2;
        Windows.Settle();
        states.Add(Keys());
        select.SelectedIndex = 0;
        Windows.Settle();
        states.Add(Keys());
        window.Close();
        return states.SequenceEqual(["A,B,C", "1,2,3", "-,-,-"]) ? [] : [$"shortcuts {string.Join(" | ", states)}"];
    }

    private static List<string> Validates()
    {
        var (window, questionnaire) = Tall("questionnaire-validation");
        Press(window, Choices(questionnaire)[0]);
        Press(window, Action(questionnaire, "PART_Next"));
        Press(window, Choices(questionnaire)[1]);
        Press(window, Action(questionnaire, "PART_Submit"));
        var refused = State(questionnaire);
        Press(window, Choices(questionnaire)[1]);
        var cleared = State(questionnaire);
        window.Close();
        return refused == (0, "1 / 2", true, "Public answers need enough context. Choose a complete answer.", false, false, true, false) && !cleared.Invalid
            ? []
            : [$"validation {refused} then {cleared}"];
    }

    private static List<string> Animates()
    {
        Windows.Holding = false;
        Windows.Clocked = true;
        try
        {
            var (window, questionnaire) = Tall("questionnaire-animated");
            Press(window, Choices(questionnaire)[0]);
            Press(window, Action(questionnaire, "PART_Next"));
            var shown = questionnaire.Questions[1];
            Windows.Clock!.Pulse(TimeSpan.Zero);
            var start = shown.Opacity;
            Windows.Clock.Pulse(TimeSpan.FromMilliseconds(300));
            var rest = shown.Opacity;
            window.Close();
            return start < 0.5 && rest == 1 ? [] : [$"animated: opacity {start} then {rest}"];
        }
        finally
        {
            Windows.Holding = true;
            Windows.Clocked = false;
        }
    }

    [Fact]
    [Trait("AC", "QUESTIONNAIRE-07")]
    public async Task TheDocsQuestionnairesBehaveAsShadcnsAsync()
    {
        var wrong = await desktop.RunAsync(() => Conditional().Concat(Controlled()).Concat(Waits()).Concat(Segments()).Concat(Resumes())
            .Concat(ShortcutStyles()).Concat(Validates()).Concat(Animates()).ToList());

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
