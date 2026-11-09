using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Display;

/// <summary>What the batch 4 desktop criteria compare: boxes and paints against shadcn/ui's metrics, in both themes.</summary>
internal static class Looks
{
    public static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    public static Rect Of(ElementMetrics element) => new(element.X, element.Y, element.Width, element.Height);

    public static double Px(string px) => double.Parse(px.Replace("px", string.Empty, StringComparison.Ordinal), System.Globalization.CultureInfo.InvariantCulture);

    /// <summary>A box differing from shadcn/ui's by more than <paramref name="tolerance"/> on any edge.</summary>
    public static IEnumerable<string> Near(string what, Rect found, ElementMetrics expected, double tolerance) =>
        Math.Abs(found.X - expected.X) <= tolerance && Math.Abs(found.Y - expected.Y) <= tolerance && Math.Abs(found.Width - expected.Width) <= tolerance && Math.Abs(found.Height - expected.Height) <= tolerance
            ? []
            : [$"{what}: {found}, shadcn {Of(expected)}"];

    /// <summary>A box whose position and height differ from shadcn/ui's (its width is its text's in shadcn's, its column's here).</summary>
    public static IEnumerable<string> Placed(string what, Rect found, ElementMetrics expected, double tolerance) =>
        Math.Abs(found.X - expected.X) <= tolerance && Math.Abs(found.Y - expected.Y) <= tolerance && Math.Abs(found.Height - expected.Height) <= tolerance
            ? []
            : [$"{what}: {found}, shadcn {Of(expected)}"];

    /// <summary>A control's layout box relative to another, ignoring its render transform.</summary>
    public static Rect Laid(Control control, Control root) =>
        new((control.GetVisualParent() as Visual)?.TranslatePoint(control.Bounds.Position, root) ?? default, control.Bounds.Size);

    public static IEnumerable<string> Paints(string what, IBrush? found, string expected) =>
        Windows.Paint(found).Near(CssColor.Parse(expected)) ? [] : [$"{what}: {Windows.Paint(found)}, shadcn {expected}"];

    /// <summary>A brush that does not paint what a theme resource paints.</summary>
    public static IEnumerable<string> Paints(string what, IBrush? found, Window window, string key)
    {
        var expected = Windows.Paint((IBrush)window.FindResource(window.ActualThemeVariant, key)!);
        return Windows.Paint(found) == expected ? [] : [$"{what}: {Windows.Paint(found)}, {key} {expected}"];
    }

    public static List<T> All<T>(Control root) => [.. root.GetVisualDescendants().OfType<T>()];

    /// <summary>Shows a fixture in both themes and gathers what <paramref name="check"/> finds wrong, given the window, the fixture's root and shadcn/ui's elements.</summary>
    public static async Task<List<string>> FixtureAsync(DesktopSession desktop, string fixture, Func<Window, Control, IReadOnlyList<ElementMetrics>, string, IEnumerable<string>> check)
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync(fixture, name);
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = DisplayFixtures.Show(fixture, variant);
                var root = (Control)window.Content!;
                var errors = check(window, root, reference, name).ToList();
                window.Close();
                return errors;
            }));
        }

        return wrong;
    }
}
