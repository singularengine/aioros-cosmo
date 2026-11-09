using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Dates;

public sealed class CalendarTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "CALENDAR-01")]
    public async Task TheSingleCalendarIsShadcnsAsync()
    {
        var wrong = await Looks.FixtureAsync(desktop, "calendar-single", (_, root, reference, theme) => Days(root, reference, theme));

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "CALENDAR-02")]
    public async Task RangesSeveralDaysBlackoutsAndHiddenDaysAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["calendar-range", "calendar-multiple", "calendar-disabled", "calendar-outside-hidden"])
        {
            wrong.AddRange(await Looks.FixtureAsync(desktop, fixture, (_, root, reference, theme) => Days(root, reference, $"{theme} {fixture}")));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "CALENDAR-03")]
    public async Task TheSelectionBindsTwoWayAsync()
    {
        var model = new DatesModel();
        var found = await desktop.RunAsync(() =>
        {
            var view = new DatesView { DataContext = model };
            var window = Windows.Show(view, Avalonia.Styling.ThemeVariant.Light);
            var (one, range, several) = (view.FindControl<Calendar>("One")!, view.FindControl<Calendar>("Range")!, view.FindControl<Calendar>("Several")!);
            Windows.Settle();
            var shown = (one.SelectedDate, range.SelectedDates.Count, Marked(range, "range-start"), Marked(range, "range-end"), string.Join(",", several.SelectedDates.Select(date => date.Day).Order()));
            Windows.Click(window, All<CalendarDayButton>(one).First(day => day.IsVisible && day.Content?.ToString() == "20"));
            range.SelectedDates.Clear();
            range.SelectedDates.AddRange(DatesModel.June(1), DatesModel.June(5));
            several.SelectedDates.Add(DatesModel.June(24));
            var picked = (model.Day, model.Start, model.End, string.Join(",", model.Days!.Select(date => date.Day)));
            model.Start = DatesModel.June(15);
            model.End = DatesModel.June(18);
            Windows.Settle();
            var set = string.Join(",", range.SelectedDates.Select(date => date.Day).Order());
            var fresh = view.FindControl<Calendar>("Fresh")!;
            Windows.Click(window, All<CalendarDayButton>(fresh).First(day => day.IsVisible && day.Content?.ToString() == "9"));
            Windows.Click(window, All<CalendarDayButton>(fresh).First(day => day.IsVisible && day.Content?.ToString() == "26"));
            var tapped = (model.From, model.To);
            Windows.Click(window, Windows.Part<Button>(one, "PART_NextButton"));
            var header = Windows.Part<Button>(one, "PART_HeaderButton").Content?.ToString();
            window.Close();
            return (shown, picked, set, header, tapped);
        });

        Assert.Equal(((DateTime?)DatesModel.June(12), 18, "9", "26", "3,12"), found.shown);
        Assert.Equal(((DateTime?)DatesModel.June(20), (DateTime?)DatesModel.June(1), (DateTime?)DatesModel.June(5), "3,12,24"), found.picked);
        Assert.Equal("15,16,17,18", found.set);
        Assert.Equal(((DateTime?)DatesModel.June(9), (DateTime?)DatesModel.June(26)), found.tapped);
        Assert.Equal(DatesModel.June(1).AddMonths(1).ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.YearMonthPattern, System.Globalization.CultureInfo.CurrentCulture), found.header);
    }

    private static string Marked(Calendar calendar, string mark) =>
        string.Join(",", All<CalendarDayButton>(calendar).Where(day => day.IsVisible && day.Classes.Contains(mark)).Select(day => day.Content));

    private static List<string> Days(Control root, IReadOnlyList<ElementMetrics> reference, string theme)
    {
        var calendar = All<Calendar>(root).Single();
        var box = (Control)calendar.Parent!;
        var errors = Near($"{theme} calendar", Windows.Box(box, root), reference.Single(element => element.Slot == "calendar"), 1).ToList();
        var navigation = reference.Where(element => element.Tag == "button" && string.IsNullOrEmpty(element.Slot)).ToList();
        errors.AddRange(Near($"{theme} previous", Windows.Box(Windows.Part<Button>(calendar, "PART_PreviousButton"), root), navigation[0], 1));
        errors.AddRange(Near($"{theme} next", Windows.Box(Windows.Part<Button>(calendar, "PART_NextButton"), root), navigation[1], 1));
        var days = All<CalendarDayButton>(root).Where(day => day.IsVisible && Windows.Part<Border>(day, "PART_Band").Opacity > 0).OrderBy(Grid.GetRow).ThenBy(Grid.GetColumn).ToList();
        var expected = reference.Where(element => element.Slot == "button").ToList();
        errors.AddRange(days.Count == expected.Count ? [] : [$"{theme}: {days.Count} days, shadcn {expected.Count}"]);
        foreach (var (day, element) in days.Zip(expected))
        {
            errors.AddRange(Near($"{theme} day {element.Text}", Windows.Box(day, root), element, 1));
            errors.AddRange(day.Content?.ToString() == element.Text ? [] : [$"{theme}: day {day.Content}, shadcn {element.Text}"]);
            errors.AddRange(Paints($"{theme} day {element.Text} foreground", day.Foreground, element.Color));
            if (element.Background != "rgba(0, 0, 0, 0)")
            {
                errors.AddRange(Paints($"{theme} day {element.Text} background", day.Background, element.Background));
            }
        }

        return errors;
    }
}
