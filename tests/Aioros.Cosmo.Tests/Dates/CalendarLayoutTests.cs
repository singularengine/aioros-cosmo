using Aioros.Cosmo.Dates;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;
using Calendar = Avalonia.Controls.Calendar;

namespace Aioros.Cosmo.Tests.Dates;

public sealed class CalendarLayoutTests(DesktopSession desktop)
{
    private static List<CalendarDayButton> Shown(Calendar calendar) =>
        [.. calendar.GetVisualDescendants().OfType<CalendarDayButton>().Where(day => day.IsVisible).OrderBy(Grid.GetRow).ThenBy(Grid.GetColumn)];

    private static IEnumerable<string> Days(string what, IEnumerable<CalendarDayButton> days, IReadOnlyList<ElementMetrics> reference, Control root, double tolerance)
    {
        var ours = days.ToList();
        var theirs = reference.Where(element => element.Slot == "button").ToList();
        return ours.Count == theirs.Count
            ? ours.Zip(theirs).SelectMany(pair => Near($"{what} day {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second, tolerance))
            : [$"{what}: {ours.Count} days, shadcn {theirs.Count}"];
    }

    [Fact]
    [Trait("AC", "CALENDAR-05")]
    public async Task WeekNumbersStandBeforeTheirWeeksAsync() =>
        Assert.Empty(await FixtureAsync(desktop, "calendar-week-numbers", (_, root, reference, theme) =>
        {
            var calendar = All<Calendar>(root).Single();
            var errors = Near($"{theme} calendar", Windows.Box((Control)calendar.Parent!, root), reference.Single(element => element.Slot == "calendar"), 1).ToList();
            var days = Shown(calendar);
            errors.AddRange(Days(theme, days, reference, root, 1));
            var numbers = calendar.GetVisualDescendants().OfType<TextBlock>().Where(text => text.Classes.Contains("week") && text.IsVisible).OrderBy(Grid.GetRow).ToList();
            errors.AddRange(numbers.Select(text => text.Text).SequenceEqual(["23", "24", "25", "26", "27"]) ? [] : [$"{theme}: weeks {string.Join(",", numbers.Select(text => text.Text))}"]);
            foreach (var number in numbers)
            {
                var box = Windows.Box(number, root);
                var row = Windows.Box(days.First(day => Grid.GetRow(day) == Grid.GetRow(number)), root);
                errors.AddRange(Math.Abs(box.Center.Y - row.Center.Y) <= 1 && Math.Abs(box.Right - row.X) <= 1 && box.Width == 32 ? [] : [$"{theme} week {number.Text}: {box}, its row {row}"]);
            }

            return errors;
        }));

    [Fact]
    [Trait("AC", "CALENDAR-06")]
    public async Task TheDropdownCaptionChoosesTheMonthAndYearAsync()
    {
        var wrong = await FixtureAsync(desktop, "calendar-dropdown", (_, root, reference, theme) =>
            Near($"{theme} calendar", Windows.Box((Control)All<Calendar>(root).Single().Parent!, root), reference.Single(element => element.Slot == "calendar"), 1));
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("calendar-dropdown", Avalonia.Styling.ThemeVariant.Light);
            var calendar = All<Calendar>((Control)window.Content!).Single();
            var selects = calendar.GetVisualDescendants().OfType<ComboBox>().Where(select => select.IsEffectivelyVisible).ToList();
            string Read() => string.Join(" ", selects.Select(select => select.SelectionBoxItem));
            var header = calendar.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_HeaderButton").IsEffectivelyVisible;
            var steps = new List<string> { $"{Read()} {header} {CalendarCaption.GetYears(calendar)![0]}-{CalendarCaption.GetYears(calendar)![^1]}" };
            selects[0].SelectedIndex = 8;
            Windows.Settle();
            steps.Add($"{calendar.DisplayDate:yyyy-MM}");
            selects[1].SelectedIndex = CalendarCaption.GetYears(calendar)!.ToList().IndexOf("2024");
            Windows.Settle();
            steps.Add($"{calendar.DisplayDate:yyyy-MM}");
            Windows.Click(window, calendar.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_PreviousButton"));
            Windows.Settle();
            steps.Add(Read());
            window.Close();
            return steps;
        });

        Assert.Empty(wrong);
        Assert.Equal([$"Jun 2025 False {DateTime.Today.Year - 100}-{DateTime.Today.Year}", "2025-09", "2024-09", "Aug 2024"], found);
    }

    [Fact]
    [Trait("AC", "CALENDAR-07")]
    public async Task APlainClickTogglesADayOfSeveralAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("calendar-multiple", Avalonia.Styling.ThemeVariant.Light);
            var calendar = All<Calendar>((Control)window.Content!).Single();
            string Selected() => string.Join(",", calendar.SelectedDates.Order().Select(date => date.Day));
            CalendarDayButton Day(int day) => Shown(calendar).First(button => button.DataContext is DateTime date && date.Month == 6 && date.Day == day);
            var steps = new List<string> { Selected() };
            Windows.Click(window, Day(24));
            Windows.Settle();
            steps.Add(Selected());
            Windows.Click(window, Day(25));
            Windows.Settle();
            steps.Add(Selected());
            window.Close();
            return steps;
        });

        Assert.Equal(["3,12,13,24", "3,12,13", "3,12,13,25"], found);
    }

    [Fact]
    [Trait("AC", "DATEPICKER-03")]
    public async Task TheRangePickerShowsTwoMonthsAndAddsToItsRangeAsync()
    {
        var wrong = await FixtureAsync(desktop, "date-picker-range-open", (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, "date-picker-range-open");
            var picker = All<DateRangePicker>(root).Single();
            var errors = Near($"{theme} button", Windows.Box(picker, root), reference.Single(element => element.Slot == "popover-trigger"), 1).ToList();
            errors.AddRange(picker.Text == "Jan 20, 2022 - Feb 09, 2022" ? [] : [$"{theme}: text {picker.Text}"]);
            var button = Windows.Part<Button>(picker, "PART_Button");
            var content = (Control)((Flyout)button.Flyout!).Content!;
            var surface = content.GetVisualAncestors().OfType<Border>().First(border => border.Name == "LayoutRoot");
            errors.AddRange(Near($"{theme} popover", Windows.Box(surface, root), reference.Single(element => element.Slot == "popover-content"), 1.5));
            var calendars = content.GetVisualDescendants().OfType<Calendar>().ToList();
            errors.AddRange(Days(theme, calendars.SelectMany(Shown), reference, root, 1.5));
            button.Flyout.Hide();
            return errors;
        });
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("date-picker-range-open", Avalonia.Styling.ThemeVariant.Light);
            DisplayFixtures.Open(window, "date-picker-range-open");
            var picker = All<DateRangePicker>((Control)window.Content!).Single();
            var calendars = ((Control)((Flyout)Windows.Part<Button>(picker, "PART_Button").Flyout!).Content!).GetVisualDescendants().OfType<Calendar>().ToList();
            CalendarDayButton Day(int month, int day) => calendars.SelectMany(Shown).First(button => button.DataContext is DateTime date && date.Month == month && date.Day == day && !((IPseudoClasses)button.Classes).Contains(":inactive"));
            string Range() => $"{picker.RangeStart:MM-dd}..{picker.RangeEnd:MM-dd}";
            var steps = new List<string>();
            foreach (var (month, day) in new[] { (1, 25), (2, 14), (2, 14) })
            {
                Windows.Click(window, Day(month, day));
                Windows.Settle();
                steps.Add(Range());
            }

            Windows.Click(window, calendars[1].GetVisualDescendants().OfType<Button>().Single(button => button.Name == "PART_NextButton"));
            Windows.Settle();
            steps.Add($"{calendars[0].DisplayDate:yyyy-MM} {calendars[1].DisplayDate:yyyy-MM}");
            window.Close();
            return steps;
        });

        Assert.Empty(wrong);
        Assert.Equal(["01-20..01-25", "01-20..02-14", "02-14..02-14", "2022-02 2022-03"], found);
    }

    [Fact]
    [Trait("AC", "DATEPICKER-04")]
    public async Task PresetsPickDaysFromTodayAsync()
    {
        var wrong = await FixtureAsync(desktop, "date-picker-presets-open", (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, "date-picker-presets-open");
            var picker = All<CalendarDatePicker>(root).Single();
            var surface = (Border)Windows.Part<Avalonia.Controls.Primitives.Popup>(picker, "PART_Popup").Child!;
            var select = surface.GetVisualDescendants().OfType<ComboBox>().Single(part => part.Name == "PART_Presets");
            var frame = surface.GetVisualDescendants().OfType<Border>().Single(part => part.Name == "PART_CalendarFrame");
            var errors = Windows.Part<TextBox>(picker, "PART_TextBox").Text == "June 12th, 2025" ? new List<string>() : [$"{theme}: text {Windows.Part<TextBox>(picker, "PART_TextBox").Text}"];
            errors.AddRange(Near($"{theme} popover", Windows.Box(surface, root), reference.Single(element => element.Slot == "popover-content"), 1.5));
            errors.AddRange(select.IsEffectivelyVisible && select.SelectedIndex == -1 && select.PlaceholderText == "Select" && frame.BorderThickness == new Thickness(1) ? [] : [$"{theme}: select {select.IsEffectivelyVisible} {select.SelectedIndex} frame {frame.BorderThickness}"]);
            Windows.Part<Avalonia.Controls.Primitives.Popup>(picker, "PART_Popup").IsOpen = false;
            return errors;
        });
        var found = await desktop.RunAsync(() =>
        {
            var window = DisplayFixtures.Show("date-picker-presets-open", Avalonia.Styling.ThemeVariant.Light);
            DisplayFixtures.Open(window, "date-picker-presets-open");
            var picker = All<CalendarDatePicker>((Control)window.Content!).Single();
            Windows.Part<Avalonia.Controls.Primitives.Popup>(picker, "PART_Popup").Child!.GetVisualDescendants().OfType<ComboBox>().Single(part => part.Name == "PART_Presets").SelectedIndex = 1;
            Windows.Settle();
            var text = Windows.Part<TextBox>(picker, "PART_TextBox").Text;
            window.Close();
            return (picker.SelectedDate, text);
        });

        Assert.Empty(wrong);
        Assert.Equal((DateTime.Today.AddDays(1), DateText.Format(DateTime.Today.AddDays(1), System.Globalization.CultureInfo.CurrentCulture)), found);
    }
}
