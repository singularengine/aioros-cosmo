using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Charts;

public sealed class ChartFidelityTests(DesktopSession desktop)
{
    /// <summary>Each chart fixture and its floor on the desktop.</summary>
    public static readonly (string Fixture, double Floor)[] Fixtures =
    [
        ("chart-area-default", 0.980),
        ("chart-area-gradient", 0.980),
        ("chart-area-stacked", 0.980),
        ("chart-area-interactive", 0.960),
        ("chart-bar-default", 0.980),
        ("chart-bar-multiple", 0.975),
        ("chart-bar-horizontal", 0.975),
        ("chart-bar-label", 0.975),
        ("chart-bar-interactive", 0.965),
        ("chart-line-default", 0.980),
        ("chart-line-multiple", 0.975),
        ("chart-line-dots", 0.980),
        ("chart-line-interactive", 0.965),
        ("chart-pie-simple", 0.980),
        ("chart-pie-donut", 0.980),
        ("chart-pie-donut-text", 0.975),
        ("chart-pie-legend", 0.975),
        ("chart-radar-default", 0.975),
        ("chart-radar-dots", 0.975),
        ("chart-radar-multiple", 0.975),
        ("chart-radial-simple", 0.980),
        ("chart-radial-text", 0.975),
        ("chart-radial-stacked", 0.975),
        ("chart-tooltip-default", 0.980),
        ("chart-tooltip-indicator-line", 0.980),
        ("chart-tooltip-indicator-none", 0.980),
    ];

    [Fact]
    [Trait("AC", "FIDELITY-13")]
    public async Task EveryChartFixtureMatchesShadcnsRenderOnTheDesktopAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var (variant, theme) in new[] { (ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark") })
        {
            foreach (var (fixture, floor) in Fixtures)
            {
                var (matched, size, expected, recharts2) = await desktop.RunAsync<(double, (int, int), (int, int), double)>(() =>
                {
                    ChartAccessors.Register();
                    var window = DisplayFixtures.Show(fixture, variant);
                    using var ours = Windows.Capture(window);
                    using var reference = Pixels.Reference(fixture, theme);
                    if (Environment.GetEnvironmentVariable("COSMO_CAPTURES") is { Length: > 0 } folder)
                    {
                        using var file = File.Create(Path.Combine(folder, $"{fixture}.{theme}.png"));
                        ours.Encode(file, SkiaSharp.SKEncodedImageFormat.Png, 100);
                    }

                    window.Close();
                    using var older = Pixels.Reference(fixture + ".recharts2", theme);
                    return (Pixels.Matched(ours, reference), (ours.Width, ours.Height), (reference.Width, reference.Height), Pixels.Matched(ours, older));
                });
                verdicts.Add($"{fixture}.{theme}: {matched:P2} (with Recharts 2.15.4: {recharts2:P2})");
                if (size != expected || matched < floor)
                {
                    wrong.Add($"{fixture}.{theme}: {size} (shadcn {expected}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.Empty(wrong);
    }
}
