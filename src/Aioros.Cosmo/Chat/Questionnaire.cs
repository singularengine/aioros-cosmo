using System.Globalization;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Chat;

/// <summary>The keys a <see cref="Questionnaire"/> gives its choices, shadcn/ui's <c>shortcuts</c>.</summary>
public enum QuestionnaireShortcuts
{
    /// <summary>No shortcut.</summary>
    None,

    /// <summary>A to Z, in order.</summary>
    Letters,

    /// <summary>1 to 9, in order.</summary>
    Numbers,
}

/// <summary>Where a <see cref="QuestionnaireItem"/> is, shadcn/ui's <c>data-status</c>.</summary>
public enum QuestionnaireStatus
{
    /// <summary>No answer yet.</summary>
    Unanswered,

    /// <summary>A choice is checked or the freeform input holds text.</summary>
    Answered,

    /// <summary>Left unanswered on purpose with Skip.</summary>
    Skipped,
}

/// <summary>
/// shadcn/ui's Questionnaire (the radix base in the radix-rhea style, on <c>@shadcn/react</c>'s primitive): its
/// <see cref="QuestionnaireItem"/>s one at a time (<see cref="CurrentIndex"/>), with their progress and the Previous,
/// Skip, Next and Submit actions that apply; Next and Submit check the questions first, and a valid Submit raises
/// <see cref="Submitted"/> and runs <see cref="SubmitCommand"/> with the answers. Its keys are shadcn/ui's: Ctrl or Meta
/// and Enter moves on (submits on the last question), as Enter does on a checked choice or a filled freeform answer;
/// ArrowLeft moves back and ArrowRight on once the question is answered, outside a text field; the arrows move the focus
/// between the answers, from an empty freeform answer or the question, and between the radio buttons from one, checking
/// it. A move focuses the question shown while the focus was inside, and with the <c>animated</c> class the question
/// shown enters from 8 px below at opacity 0 (not when the page opens); on the web the actions answer the same keys
/// (Aioros's <c>a:Web.KeyScope</c> and <c>a:Web.Keys</c>).
/// </summary>
[TemplatePart("PART_Previous", typeof(Button))]
[TemplatePart("PART_Skip", typeof(Button))]
[TemplatePart("PART_Next", typeof(Button))]
[TemplatePart("PART_Submit", typeof(Button))]
[TemplatePart("PART_Reset", typeof(Button))]
[TemplatePart("PART_Segments", typeof(Panel))]
[PseudoClasses(":first", ":last", ":optional", ":waiting")]
public sealed class Questionnaire : ItemsControl
{
    /// <summary>Defines the <see cref="CurrentIndex"/> property.</summary>
    public static readonly StyledProperty<int> CurrentIndexProperty =
        AvaloniaProperty.Register<Questionnaire, int>(nameof(CurrentIndex), defaultBindingMode: Avalonia.Data.BindingMode.TwoWay);

    /// <summary>Defines the <see cref="Shortcuts"/> property.</summary>
    public static readonly StyledProperty<QuestionnaireShortcuts> ShortcutsProperty = AvaloniaProperty.Register<Questionnaire, QuestionnaireShortcuts>(nameof(Shortcuts));

    /// <summary>Defines the <see cref="ShowProgress"/> property.</summary>
    public static readonly StyledProperty<bool> ShowProgressProperty = AvaloniaProperty.Register<Questionnaire, bool>(nameof(ShowProgress));

    /// <summary>Defines the <see cref="Progress"/> property.</summary>
    public static readonly DirectProperty<Questionnaire, string> ProgressProperty =
        AvaloniaProperty.RegisterDirect<Questionnaire, string>(nameof(Progress), static questionnaire => questionnaire.Progress);

    /// <summary>Defines the <see cref="ProgressFormat"/> property.</summary>
    public static readonly StyledProperty<string> ProgressFormatProperty = AvaloniaProperty.Register<Questionnaire, string>(nameof(ProgressFormat), "Question {0} of {1}");

    /// <summary>Defines the <see cref="Segments"/> property.</summary>
    public static readonly DirectProperty<Questionnaire, IReadOnlyList<bool>> SegmentsProperty =
        AvaloniaProperty.RegisterDirect<Questionnaire, IReadOnlyList<bool>>(nameof(Segments), static questionnaire => questionnaire.Segments);

    /// <summary>Defines the <see cref="RequiresAnswer"/> property.</summary>
    public static readonly StyledProperty<bool> RequiresAnswerProperty = AvaloniaProperty.Register<Questionnaire, bool>(nameof(RequiresAnswer));

    /// <summary>Defines the <see cref="ResetContent"/> property.</summary>
    public static readonly StyledProperty<object?> ResetContentProperty = AvaloniaProperty.Register<Questionnaire, object?>(nameof(ResetContent));

    /// <summary>Defines the <see cref="PreviousContent"/> property.</summary>
    public static readonly StyledProperty<object?> PreviousContentProperty = AvaloniaProperty.Register<Questionnaire, object?>(nameof(PreviousContent), "Previous");

    /// <summary>Defines the <see cref="SkipContent"/> property.</summary>
    public static readonly StyledProperty<object?> SkipContentProperty = AvaloniaProperty.Register<Questionnaire, object?>(nameof(SkipContent), "Skip");

    /// <summary>Defines the <see cref="NextContent"/> property.</summary>
    public static readonly StyledProperty<object?> NextContentProperty = AvaloniaProperty.Register<Questionnaire, object?>(nameof(NextContent), "Next");

    /// <summary>Defines the <see cref="SubmitContent"/> property.</summary>
    public static readonly StyledProperty<object?> SubmitContentProperty = AvaloniaProperty.Register<Questionnaire, object?>(nameof(SubmitContent), "Submit");

    /// <summary>Defines the <see cref="SubmitCommand"/> property.</summary>
    public static readonly StyledProperty<ICommand?> SubmitCommandProperty = AvaloniaProperty.Register<Questionnaire, ICommand?>(nameof(SubmitCommand));

    /// <summary>Defines the <see cref="Submitted"/> event.</summary>
    public static readonly RoutedEvent<RoutedEventArgs> SubmittedEvent = RoutedEvent.Register<Questionnaire, RoutedEventArgs>(nameof(Submitted), RoutingStrategies.Bubble);

    private string progress = string.Empty;
    private IReadOnlyList<bool> segments = [];
    private List<(ToggleButton Choice, bool? Checked)>? savedChoices;
    private List<(TextBox Input, string? Text)>? savedInputs;
    private Panel? segmentBars;

    static Questionnaire()
    {
        IsEnabledProperty.Changed.AddClassHandler<QuestionnaireItem>((item, _) => (item.Parent as Questionnaire)?.Show());
        KeyDownEvent.AddClassHandler<Questionnaire>((questionnaire, e) => questionnaire.Keys(e), RoutingStrategies.Tunnel);
    }

    /// <summary>Raised when every question is valid and Submit is clicked on the last one.</summary>
    public event EventHandler<RoutedEventArgs>? Submitted
    {
        add => AddHandler(SubmittedEvent, value);
        remove => RemoveHandler(SubmittedEvent, value);
    }

    /// <summary>The position of the question shown, from 0.</summary>
    public int CurrentIndex
    {
        get => GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    /// <summary>The keys given to the shown question's choices.</summary>
    public QuestionnaireShortcuts Shortcuts
    {
        get => GetValue(ShortcutsProperty);
        set => SetValue(ShortcutsProperty, value);
    }

    /// <summary>Whether the progress (<c>Question 1 of 3</c>) is shown.</summary>
    public bool ShowProgress
    {
        get => GetValue(ShowProgressProperty);
        set => SetValue(ShowProgressProperty, value);
    }

    /// <summary>The progress text, <c>Question {current} of {total}</c>.</summary>
    public string Progress
    {
        get => progress;
        private set => SetAndRaise(ProgressProperty, ref progress, value);
    }

    /// <summary>The progress's text, its <c>{0}</c> the shown question's position among the questions that apply and its <c>{1}</c> their count (shadcn/ui's <c>QuestionnaireProgress</c> render: <c>Checkpoint {0} of {1}</c>, <c>{0} / {1}</c>).</summary>
    public string ProgressFormat
    {
        get => GetValue(ProgressFormatProperty);
        set => SetValue(ProgressFormatProperty, value);
    }

    /// <summary>One flag per question that applies, set up to the shown one: the bars of a progress drawn in segments (the <c>segmented</c> class).</summary>
    public IReadOnlyList<bool> Segments
    {
        get => segments;
        private set => SetAndRaise(SegmentsProperty, ref segments, value);
    }

    /// <summary>Whether Next and Submit stay disabled until the shown question is answered (shadcn/ui's <c>disabled</c> Next of an unanswered item).</summary>
    public bool RequiresAnswer
    {
        get => GetValue(RequiresAnswerProperty);
        set => SetValue(RequiresAnswerProperty, value);
    }

    /// <summary>The content of a button that restores the answers the questionnaire opened with (a <c>type="reset"</c> button); none shows without it.</summary>
    public object? ResetContent
    {
        get => GetValue(ResetContentProperty);
        set => SetValue(ResetContentProperty, value);
    }

    /// <summary>The Previous button's content.</summary>
    public object? PreviousContent
    {
        get => GetValue(PreviousContentProperty);
        set => SetValue(PreviousContentProperty, value);
    }

    /// <summary>The Skip button's content.</summary>
    public object? SkipContent
    {
        get => GetValue(SkipContentProperty);
        set => SetValue(SkipContentProperty, value);
    }

    /// <summary>The Next button's content.</summary>
    public object? NextContent
    {
        get => GetValue(NextContentProperty);
        set => SetValue(NextContentProperty, value);
    }

    /// <summary>The Submit button's content.</summary>
    public object? SubmitContent
    {
        get => GetValue(SubmitContentProperty);
        set => SetValue(SubmitContentProperty, value);
    }

    /// <summary>Run on a valid Submit with the answers by question name (<see cref="Answers"/>).</summary>
    public ICommand? SubmitCommand
    {
        get => GetValue(SubmitCommandProperty);
        set => SetValue(SubmitCommandProperty, value);
    }

    /// <summary>The questions, in order.</summary>
    public IReadOnlyList<QuestionnaireItem> Questions => [.. Items.OfType<QuestionnaireItem>().Where(static item => item.IsEnabled)];

    /// <summary>The answers by question name: the checked choices' values and the freeform text; a skipped question has none.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Answers =>
        Questions.Where(static item => item.Status != QuestionnaireStatus.Skipped).ToDictionary(static item => item.Name ?? string.Empty, static item => item.Answers, StringComparer.Ordinal);

    /// <summary>Moves to the previous question.</summary>
    public void Previous() => CurrentIndex = Math.Max(0, CurrentIndex - 1);

    /// <summary>Leaves the shown question unanswered on purpose and moves on, when it is optional.</summary>
    public void Skip()
    {
        if (Current is { IsRequired: false } item)
        {
            item.MarkSkipped();
            CurrentIndex = Math.Min(Questions.Count - 1, CurrentIndex + 1);
        }
    }

    /// <summary>Moves to the next question once the shown one is answered or skipped; marks it invalid otherwise.</summary>
    public void Next()
    {
        if (Current is { } item && item.Validate())
        {
            CurrentIndex = Math.Min(Questions.Count - 1, CurrentIndex + 1);
        }
    }

    /// <summary>Restores the answers the questionnaire opened with and shows the question it opened on.</summary>
    public void Reset()
    {
        foreach (var (choice, isChecked) in savedChoices ?? [])
        {
            choice.IsChecked = isChecked;
        }

        foreach (var (input, text) in savedInputs ?? [])
        {
            input.Text = text;
        }

        foreach (var item in Items.OfType<QuestionnaireItem>())
        {
            item.IsInvalid = false;
            item.Message = null;
        }
    }

    /// <summary>Submits when every question is answered or skipped; shows the first one that is not otherwise. The app checks the answers in <see cref="SubmitCommand"/> and gives a question its <see cref="QuestionnaireItem.Message"/> and <see cref="CurrentIndex"/> its position when they fail.</summary>
    public void Submit()
    {
        var questions = Questions;
        if (questions.FirstOrDefault(static item => !item.Validate()) is { } invalid)
        {
            CurrentIndex = IndexOf(questions, invalid);
            return;
        }


        var answers = Answers;
        RaiseEvent(new RoutedEventArgs(SubmittedEvent, this));
        if (SubmitCommand is { } command && command.CanExecute(answers))
        {
            command.Execute(answers);
        }
    }

    /// <summary>The questions are their own containers.</summary>
    /// <param name="item">The item.</param>
    /// <param name="index">Its position.</param>
    /// <param name="recycleKey">The recycle key.</param>
    /// <returns>Whether the item needs a container.</returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) => NeedsContainer<Control>(item, out recycleKey);

    /// <summary>Takes the actions of the template.</summary>
    /// <param name="e">The applied template.</param>
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Wire(e, "PART_Previous", Previous);
        Wire(e, "PART_Skip", Skip);
        Wire(e, "PART_Next", Next);
        Wire(e, "PART_Submit", Submit);
        Wire(e, "PART_Reset", Reset);
        segmentBars = e.NameScope.Find<Panel>("PART_Segments");
        Save();
        Show();
    }

    private void Keys(KeyEventArgs e)
    {
        if (!e.Handled && Current is { } item && e.Source is Visual source)
        {
            e.Handled = Answer(e, item, source);
        }
    }

    private bool Answer(KeyEventArgs e, QuestionnaireItem item, Visual source)
    {
        List<Control> answers = [.. item.Items.OfType<Control>().Where(static answer => answer is ToggleButton or TextBox && answer.IsEffectivelyEnabled && answer.IsEffectivelyVisible)];
        var answer = answers.FirstOrDefault(candidate => ReferenceEquals(candidate, source) || candidate.IsVisualAncestorOf(source));
        var typing = answer is TextBox;
        var plain = e.KeyModifiers == KeyModifiers.None;
        return e.Key switch
        {
            Key.Enter when e.KeyModifiers is KeyModifiers.Control or KeyModifiers.Meta => Advance(),
            Key.Enter when plain && answer is not null => !Answered(answer) || Advance(),
            Key.Up or Key.Down when plain && (answer is not null || ReferenceEquals(source, item)) => Move(answers, answer, e.Key == Key.Down),
            Key.Left or Key.Right when plain && answer is RadioButton => Move(answers, answer, e.Key == Key.Right),
            Key.Left when plain && !typing => Back(),
            Key.Right when plain && !typing => item.Status == QuestionnaireStatus.Unanswered || Advance(),
            _ => false,
        };
    }

    /// <summary>Shows the current question as its index or the shortcuts change.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == CurrentIndexProperty || change.Property == ShortcutsProperty || change.Property == ProgressFormatProperty || change.Property == RequiresAnswerProperty)
        {
            var focused = IsKeyboardFocusWithin;
            Show(entering: change.Property == CurrentIndexProperty);
            if (change.Property == CurrentIndexProperty && focused)
            {
                Current?.Focus(NavigationMethod.Directional);
            }
        }
    }

    private QuestionnaireItem? Current => Questions.ElementAtOrDefault(CurrentIndex);

    private void Bars()
    {
        if (segmentBars is not { } bars)
        {
            return;
        }

        while (bars.Children.Count > Segments.Count)
        {
            bars.Children.RemoveAt(bars.Children.Count - 1);
        }

        while (bars.Children.Count < Segments.Count)
        {
            bars.Children.Add(new Border { Height = 6, CornerRadius = new CornerRadius(3), Classes = { "questionnaire-segment" } });
        }

        for (var index = 0; index < Segments.Count; index++)
        {
            bars.Children[index].Classes.Set("done", Segments[index]);
        }
    }

    private static bool Answered(Control answer) => answer is ToggleButton { IsChecked: true } || answer is TextBox { Text: { } text } && !string.IsNullOrWhiteSpace(text);

    private static bool Move(List<Control> answers, Control? from, bool forward)
    {
        if (from is TextBox && Answered(from))
        {
            return false;
        }

        var pool = from is RadioButton ? answers.OfType<RadioButton>().ToList<Control>() : answers;
        var index = from is null ? -1 : pool.IndexOf(from);
        if (pool.Count == 0 || (index >= 0 && pool.Count == 1))
        {
            return false;
        }

        var target = index < 0 ? pool.FirstOrDefault(Answered) ?? (forward ? pool[0] : pool[^1]) : pool[(index + (forward ? 1 : -1) + pool.Count) % pool.Count];
        target.Focus(NavigationMethod.Directional);
        if (target is RadioButton radio)
        {
            radio.IsChecked = true;
        }

        return true;
    }

    private bool Advance()
    {
        if (CurrentIndex >= Questions.Count - 1)
        {
            Submit();
        }
        else
        {
            Next();
        }

        return true;
    }

    private bool Back()
    {
        Previous();
        return true;
    }

    private static int IndexOf(IReadOnlyList<QuestionnaireItem> questions, QuestionnaireItem item)
    {
        for (var index = 0; index < questions.Count; index++)
        {
            if (ReferenceEquals(questions[index], item))
            {
                return index;
            }
        }

        return 0;
    }

    private static void Wire(TemplateAppliedEventArgs e, string name, Action action)
    {
        if (e.NameScope.Find<Button>(name) is { } button)
        {
            button.Click += (_, _) => action();
        }
    }

    private void Save()
    {
        savedChoices ??= [.. Items.OfType<QuestionnaireItem>().SelectMany(static item => item.Items.OfType<ToggleButton>()).Select(static choice => (choice, choice.IsChecked))];
        savedInputs ??= [.. Items.OfType<QuestionnaireItem>().SelectMany(static item => item.Items.OfType<TextBox>()).Select(static input => (input, input.Text))];
    }

    /// <summary>Follows the shown question's answer for <see cref="RequiresAnswer"/>.</summary>
    internal void Answered() => Show();

    internal void Show() => Show(entering: false);

    private void Show(bool entering)
    {
        var questions = Questions;
        foreach (var hidden in Items.OfType<QuestionnaireItem>().Where(static item => !item.IsEnabled))
        {
            hidden.IsVisible = false;
        }

        for (var index = 0; index < questions.Count; index++)
        {
            var shown = index == CurrentIndex;
            if (entering && shown && !questions[index].IsVisible && Classes.Contains("animated"))
            {
                questions[index].IsVisible = true;
                Overlays.Motion.SetEnterOpacity(questions[index], 0);
                Overlays.Motion.SetEnterOffset(questions[index], new RelativePoint(0, 8, RelativeUnit.Absolute));
                Overlays.Motion.Enter(questions[index]);
            }

            questions[index].IsVisible = shown;
            questions[index].Assign(Shortcuts, shown);
        }

        Progress = string.Format(CultureInfo.InvariantCulture, ProgressFormat, CurrentIndex + 1, questions.Count);
        foreach (var question in questions)
        {
            question.Progress = Progress;
        }
        Segments = [.. Enumerable.Range(0, questions.Count).Select(index => index <= CurrentIndex)];
        Bars();
        PseudoClasses.Set(":waiting", RequiresAnswer && Current is { Status: QuestionnaireStatus.Unanswered });
        PseudoClasses.Set(":first", CurrentIndex == 0);
        PseudoClasses.Set(":last", CurrentIndex >= questions.Count - 1);
        PseudoClasses.Set(":optional", Current is { IsRequired: false });
    }
}

/// <summary>
/// shadcn/ui's QuestionnaireItem: a question's <see cref="Title"/>, <see cref="Description"/> and answers (its
/// <see cref="QuestionnaireChoice"/>s, <see cref="QuestionnaireCheck"/>s and <see cref="QuestionnaireInput"/>), its
/// <see cref="Status"/>, and its error while <see cref="IsInvalid"/>; its <c>Name</c> names its answers.
/// </summary>
[PseudoClasses(":invalid")]
public sealed class QuestionnaireItem : ItemsControl
{
    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<QuestionnaireItem, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<QuestionnaireItem, object?>(nameof(Description));

    /// <summary>Defines the <see cref="IsRequired"/> property.</summary>
    public static readonly StyledProperty<bool> IsRequiredProperty = AvaloniaProperty.Register<QuestionnaireItem, bool>(nameof(IsRequired));

    /// <summary>Defines the <see cref="Status"/> property.</summary>
    public static readonly StyledProperty<QuestionnaireStatus> StatusProperty = AvaloniaProperty.Register<QuestionnaireItem, QuestionnaireStatus>(nameof(Status));

    /// <summary>Defines the <see cref="IsInvalid"/> property.</summary>
    public static readonly StyledProperty<bool> IsInvalidProperty = AvaloniaProperty.Register<QuestionnaireItem, bool>(nameof(IsInvalid));

    /// <summary>Defines the <see cref="Message"/> property.</summary>
    public static readonly StyledProperty<string?> MessageProperty = AvaloniaProperty.Register<QuestionnaireItem, string?>(nameof(Message));

    /// <summary>Defines the <see cref="Error"/> property.</summary>
    public static readonly DirectProperty<QuestionnaireItem, string> ErrorProperty =
        AvaloniaProperty.RegisterDirect<QuestionnaireItem, string>(nameof(Error), static item => item.Error);

    /// <summary>Defines the <see cref="Progress"/> property.</summary>
    public static readonly DirectProperty<QuestionnaireItem, string> ProgressProperty =
        AvaloniaProperty.RegisterDirect<QuestionnaireItem, string>(nameof(Progress), static item => item.Progress);

    private string error = "Choose an answer or skip this question.";
    private string progress = string.Empty;
    private QuestionnaireShortcuts shortcuts;
    private bool shown;
    private bool typing;

    /// <summary>Follows its answers, and lets the freeform input take the letters while it has the focus.</summary>
    public QuestionnaireItem()
    {
        AddHandler(ToggleButton.IsCheckedChangedEvent, (_, _) => Follow());
        AddHandler(TextBox.TextChangedEvent, (_, _) => Follow());
        AddHandler(GotFocusEvent, (_, e) => Type(e.Source is QuestionnaireInput));
        AddHandler(LostFocusEvent, (_, _) => Type(false));
    }

    /// <summary>The question.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The line below the question.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Whether the question needs an answer; an optional one can be skipped.</summary>
    public bool IsRequired
    {
        get => GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    /// <summary>Whether the question is answered, skipped or neither.</summary>
    public QuestionnaireStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>Whether the question failed its check; its error shows.</summary>
    public bool IsInvalid
    {
        get => GetValue(IsInvalidProperty);
        set => SetValue(IsInvalidProperty, value);
    }

    /// <summary>The app's own error for the question (shadcn/ui's <c>QuestionnaireError</c> children); setting it marks the question invalid, and a new answer clears it.</summary>
    public string? Message
    {
        get => GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    /// <summary>The error shown while the question is invalid.</summary>
    public string Error
    {
        get => error;
        private set => SetAndRaise(ErrorProperty, ref error, value);
    }

    /// <summary>The checked choices' values and the freeform text.</summary>
    public IReadOnlyList<string> Answers =>
    [
        .. Items.OfType<ToggleButton>().Where(static choice => choice.IsChecked == true).Select(static choice => (choice as QuestionnaireChoice)?.Value ?? (choice as QuestionnaireCheck)?.Value ?? string.Empty),
        .. Items.OfType<QuestionnaireInput>().Select(static input => input.Text ?? string.Empty).Where(static text => text.Length > 0),
    ];

    /// <summary>The answers are their own containers.</summary>
    /// <param name="item">The item.</param>
    /// <param name="index">Its position.</param>
    /// <param name="recycleKey">The recycle key.</param>
    /// <returns>Whether the item needs a container.</returns>
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) => NeedsContainer<Control>(item, out recycleKey);

    /// <summary>Marks the question invalid and its error as its requirement changes.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == IsInvalidProperty)
        {
            PseudoClasses.Set(":invalid", IsInvalid);
        }
        else if (change.Property == IsRequiredProperty || change.Property == MessageProperty)
        {
            Error = Message ?? (IsRequired ? "Choose an answer to continue." : "Choose an answer or skip this question.");
            IsInvalid = IsInvalid || Message is not null;
        }
        else if (change.Property == StatusProperty)
        {
            (Parent as Questionnaire)?.Answered();
        }
    }

    /// <summary>Whether the question is answered, or skipped when optional; marks it invalid otherwise.</summary>
    /// <returns>Whether the question is valid.</returns>
    internal bool Validate()
    {
        var valid = Status == QuestionnaireStatus.Answered || (!IsRequired && Status == QuestionnaireStatus.Skipped);
        IsInvalid = !valid;
        return valid;
    }

    /// <summary>Leaves the question unanswered on purpose.</summary>
    internal void MarkSkipped()
    {
        foreach (var choice in Items.OfType<ToggleButton>())
        {
            choice.IsChecked = false;
        }

        Status = QuestionnaireStatus.Skipped;
        IsInvalid = false;
    }

    /// <summary>Its questionnaire's progress text, which a card layout shows in the question's header.</summary>
    public string Progress
    {
        get => progress;
        internal set => SetAndRaise(ProgressProperty, ref progress, value);
    }

    /// <summary>Gives the choices their keys, as hot keys only while the question is shown.</summary>
    /// <param name="shortcuts">The kind of keys.</param>
    /// <param name="shown">Whether the question is shown.</param>
    internal void Assign(QuestionnaireShortcuts shortcuts, bool shown)
    {
        this.shortcuts = shortcuts;
        this.shown = shown;
        var position = 0;
        foreach (var choice in Items.OfType<ToggleButton>())
        {
            var (text, key) = Key(shortcuts, position++);
            choice.SetValue(QuestionnaireChoice.ShortcutProperty, text);
            choice.HotKey = shown && !typing && key is { } hot ? new KeyGesture(hot) : null;
        }
    }

    private static (string? Text, Key? Key) Key(QuestionnaireShortcuts shortcuts, int position) => shortcuts switch
    {
        QuestionnaireShortcuts.Letters when position < 26 => (((char)('A' + position)).ToString(), Avalonia.Input.Key.A + position),
        QuestionnaireShortcuts.Numbers when position < 9 => ((position + 1).ToString(CultureInfo.InvariantCulture), Avalonia.Input.Key.D1 + position),
        _ => (null, null),
    };

    private void Type(bool focused)
    {
        typing = focused;
        Assign(shortcuts, shown);
    }

    private void Follow()
    {
        var answered = Answers.Count > 0;
        if (answered)
        {
            Status = QuestionnaireStatus.Answered;
            Message = null;
            IsInvalid = false;
        }
        else if (Status == QuestionnaireStatus.Answered)
        {
            Status = QuestionnaireStatus.Unanswered;
        }
    }
}

/// <summary>
/// shadcn/ui's QuestionnaireChoice for one answer: a radio button drawn as an 18 px rounded card of its 16 px indicator,
/// its label (the content) over its <see cref="Description"/>, and its <see cref="Shortcut"/>; its <see cref="Value"/>
/// is the answer.
/// </summary>
public sealed class QuestionnaireChoice : RadioButton
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<QuestionnaireChoice, string?>(nameof(Value));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty = AvaloniaProperty.Register<QuestionnaireChoice, string?>(nameof(Description));

    /// <summary>Defines the <see cref="Shortcut"/> property, shared with <see cref="QuestionnaireCheck"/>.</summary>
    public static readonly AttachedProperty<string?> ShortcutProperty = AvaloniaProperty.RegisterAttached<QuestionnaireChoice, ToggleButton, string?>("Shortcut");

    /// <summary>The answer this choice gives.</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The muted line below the label.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The key shown at the card's end, given by the questionnaire.</summary>
    public string? Shortcut => GetValue(ShortcutProperty);
}

/// <summary>shadcn/ui's QuestionnaireChoice for several answers: a check box drawn as the choice's card, its indicator a rounded square.</summary>
public sealed class QuestionnaireCheck : CheckBox
{
    /// <summary>Defines the <see cref="Value"/> property.</summary>
    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<QuestionnaireCheck, string?>(nameof(Value));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<string?> DescriptionProperty = AvaloniaProperty.Register<QuestionnaireCheck, string?>(nameof(Description));

    /// <summary>The answer this choice gives.</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>The muted line below the label.</summary>
    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Defines the <see cref="Shortcut"/> property, the choice's one.</summary>
    public static readonly AttachedProperty<string?> ShortcutProperty = QuestionnaireChoice.ShortcutProperty.AddOwner<QuestionnaireCheck>();

    /// <summary>The key shown at the card's end, given by the questionnaire.</summary>
    public string? Shortcut => GetValue(ShortcutProperty);
}

/// <summary>shadcn/ui's QuestionnaireInput: a freeform answer, a filled 32 px field; while it has the focus, letters type in it rather than check the choices.</summary>
public sealed class QuestionnaireInput : TextBox
{
}
