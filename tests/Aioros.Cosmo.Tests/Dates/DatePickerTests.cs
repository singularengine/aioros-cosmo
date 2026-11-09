using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Dates;

public sealed class DatePickerTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "DATEPICKER-01")]
    public async Task TheButtonThePopoverAndTheInputAreShadcnsAsync()
    {
        var wrong = await Looks.FixtureAsync(desktop, "date-picker-demo", (_, root, reference, theme) =>
        {
            var picker = All<CalendarDatePicker>(root).Single();
            var trigger = reference.Single(element => element.Slot == "popover-trigger");
            var errors = Near($"{theme} button", Windows.Box(picker, root), trigger, 1).ToList();
            errors.AddRange(Paints($"{theme} border", Windows.Part<Border>(picker, "PART_Root").BorderBrush, trigger.BorderColor));
            errors.AddRange(Text(picker) == ("Pick a date", true) ? [] : [$"{theme}: text {Text(picker)}"]);
            return errors;
        });
        wrong.AddRange(await Looks.FixtureAsync(desktop, "date-picker-open", (window, root, reference, theme) =>
        {
            DisplayFixtures.Open(window, "date-picker-open");
            var picker = All<CalendarDatePicker>(root).Single();
            var errors = Text(picker) == ("June 12th, 2025", false) ? new List<string>() : [$"{theme}: text {Text(picker)}"];
            var calendar = window.GetVisualDescendants().OfType<Calendar>().FirstOrDefault() ?? Windows.Part<Popup>(picker, "PART_Popup").Child!.GetVisualDescendants().OfType<Calendar>().Single();
            errors.AddRange(Near($"{theme} popover", Windows.Box(calendar.GetVisualAncestors().OfType<Border>().First(border => border.Name == "PART_Surface"), root), reference.Single(element => element.Slot == "popover-content"), 1.5));
            var days = calendar.GetVisualDescendants().OfType<CalendarDayButton>().Where(day => day.IsVisible).OrderBy(Grid.GetRow).ThenBy(Grid.GetColumn).ToList();
            var expected = reference.Where(element => element.Slot == "button").ToList();
            errors.AddRange(days.Count == expected.Count ? days.Zip(expected).SelectMany(pair => Near($"{theme} day {pair.Second.Text}", Windows.Box(pair.First, root), pair.Second, 1.5)) : [$"{theme}: {days.Count} days"]);
            Windows.Part<Popup>(picker, "PART_Popup").IsOpen = false;
            return errors;
        }));
        wrong.AddRange(await Looks.FixtureAsync(desktop, "date-picker-input", (_, root, reference, theme) =>
        {
            var picker = All<CalendarDatePicker>(root).Single();
            var errors = Near($"{theme} input", Windows.Box(picker, root), reference.Single(element => element.Slot == "input"), 1).ToList();
            errors.AddRange(Near($"{theme} calendar button", Windows.Box(Windows.Part<Button>(picker, "PART_Button"), root), reference.Single(element => element.Slot == "popover-trigger"), 1));
            errors.AddRange(Text(picker) == ("June 01, 2025", false) ? [] : [$"{theme}: text {Text(picker)}"]);
            return errors;
        }));

        Assert.Empty(wrong);
    }

    private static (string Text, bool Placeholder) Text(CalendarDatePicker picker)
    {
        var box = Windows.Part<TextBox>(picker, "PART_TextBox");
        return string.IsNullOrEmpty(box.Text) ? (box.PlaceholderText ?? string.Empty, true) : (box.Text, false);
    }
}
