using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Calendar = Avalonia.Controls.Calendar;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Dates;

/// <summary>
/// The day grid of an Avalonia <see cref="Calendar"/> as react-day-picker lays it out for shadcn/ui: only the weeks that
/// hold a day of the shown month (Avalonia always shows six, and a whole week of the previous month when the month starts
/// on the first day of the week), the days of a range marked <c>range-start</c>, <c>range-middle</c> and
/// <c>range-end</c> with the band joining them rounded at the ends of each week, the other months' days hidden with
/// the calendar's <c>outside-hidden</c> class (shadcn's <c>showOutsideDays={false}</c>), each week's number in the
/// culture's week rule before its days with the <c>week-numbers</c> class (<c>showWeekNumber</c>), the caption's month
/// and year selects with the <c>dropdown</c> class (<c>captionLayout="dropdown"</c>, <see cref="CalendarCaption"/>), and
/// a plain click or Space toggling a day in <see cref="CalendarSelectionMode.MultipleRange"/> (<c>mode="multiple"</c>),
/// where Avalonia replaces the selection without Ctrl. Cosmo's theme sets <see cref="FollowsProperty"/> on every calendar.
/// </summary>
public static class CalendarDays
{
    /// <summary>Defines the attached property that makes a calendar's day grid follow shadcn/ui's.</summary>
    public static readonly AttachedProperty<bool> FollowsProperty = AvaloniaProperty.RegisterAttached<Calendar, bool>("Follows", typeof(CalendarDays));

    private static readonly AttachedProperty<CalendarDayButton?> ToggledProperty = AvaloniaProperty.RegisterAttached<Calendar, CalendarDayButton?>("Toggled", typeof(CalendarDays));

    private static readonly string[] Marks = ["range-start", "range-middle", "range-end", "outside-hidden"];

    static CalendarDays() => FollowsProperty.Changed.AddClassHandler<Calendar>((calendar, change) =>
    {
        if (change.GetNewValue<bool>())
        {
            CalendarSelection.Follow(calendar);
            CalendarCaption.Follow(calendar);
            calendar.SelectedDatesChanged += (_, _) => Later(calendar);
            calendar.DisplayDateChanged += (_, _) => Later(calendar);
            calendar.TemplateApplied += (_, _) => Later(calendar);
            calendar.Classes.CollectionChanged += (_, _) => Later(calendar);
            calendar.PropertyChanged += (_, e) =>
            {
                if (e.Property == Calendar.SelectionModeProperty)
                {
                    Later(calendar);
                }
            };
            calendar.AttachedToVisualTree += (_, _) => Later(calendar);
            calendar.AddHandler(InputElement.PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
            calendar.AddHandler(InputElement.PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
            calendar.AddHandler(InputElement.KeyDownEvent, Keyed, RoutingStrategies.Tunnel);
        }
    });

    /// <summary>Gets <see cref="FollowsProperty"/>.</summary>
    public static bool GetFollows(Calendar calendar) => calendar.GetValue(FollowsProperty);

    /// <summary>Sets <see cref="FollowsProperty"/>.</summary>
    public static void SetFollows(Calendar calendar, bool value) => calendar.SetValue(FollowsProperty, value);

    /// <summary>Lays the calendar's days out at once, as its <see cref="CalendarItem"/> left them.</summary>
    public static void Follow(Calendar calendar)
    {
        Caption(calendar);
        var view = calendar.GetVisualDescendants().OfType<Grid>().FirstOrDefault(grid => grid.Name == "PART_MonthView");
        var days = view?.Children.OfType<CalendarDayButton>().Where(day => day.DataContext is DateTime).ToList() ?? [];
        if (view is null || days.Count == 0)
        {
            return;
        }

        var range = calendar.SelectionMode == CalendarSelectionMode.SingleRange && calendar.SelectedDates.Count > 1
            ? (Start: calendar.SelectedDates.Min(), End: calendar.SelectedDates.Max())
            : ((DateTime Start, DateTime End)?)null;
        var hidden = calendar.Classes.Contains("outside-hidden");
        var radius = calendar.TryFindResource("CosmoRadiusMd", calendar.ActualThemeVariant, out var found) && found is CornerRadius md ? md : new CornerRadius(8);
        var numbers = Numbers(view, calendar.Classes.Contains("week-numbers"));
        foreach (var week in days.GroupBy(Grid.GetRow))
        {
            var row = week.OrderBy(Grid.GetColumn).ToList();
            var shown = row.Exists(day => !Inactive(day));
            for (var index = 0; index < row.Count; index++)
            {
                Mark(row[index], shown, range, hidden, index == 0, index == row.Count - 1, radius);
            }

            if (numbers.Count > week.Key)
            {
                numbers[week.Key].IsVisible = shown && numbers[0].IsVisible;
                numbers[week.Key].Text = Week((DateTime)row[0].DataContext!, calendar.FirstDayOfWeek);
            }
        }
    }

    private static void Later(Calendar calendar) => Dispatcher.UIThread.Post(() => Follow(calendar), DispatcherPriority.Loaded);

    private static bool Inactive(CalendarDayButton day) => ((IPseudoClasses)day.Classes).Contains(":inactive");

    /// <summary>
    /// Shows the caption's selects in place of its label with the <c>dropdown</c> class, and leaves out the next button of a
    /// first month (<c>range-first</c>) and the previous button of a last one (<c>range-last</c>), as react-day-picker
    /// navigates several months from their ends.
    /// </summary>
    private static void Caption(Calendar calendar)
    {
        var dropdown = calendar.Classes.Contains("dropdown");
        foreach (var part in calendar.GetVisualDescendants().OfType<Control>().Where(part => part.Name is "PART_HeaderButton" or "PART_Dropdowns" or "PART_PreviousButton" or "PART_NextButton"))
        {
            part.IsVisible = part.Name switch
            {
                "PART_PreviousButton" => !calendar.Classes.Contains("range-last"),
                "PART_NextButton" => !calendar.Classes.Contains("range-first"),
                _ => (part.Name == "PART_Dropdowns") == dropdown,
            };
        }
    }

    /// <summary>
    /// The week numbers of the month view, one per row with an empty head for the weekdays' row, in the column before the
    /// days, which move one column on once; shown with the <c>week-numbers</c> class.
    /// </summary>
    private static List<TextBlock> Numbers(Grid view, bool shown)
    {
        var numbers = view.Children.OfType<TextBlock>().Where(text => text.Classes.Contains("week-number")).OrderBy(Grid.GetRow).ToList();
        if (numbers.Count == 0)
        {
            foreach (var child in view.Children)
            {
                Grid.SetColumn(child, Grid.GetColumn(child) + 1);
            }

            numbers = [.. Enumerable.Range(0, 7).Select(row => new TextBlock { Classes = { "week-number", row == 0 ? "week-head" : "week" }, [Grid.RowProperty] = row, [Grid.ColumnProperty] = 0 })];
            view.Children.AddRange(numbers);
        }

        numbers.ForEach(number => number.IsVisible = shown);
        return numbers;
    }

    /// <summary>A week's number in the current culture's rule, as react-day-picker numbers a week by its locale.</summary>
    private static string Week(DateTime first, DayOfWeek start)
    {
        var culture = CultureInfo.CurrentCulture;
        return culture.Calendar.GetWeekOfYear(first, culture.DateTimeFormat.CalendarWeekRule, start).ToString(culture);
    }

    /// <summary>A plain press on a day of a calendar that selects several toggles it, as react-day-picker's <c>mode="multiple"</c>.</summary>
    private static void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Calendar calendar || Toggles(calendar, e.Source, e.KeyModifiers) is not { } day || !e.GetCurrentPoint(day).Properties.IsLeftButtonPressed)
        {
            return;
        }

        Toggle(calendar, (DateTime)day.DataContext!);
        calendar.SetValue(ToggledProperty, day);
        calendar.Focus();
        e.Handled = true;
    }

    private static void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (sender is Calendar calendar && calendar.GetValue(ToggledProperty) is { } day && (e.Source as Visual)?.FindAncestorOfType<CalendarDayButton>(true) == day)
        {
            calendar.ClearValue(ToggledProperty);
            e.Handled = true;
        }
    }

    private static void Keyed(object? sender, KeyEventArgs e)
    {
        if (sender is not Calendar calendar || e.Key is not (Key.Space or Key.Enter) || calendar.SelectionMode != CalendarSelectionMode.MultipleRange || e.KeyModifiers.HasFlag(KeyModifiers.Shift))
        {
            return;
        }

        if (calendar.GetVisualDescendants().OfType<CalendarDayButton>().FirstOrDefault(day => ((IPseudoClasses)day.Classes).Contains(":dayfocused")) is { DataContext: DateTime date } focused
            && Toggles(calendar, focused, KeyModifiers.None) is not null)
        {
            Toggle(calendar, date);
            e.Handled = true;
        }
    }

    private static CalendarDayButton? Toggles(Calendar calendar, object? source, KeyModifiers modifiers) =>
        calendar.SelectionMode == CalendarSelectionMode.MultipleRange && !modifiers.HasFlag(KeyModifiers.Shift)
        && (source as Visual)?.FindAncestorOfType<CalendarDayButton>(true) is { DataContext: DateTime, IsEnabled: true } day
        && !((IPseudoClasses)day.Classes).Contains(":blackout")
            ? day
            : null;

    private static void Toggle(Calendar calendar, DateTime date)
    {
        if (calendar.SelectedDates.Contains(date.Date))
        {
            calendar.SelectedDates.Remove(date.Date);
        }
        else
        {
            calendar.SelectedDates.Add(date.Date);
        }
    }

    private static void Mark(CalendarDayButton day, bool shown, (DateTime Start, DateTime End)? range, bool hidden, bool first, bool last, CornerRadius radius)
    {
        day.IsVisible = shown;
        day.Classes.RemoveAll(Marks);
        var date = ((DateTime)day.DataContext!).Date;
        var place = range is { } r ? Place(date, r.Start.Date, r.End.Date) : Range.None;
        day.Classes.AddRange(Names(place, hidden));
        var left = new CornerRadius(radius.TopLeft, 0, 0, radius.BottomLeft);
        var right = new CornerRadius(0, radius.TopRight, radius.BottomRight, 0);
        if (place == Range.Middle)
        {
            day.CornerRadius = Edge(first, last, radius, left, right);
        }
        else
        {
            day.ClearValue(TemplatedControl.CornerRadiusProperty);
        }

        if (day.GetVisualDescendants().OfType<Border>().FirstOrDefault(part => part.Name == "PART_Band") is { } band)
        {
            band.Background = place is Range.Start or Range.End ? day.FindResource("CosmoAccentBrush") as Avalonia.Media.IBrush : null;
            band.CornerRadius = place switch
            {
                Range.Start => last ? radius : left,
                Range.End => first ? radius : right,
                _ => default,
            };
        }
    }

    private static Range Place(DateTime date, DateTime start, DateTime end) =>
        date == start && date == end ? Range.Single
        : date == start ? Range.Start
        : date == end ? Range.End
        : date > start && date < end ? Range.Middle
        : Range.None;

    private static List<string> Names(Range place, bool hidden)
    {
        List<string> names = place switch
        {
            Range.Start => ["range-start"],
            Range.End => ["range-end"],
            Range.Middle => ["range-middle"],
            Range.Single => ["range-start", "range-end"],
            _ => [],
        };
        return hidden ? [.. names, "outside-hidden"] : names;
    }

    private static CornerRadius Edge(bool first, bool last, CornerRadius radius, CornerRadius left, CornerRadius right) =>
        first && last ? radius : first ? left : last ? right : default;

    private enum Range
    {
        None,
        Start,
        Middle,
        End,
        Single,
    }
}
