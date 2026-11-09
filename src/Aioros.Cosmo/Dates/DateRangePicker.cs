using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Calendar = Avalonia.Controls.Calendar;

namespace Aioros.Cosmo.Dates;

/// <summary>
/// shadcn/ui's date range picker (<c>date-picker-with-range</c>): an outline button with the calendar icon and the range
/// (<c>Jan 20, 2022 - Feb 09, 2022</c>, as date-fns's <c>LLL dd, y</c>) whose popover shows two months side by side
/// (react-day-picker's <c>mode="range" numberOfMonths={2}</c>): the previous button on the first, the next on the second,
/// moving both, and a click adding the day to the range as react-day-picker's <c>addToRange</c> does.
/// <see cref="RangeStartProperty"/> and <see cref="RangeEndProperty"/> bind two way.
/// </summary>
[TemplatePart("PART_First", typeof(Calendar))]
[TemplatePart("PART_Second", typeof(Calendar))]
public sealed class DateRangePicker : TemplatedControl
{
    /// <summary>Defines the first day of the range.</summary>
    public static readonly StyledProperty<DateTime?> RangeStartProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(RangeStart), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the last day of the range.</summary>
    public static readonly StyledProperty<DateTime?> RangeEndProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(RangeEnd), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the first month the popover shows; the range's first day's when unset.</summary>
    public static readonly StyledProperty<DateTime?> DisplayDateProperty =
        AvaloniaProperty.Register<DateRangePicker, DateTime?>(nameof(DisplayDate));

    /// <summary>Defines the text shown without a range.</summary>
    public static readonly StyledProperty<string> PlaceholderTextProperty =
        AvaloniaProperty.Register<DateRangePicker, string>(nameof(PlaceholderText), "Pick a date");

    /// <summary>Defines the button's text: the range, or the placeholder.</summary>
    public static readonly DirectProperty<DateRangePicker, string> TextProperty =
        AvaloniaProperty.RegisterDirect<DateRangePicker, string>(nameof(Text), picker => picker.Text);

    /// <summary>Defines whether no range is chosen, the placeholder showing.</summary>
    public static readonly DirectProperty<DateRangePicker, bool> IsEmptyProperty =
        AvaloniaProperty.RegisterDirect<DateRangePicker, bool>(nameof(IsEmpty), picker => picker.IsEmpty);

    private Calendar? _first;
    private Calendar? _second;
    private bool _moving;
    private string _text = string.Empty;
    private bool _empty = true;
    private CalendarDayButton? _pressed;
    private bool _selecting;

    static DateRangePicker()
    {
        RangeStartProperty.Changed.AddClassHandler<DateRangePicker>((picker, _) => picker.Update());
        RangeEndProperty.Changed.AddClassHandler<DateRangePicker>((picker, _) => picker.Update());
        PlaceholderTextProperty.Changed.AddClassHandler<DateRangePicker>((picker, _) => picker.Update());
    }

    /// <summary>Creates a picker without a range.</summary>
    public DateRangePicker() => Update();

    /// <summary>The first day of the range.</summary>
    public DateTime? RangeStart
    {
        get => GetValue(RangeStartProperty);
        set => SetValue(RangeStartProperty, value);
    }

    /// <summary>The last day of the range.</summary>
    public DateTime? RangeEnd
    {
        get => GetValue(RangeEndProperty);
        set => SetValue(RangeEndProperty, value);
    }

    /// <summary>The first month the popover shows; the range's first day's when unset.</summary>
    public DateTime? DisplayDate
    {
        get => GetValue(DisplayDateProperty);
        set => SetValue(DisplayDateProperty, value);
    }

    /// <summary>The text shown without a range.</summary>
    public string PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>The button's text: the range, or the placeholder.</summary>
    public string Text
    {
        get => _text;
        private set => SetAndRaise(TextProperty, ref _text, value);
    }

    /// <summary>Whether no range is chosen, the placeholder showing.</summary>
    public bool IsEmpty
    {
        get => _empty;
        private set => SetAndRaise(IsEmptyProperty, ref _empty, value);
    }

    /// <summary>
    /// The range after a click on <paramref name="date"/>, as react-day-picker's <c>addToRange</c> makes it (no minimum or
    /// maximum, not required): an empty range becomes that day, an open one closes on it, a closed one moves its nearer end.
    /// </summary>
    public static (DateTime? Start, DateTime? End) Add(DateTime date, DateTime? start, DateTime? end)
    {
        date = date.Date;
        return (start, end) switch
        {
            (null, null) => (date, date),
            ({ } from, null) when from.Date == date => (null, null),
            ({ } from, null) => date < from ? (date, from) : (from, date),
            ({ } from, { } to) when from.Date == date && to.Date == date => (null, null),
            ({ } from, _) when from.Date == date => (from, date),
            (_, { } to) when to.Date == date => (date, date),
            ({ } from, { } to) when date < from => (date, to),
            ({ } from, _) => (from, date),
            _ => (date, date),
        };
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        foreach (var calendar in new[] { _first, _second }.OfType<Calendar>())
        {
            calendar.DisplayDateChanged -= Moved;
            calendar.SelectedDatesChanged -= Restored;
            calendar.RemoveHandler(PointerPressedEvent, Pressed);
            calendar.RemoveHandler(PointerReleasedEvent, Released);
            calendar.RemoveHandler(KeyDownEvent, Keyed);
        }

        _first = e.NameScope.Find<Calendar>("PART_First");
        _second = e.NameScope.Find<Calendar>("PART_Second");
        foreach (var calendar in new[] { _first, _second }.OfType<Calendar>())
        {
            calendar.SelectionMode = CalendarSelectionMode.SingleRange;
            calendar.AllowTapRangeSelection = false;
            calendar.DisplayDateChanged += Moved;
            calendar.SelectedDatesChanged += Restored;
            calendar.AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
            calendar.AddHandler(PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
            calendar.AddHandler(KeyDownEvent, Keyed, RoutingStrategies.Tunnel);
        }

        var shown = DisplayDate ?? RangeStart ?? DateTime.Today;
        Show(new DateTime(shown.Year, shown.Month, 1, 0, 0, 0, shown.Kind));
        Select();
    }

    private void Update()
    {
        var format = CultureInfo.CurrentCulture;
        IsEmpty = RangeStart is null;
        Text = (RangeStart, RangeEnd) switch
        {
            ({ } start, { } end) => $"{start.ToString("MMM dd, yyyy", format)} - {end.ToString("MMM dd, yyyy", format)}",
            ({ } start, null) => start.ToString("MMM dd, yyyy", format),
            _ => PlaceholderText,
        };
        Select();
    }

    private void Select()
    {
        _selecting = true;
        try
        {
            foreach (var calendar in new[] { _first, _second }.OfType<Calendar>())
            {
                CalendarSelection.SetRangeStart(calendar, RangeStart);
                CalendarSelection.SetRangeEnd(calendar, RangeEnd ?? RangeStart);
            }
        }
        finally
        {
            _selecting = false;
        }
    }

    /// <summary>Takes the range a calendar's selection was given from elsewhere (a post restoring it on the web).</summary>
    private void Restored(object? sender, SelectionChangedEventArgs e)
    {
        if (_selecting || sender is not Calendar calendar)
        {
            return;
        }

        var days = calendar.SelectedDates.Select(date => date.Date).Order().ToList();
        SetCurrentValue(RangeStartProperty, days.Count > 0 ? days[0] : null);
        SetCurrentValue(RangeEndProperty, days.Count > 0 ? days[^1] : null);
    }

    private void Show(DateTime first)
    {
        if (_first is null || _second is null)
        {
            return;
        }

        _moving = true;
        try
        {
            _first.DisplayDate = first;
            _second.DisplayDate = first.AddMonths(1);
        }
        finally
        {
            _moving = false;
        }
    }

    private void Moved(object? sender, CalendarDateChangedEventArgs e)
    {
        if (_moving || e.AddedDate is not { } shown)
        {
            return;
        }

        Show(sender == _second ? shown.AddMonths(-1) : shown);
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (Day(e.Source) is { } day && e.GetCurrentPoint(day).Properties.IsLeftButtonPressed)
        {
            Choose((DateTime)day.DataContext!);
            _pressed = day;
            e.Handled = true;
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_pressed is { } day && Day(e.Source) == day)
        {
            _pressed = null;
            e.Handled = true;
        }
    }

    private void Keyed(object? sender, KeyEventArgs e)
    {
        if (e.Key is Key.Space or Key.Enter && sender is Calendar calendar
            && calendar.GetVisualDescendants().OfType<CalendarDayButton>().FirstOrDefault(day => ((IPseudoClasses)day.Classes).Contains(":dayfocused")) is { } focused
            && Day(focused) is { } day)
        {
            Choose((DateTime)day.DataContext!);
            e.Handled = true;
        }
    }

    private void Choose(DateTime date)
    {
        var (start, end) = Add(date, RangeStart, RangeEnd);
        SetCurrentValue(RangeStartProperty, start);
        SetCurrentValue(RangeEndProperty, end);
    }

    private static CalendarDayButton? Day(object? source) =>
        (source as Visual)?.FindAncestorOfType<CalendarDayButton>(true) is { DataContext: DateTime, IsEnabled: true } day
        && !((IPseudoClasses)day.Classes).Contains(":blackout")
            ? day
            : null;
}
