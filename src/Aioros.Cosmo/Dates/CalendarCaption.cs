using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Calendar = Avalonia.Controls.Calendar;
using Avalonia.Data;

namespace Aioros.Cosmo.Dates;

/// <summary>
/// The month and year selects of a calendar whose caption is a drop-down (react-day-picker's
/// <c>captionLayout="dropdown"</c>, Cosmo's <c>dropdown</c> class): the culture's abbreviated month names and the years
/// from a hundred before today to today (react-day-picker's default range, widened to <see cref="Calendar.DisplayDateStart"/>,
/// <see cref="Calendar.DisplayDateEnd"/> and the shown year), with the shown month and year as indexes the template's
/// combo boxes bind two way; choosing one shows that month. Cosmo's theme starts it for every calendar.
/// </summary>
public static class CalendarCaption
{
    /// <summary>Defines the months the month select offers.</summary>
    public static readonly AttachedProperty<IReadOnlyList<string>?> MonthsProperty =
        AvaloniaProperty.RegisterAttached<Calendar, IReadOnlyList<string>?>("Months", typeof(CalendarCaption));

    /// <summary>Defines the shown month as an index into <see cref="MonthsProperty"/>.</summary>
    public static readonly AttachedProperty<int> MonthProperty =
        AvaloniaProperty.RegisterAttached<Calendar, int>("Month", typeof(CalendarCaption), -1, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines the years the year select offers.</summary>
    public static readonly AttachedProperty<IReadOnlyList<string>?> YearsProperty =
        AvaloniaProperty.RegisterAttached<Calendar, IReadOnlyList<string>?>("Years", typeof(CalendarCaption));

    /// <summary>Defines the shown year as an index into <see cref="YearsProperty"/>.</summary>
    public static readonly AttachedProperty<int> YearProperty =
        AvaloniaProperty.RegisterAttached<Calendar, int>("Year", typeof(CalendarCaption), -1, defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<int> FirstYearProperty = AvaloniaProperty.RegisterAttached<Calendar, int>("FirstYear", typeof(CalendarCaption));

    private static readonly AttachedProperty<bool> SyncingProperty = AvaloniaProperty.RegisterAttached<Calendar, bool>("Syncing", typeof(CalendarCaption));

    static CalendarCaption()
    {
        MonthProperty.Changed.AddClassHandler<Calendar>((calendar, _) => Show(calendar));
        YearProperty.Changed.AddClassHandler<Calendar>((calendar, _) => Show(calendar));
    }

    /// <summary>Gets <see cref="MonthsProperty"/>.</summary>
    public static IReadOnlyList<string>? GetMonths(Calendar calendar) => calendar.GetValue(MonthsProperty);

    /// <summary>Gets <see cref="MonthProperty"/>.</summary>
    public static int GetMonth(Calendar calendar) => calendar.GetValue(MonthProperty);

    /// <summary>Sets <see cref="MonthProperty"/>.</summary>
    public static void SetMonth(Calendar calendar, int value) => calendar.SetValue(MonthProperty, value);

    /// <summary>Gets <see cref="YearsProperty"/>.</summary>
    public static IReadOnlyList<string>? GetYears(Calendar calendar) => calendar.GetValue(YearsProperty);

    /// <summary>Gets <see cref="YearProperty"/>.</summary>
    public static int GetYear(Calendar calendar) => calendar.GetValue(YearProperty);

    /// <summary>Sets <see cref="YearProperty"/>.</summary>
    public static void SetYear(Calendar calendar, int value) => calendar.SetValue(YearProperty, value);

    /// <summary>Fills the selects in the current culture and keeps them on the shown month from now on.</summary>
    internal static void Follow(Calendar calendar)
    {
        calendar.DisplayDateChanged += (_, _) => Shown(calendar);
        calendar.PropertyChanged += (_, e) =>
        {
            if (e.Property == Calendar.DisplayDateStartProperty || e.Property == Calendar.DisplayDateEndProperty)
            {
                Shown(calendar);
            }
        };
        Shown(calendar);
    }

    private static void Shown(Calendar calendar)
    {
        var shown = calendar.DisplayDate;
        var today = DateTime.Today;
        var first = Math.Min(calendar.DisplayDateStart?.Year ?? today.Year - 100, shown.Year);
        var last = Math.Max(calendar.DisplayDateEnd?.Year ?? today.Year, shown.Year);
        var format = CultureInfo.CurrentCulture.DateTimeFormat;
        calendar.SetValue(SyncingProperty, true);
        try
        {
            calendar.SetValue(MonthsProperty, format.AbbreviatedMonthNames.Take(12).ToArray());
            calendar.SetValue(YearsProperty, Enumerable.Range(first, last - first + 1).Select(year => year.ToString(format)).ToArray());
            calendar.SetValue(FirstYearProperty, first);
            calendar.SetCurrentValue(MonthProperty, shown.Month - 1);
            calendar.SetCurrentValue(YearProperty, shown.Year - first);
        }
        finally
        {
            calendar.SetValue(SyncingProperty, false);
        }
    }

    private static void Show(Calendar calendar)
    {
        var (month, year) = (GetMonth(calendar), GetYear(calendar));
        if (calendar.GetValue(SyncingProperty) || month is < 0 or > 11 || year < 0 || year >= (GetYears(calendar)?.Count ?? 0))
        {
            return;
        }

        var shown = new DateTime(calendar.GetValue(FirstYearProperty) + year, month + 1, 1, 0, 0, 0, calendar.DisplayDate.Kind);
        if (shown.Year != calendar.DisplayDate.Year || shown.Month != calendar.DisplayDate.Month)
        {
            calendar.SetCurrentValue(Calendar.DisplayDateProperty, shown);
        }
    }
}
