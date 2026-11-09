using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Navigation;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Display;

/// <summary>
/// The batch 4 reference fixtures on the desktop: the templates <c>samples/Gallery/tools/fixtures.py</c> writes from the
/// same XAML as the Gallery's <c>/fidelity</c> pages, shown in a window the size of shadcn/ui's capture.
/// </summary>
internal static class DisplayFixtures
{
    private static ResourceDictionary? _templates;

    /// <summary>Builds a fixture's content.</summary>
    public static Control Build(string name)
    {
        _templates ??= (ResourceDictionary)AvaloniaXamlLoader.Load(new Uri("avares://Aioros.Cosmo.Tests/Display/Fixtures.axaml"));
        return ((IDataTemplate)_templates[name]!).Build(null)!;
    }

    /// <summary>Whether a fixture is an app shell block, captured filling a 1280 x 800 viewport with no fixture padding.</summary>
    public static bool IsShell(string name) => (name.StartsWith("sidebar-", StringComparison.Ordinal) && name.Length == 10) || name == "dashboard-01";

    /// <summary>Shows a fixture as the reference app's fixture box, in a window of the reference capture's height.</summary>
    public static Window Show(string name, ThemeVariant variant)
    {
        Charts.ChartAccessors.Register();
        using var reference = Pixels.Reference(name, variant == ThemeVariant.Dark ? "dark" : "light");
        return IsShell(name) ? Windows.Fixture(Build(name), reference.Height, variant, width: reference.Width, padding: 0) : Windows.Fixture(Build(name), reference.Height, variant);
    }

    /// <summary>Opens a fixture's overlay as shadcn/ui's capture does, or moves the pointer away from a fixture at rest.</summary>
    public static void Open(Window window, string name)
    {
        switch (name)
        {
            case "navigation-menu-open":
                Windows.Click(window, window.GetVisualDescendants().OfType<NavigationMenuTrigger>().ElementAt(1));
                break;
            case "menubar-open":
                Windows.Click(window, window.GetVisualDescendants().OfType<MenuItem>().First());
                Windows.Away(window, new Point(2, window.Height - 2));
                break;
            case "command-filtered":
                Windows.Part<TextBox>(window.GetVisualDescendants().OfType<Aioros.Cosmo.Search.Command>().First(), "PART_Input").Focus();
                Windows.Type(window, "se");
                Windows.Away(window, new Point(2, window.Height - 2));
                window.Focus();
                break;
            case "command-dialog-open":
                window.KeyPress(Avalonia.Input.Key.J, Avalonia.Input.RawInputModifiers.Control, Avalonia.Input.PhysicalKey.None, null);
                window.KeyRelease(Avalonia.Input.Key.J, Avalonia.Input.RawInputModifiers.Control, Avalonia.Input.PhysicalKey.None, null);
                Windows.Settle();
                window.Focus();
                break;
            case "date-picker-open" or "date-picker-presets-open":
                Windows.Click(window, Windows.Part<Button>(window.GetVisualDescendants().OfType<CalendarDatePicker>().First(), "PART_Button"));
                Windows.Away(window, new Point(2, window.Height - 2));
                break;
            case "date-picker-range-open":
                var range = Windows.Part<Button>(window.GetVisualDescendants().OfType<Aioros.Cosmo.Dates.DateRangePicker>().First(), "PART_Button");
                Windows.Click(window, range);
                range.Focus(Avalonia.Input.NavigationMethod.Pointer);
                Windows.Settle();
                Windows.Away(window, new Point(2, window.Height - 2));
                break;
            case "combobox-open":
                Windows.Click(window, Windows.Part<Button>(window.GetVisualDescendants().OfType<Aioros.Cosmo.Search.Combobox>().First(), "PART_Trigger"));
                Windows.Away(window, new Point(2, window.Height - 2));
                window.Focus();
                break;
            case "data-table-selected":
                var boxes = window.GetVisualDescendants().OfType<CheckBox>().ToList();
                Windows.Click(window, boxes[1]);
                Windows.Click(window, boxes[2]);
                Windows.Click(window, window.GetVisualDescendants().OfType<TableViewColumnHeader>().ElementAt(2).GetVisualDescendants().OfType<Button>().First());
                Windows.Away(window, new Point(2, window.Height - 2));
                break;
            default:
                Windows.Away(window, new Point(2, window.Height - 2));
                break;
        }

        Windows.Settle();
    }
}
