using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Theming;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Charts;

public sealed class ChartThemeTests(DesktopSession desktop)
{
    private static readonly (string Key, string Token, double Alpha)[] Keys =
    [
        ("PictorPalette1", "Chart1", 1), ("PictorPalette2", "Chart2", 1), ("PictorPalette3", "Chart3", 1), ("PictorPalette4", "Chart4", 1), ("PictorPalette5", "Chart5", 1),
        ("PictorSeriesColor", "Chart1", 1), ("PictorGridStroke", "Border", 0.5), ("PictorTickText", "MutedForeground", 1), ("PictorPolarTickText", "MutedForeground", 1),
        ("PictorCursorStroke", "Border", 1), ("PictorCursorFill", "Muted", 1), ("PictorActiveDotStroke", "Background", 0), ("PictorPolarGridStroke", "Border", 1),
        ("PictorReferenceLineStroke", "Border", 1), ("PictorBarBackground", "Muted", 1), ("PictorPieStroke", "Background", 0), ("PictorTooltipBackground", "Background", 1),
        ("PictorTooltipBorder", "Border", 0.5), ("PictorTooltipLabel", "Foreground", 1), ("PictorTooltipText", "Foreground", 1),
        ("CosmoChartForeground", "Foreground", 1), ("CosmoChartMutedForeground", "MutedForeground", 1),
    ];

    [Fact]
    [Trait("AC", "CHART-01")]
    public async Task EveryColorThemeGivesPictorTheChartTokensAsync()
    {
        var wrong = await desktop.RunAsync(() => Enum.GetValues<CosmoColor>()
            .SelectMany(color => new[] { ThemeVariant.Light, ThemeVariant.Dark }.SelectMany(variant => Mismatches(CosmoResources.Colors(color), $"{color} {variant}", variant)))
            .ToList());

        Assert.Empty(wrong);
    }

    private static IEnumerable<string> Mismatches(ResourceDictionary dictionary, string what, ThemeVariant variant)
    {
        foreach (var (key, token, alpha) in Keys)
        {
            var expected = Token(dictionary, token, variant, alpha);
            if (!dictionary.TryGetResource(key, variant, out var value) || value is not string css || !CssColor.Parse(css).Near(expected))
            {
                yield return $"{what} {key}: {value}, expected {expected}";
            }
        }

        for (var index = 1; index <= 5; index++)
        {
            foreach (var (stop, alpha) in new[] { ("Top", 0.8), ("Bottom", 0.1) })
            {
                var expected = Token(dictionary, $"Chart{index}", variant, alpha);
                if (!dictionary.TryGetResource($"CosmoChart{index}{stop}Color", variant, out var value) || value is not Color color || !new CssColor(color.R, color.G, color.B, color.A / 255.0).Near(expected))
                {
                    yield return $"{what} CosmoChart{index}{stop}Color: {value}, expected {expected}";
                }
            }
        }

        var border = Token(dictionary, "Border", variant, 0.5);
        if (!dictionary.TryGetResource("CosmoChartTooltipBorderBrush", variant, out var brush) || brush is not ISolidColorBrush solid || !Windows.Paint(solid).Near(border))
        {
            yield return $"{what} CosmoChartTooltipBorderBrush: {brush}, expected {border}";
        }
    }

    private static CssColor Token(ResourceDictionary dictionary, string token, ThemeVariant variant, double alpha) =>
        dictionary.TryGetResource($"Cosmo{token}Color", variant, out var value) && value is Color color
            ? new CssColor(color.R, color.G, color.B, color.A / 255.0 * alpha)
            : throw new InvalidOperationException($"No Cosmo{token}Color");
}
