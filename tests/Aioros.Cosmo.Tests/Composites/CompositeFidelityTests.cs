using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Aioros.Cosmo.Tests.Display;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class CompositeFidelityTests(DesktopSession desktop)
{
    /// <summary>Each composite fixture and its floor on the desktop.</summary>
    public static readonly (string Fixture, double Floor)[] Fixtures =
    [
        ("resizable-demo", 0.97), ("resizable-handle", 0.97), ("resizable-vertical", 0.97), ("carousel-demo", 0.97), ("carousel-api", 0.97), ("carousel-orientation", 0.97),
        ("command-demo", 0.95), ("command-filtered", 0.95), ("command-dialog-open", 0.96), ("combobox-demo", 0.97), ("combobox-open", 0.95),
        ("data-table-demo", 0.95), ("data-table-selected", 0.95),
        ("sidebar-demo", 0.96), ("sidebar-icon", 0.97), ("sidebar-floating", 0.96), ("sidebar-inset", 0.96),
    ];

    [Fact]
    [Trait("AC", "FIDELITY-10")]
    public async Task EveryCompositeFixtureMatchesShadcnsRenderOnTheDesktopAsync()
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
