using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Dates;

public sealed class DateFidelityTests(DesktopSession desktop)
{
    /// <summary>Each calendar fixture and its floor on the desktop.</summary>
    public static readonly (string Fixture, double Floor)[] Fixtures =
    [
        ("calendar-single", 0.97), ("calendar-range", 0.97), ("calendar-multiple", 0.97), ("calendar-disabled", 0.97), ("calendar-outside-hidden", 0.97),
        ("calendar-dropdown", 0.97), ("calendar-week-numbers", 0.97),
        ("date-picker-demo", 0.97), ("date-picker-open", 0.96), ("date-picker-input", 0.97), ("date-picker-range-open", 0.95), ("date-picker-presets-open", 0.96),
    ];

    [Fact]
    [Trait("AC", "FIDELITY-11")]
    public async Task EveryCalendarFixtureMatchesShadcnsRenderOnTheDesktopAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var (variant, theme) in new[] { (ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark") })
        {
            foreach (var (fixture, floor) in Fixtures)
            {
                var (matched, size, expected) = await desktop.RunAsync<(double, (int, int), (int, int))>(() =>
                {
                    var window = DisplayFixtures.Show(fixture, variant);
                    DisplayFixtures.Open(window, fixture);
                    using var ours = Windows.Capture(window);
                    using var reference = Pixels.Reference(fixture, theme);
                    if (Environment.GetEnvironmentVariable("COSMO_CAPTURES") is { Length: > 0 } folder)
                    {
                        using var file = File.Create(Path.Combine(folder, $"{fixture}.{theme}.png"));
                        ours.Encode(file, SkiaSharp.SKEncodedImageFormat.Png, 100);
                    }

                    window.Close();
                    return (Pixels.Matched(ours, reference), (ours.Width, ours.Height), (reference.Width, reference.Height));
                });
                verdicts.Add($"{fixture}.{theme}: {matched:P2}");
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
