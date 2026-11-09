using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;

namespace Aioros.Cosmo.Dates;

/// <summary>
/// The selection of an Avalonia <see cref="Calendar"/> as properties a view model binds two way, as shadcn/ui's
/// <c>selected</c> and <c>onSelect</c>: <see cref="RangeStartProperty"/> and <see cref="RangeEndProperty"/> for a
/// <see cref="CalendarSelectionMode.SingleRange"/> calendar (react-day-picker's <c>mode="range"</c>) and
/// <see cref="DatesProperty"/> for a <see cref="CalendarSelectionMode.MultipleRange"/> one (<c>mode="multiple"</c>).
/// <see cref="Calendar.SelectedDates"/> is a collection no binding can set.
/// </summary>
public static class CalendarSelection
{
    /// <summary>Defines the first day of the selected range.</summary>
    public static readonly AttachedProperty<DateTime?> RangeStartProperty =
        AvaloniaProperty.RegisterAttached<Calendar, DateTime?>("RangeStart", typeof(CalendarSelection), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the last day of the selected range.</summary>
    public static readonly AttachedProperty<DateTime?> RangeEndProperty =
        AvaloniaProperty.RegisterAttached<Calendar, DateTime?>("RangeEnd", typeof(CalendarSelection), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the selected days of a calendar that selects several.</summary>
    public static readonly AttachedProperty<IReadOnlyList<DateTime>?> DatesProperty =
        AvaloniaProperty.RegisterAttached<Calendar, IReadOnlyList<DateTime>?>("Dates", typeof(CalendarSelection), defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<bool> SyncingProperty = AvaloniaProperty.RegisterAttached<Calendar, bool>("Syncing", typeof(CalendarSelection));

    private static readonly AttachedProperty<bool> FollowedProperty = AvaloniaProperty.RegisterAttached<Calendar, bool>("Followed", typeof(CalendarSelection));

    static CalendarSelection()
    {
        RangeStartProperty.Changed.AddClassHandler<Calendar>((calendar, _) => ToCalendar(calendar));
        RangeEndProperty.Changed.AddClassHandler<Calendar>((calendar, _) => ToCalendar(calendar));
        DatesProperty.Changed.AddClassHandler<Calendar>((calendar, _) => ToCalendar(calendar));
        Calendar.SelectionModeProperty.Changed.AddClassHandler<Calendar>((calendar, _) =>
        {
            if (calendar.IsSet(RangeStartProperty) || calendar.IsSet(DatesProperty))
            {
                ToCalendar(calendar);
            }
        });
    }

    /// <summary>Gets <see cref="RangeStartProperty"/>.</summary>
    public static DateTime? GetRangeStart(Calendar calendar) => calendar.GetValue(RangeStartProperty);

    /// <summary>Sets <see cref="RangeStartProperty"/>.</summary>
    public static void SetRangeStart(Calendar calendar, DateTime? value) => calendar.SetValue(RangeStartProperty, value);

    /// <summary>Gets <see cref="RangeEndProperty"/>.</summary>
    public static DateTime? GetRangeEnd(Calendar calendar) => calendar.GetValue(RangeEndProperty);

    /// <summary>Sets <see cref="RangeEndProperty"/>.</summary>
    public static void SetRangeEnd(Calendar calendar, DateTime? value) => calendar.SetValue(RangeEndProperty, value);

    /// <summary>Gets <see cref="DatesProperty"/>.</summary>
    public static IReadOnlyList<DateTime>? GetDates(Calendar calendar) => calendar.GetValue(DatesProperty);

    /// <summary>Sets <see cref="DatesProperty"/>.</summary>
    public static void SetDates(Calendar calendar, IReadOnlyList<DateTime>? value) => calendar.SetValue(DatesProperty, value);

    private static void ToCalendar(Calendar calendar)
    {
        Follow(calendar);
        if (calendar.GetValue(SyncingProperty))
        {
            return;
        }

        calendar.SetValue(SyncingProperty, true);
        try
        {
            calendar.SelectedDates.Clear();
            if (calendar.SelectionMode == CalendarSelectionMode.SingleRange && GetRangeStart(calendar) is { } start)
            {
                var end = GetRangeEnd(calendar) ?? start;
                calendar.SelectedDates.AddRange(start <= end ? start : end, start <= end ? end : start);
            }
            else if (GetDates(calendar) is { } dates)
            {
                foreach (var date in dates)
                {
                    calendar.SelectedDates.Add(date.Date);
                }
            }
        }
        finally
        {
            calendar.SetValue(SyncingProperty, false);
        }
    }

    /// <summary>Writes the calendar's selection back to the attached properties from now on; Cosmo's theme starts it for every calendar.</summary>
    internal static void Follow(Calendar calendar)
    {
        if (calendar.GetValue(FollowedProperty))
        {
            return;
        }

        calendar.SetValue(FollowedProperty, true);
        calendar.SelectedDatesChanged += (_, _) => FromCalendar(calendar);
    }

    private static void FromCalendar(Calendar calendar)
    {
        if (calendar.GetValue(SyncingProperty))
        {
            return;
        }

        calendar.SetValue(SyncingProperty, true);
        try
        {
            var selected = calendar.SelectedDates.Select(date => date.Date).Distinct().Order().ToList();
            if (calendar.SelectionMode == CalendarSelectionMode.SingleRange)
            {
                calendar.SetCurrentValue(RangeStartProperty, selected.Count > 0 ? selected[0] : null);
                calendar.SetCurrentValue(RangeEndProperty, selected.Count > 0 ? selected[^1] : null);
            }
            else
            {
                calendar.SetCurrentValue(DatesProperty, selected);
            }
        }
        finally
        {
            calendar.SetValue(SyncingProperty, false);
        }
    }
}

/// <summary>Days written in XAML for <see cref="CalendarSelection.DatesProperty"/>.</summary>
public sealed class CalendarDates : List<DateTime>;
