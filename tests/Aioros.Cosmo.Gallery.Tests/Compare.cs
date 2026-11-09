using System.Globalization;
using Aioros.Cosmo.Testing;

namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>Comparisons of Cosmo's computed styles and boxes with shadcn/ui's.</summary>
internal static class Compare
{
    public static double Px(string value) => value.EndsWith("px", StringComparison.Ordinal) ? double.Parse(value[..^2], CultureInfo.InvariantCulture) : double.Parse(value, CultureInfo.InvariantCulture);

    /// <summary>The colors that differ (background, color, and the border's when it has a width).</summary>
    public static IEnumerable<string> Colors(string what, ElementMetrics found, ElementMetrics expected)
    {
        if (!CssColor.Parse(found.Background).Near(CssColor.Parse(expected.Background)))
        {
            yield return $"{what}: background {found.Background}, shadcn {expected.Background}";
        }

        if (!CssColor.Parse(found.Color).Near(CssColor.Parse(expected.Color)))
        {
            yield return $"{what}: color {found.Color}, shadcn {expected.Color}";
        }

        if (Math.Abs(Px(found.BorderWidth) - Px(expected.BorderWidth)) > 0.01 || (Px(expected.BorderWidth) > 0 && !CssColor.Parse(found.BorderColor).Near(CssColor.Parse(expected.BorderColor))))
        {
            yield return $"{what}: border {found.BorderWidth} {found.BorderColor}, shadcn {expected.BorderWidth} {expected.BorderColor}";
        }
    }

    /// <summary>Whether two boxes are within <paramref name="tolerance"/> px on every edge.</summary>
    public static IEnumerable<string> Box(string what, ElementMetrics found, ElementMetrics expected, double tolerance) =>
        new[] { ("x", found.X, expected.X), ("y", found.Y, expected.Y), ("width", found.Width, expected.Width), ("height", found.Height, expected.Height) }
            .Where(edge => Math.Abs(edge.Item2 - edge.Item3) > tolerance)
            .Select(edge => $"{what}: {edge.Item1} {edge.Item2:0.##}, shadcn {edge.Item3:0.##}");

    /// <summary>
    /// The reference elements no element of the page stands at: none with the same box within <paramref name="tolerance"/>
    /// px and, when <paramref name="colors"/>, the same background (the parts Aioros writes carry no shadcn slots, so
    /// they are found by where they stand).
    /// </summary>
    public static IEnumerable<string> Standing(string what, IReadOnlyList<ElementMetrics> page, IEnumerable<ElementMetrics> expected, double tolerance, bool colors = true) =>
        expected.Where(element => !page.Any(found => !Box(string.Empty, found, element, tolerance).Any()
                && (!colors || CssColor.Parse(found.Background).Near(CssColor.Parse(element.Background)))))
            .Select(element => $"{what}: nothing stands at shadcn's {element.Slot} {element.Text} {element.X:0.##}, {element.Y:0.##}, {element.Width:0.##}x{element.Height:0.##} in {element.Background}");
}
