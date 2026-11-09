using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Display;

public sealed class DisplayFidelityTests(DesktopSession desktop)
{
    /// <summary>Each display and navigation fixture and its floor on the desktop.</summary>
    public static readonly (string Fixture, double Floor)[] Fixtures =
    [
        ("separator-demo", 0.96), ("skeleton-demo", 0.98), ("spinner-demo", 0.96), ("kbd-demo", 0.96), ("avatar-demo", 0.97), ("avatar-badge", 0.98), ("avatar-group-count", 0.98), ("badge-links", 0.94), ("card-separated", 0.97), ("alert-demo", 0.95),
        ("aspect-ratio-demo", 0.97), ("typography-headings", 0.92), ("typography-text", 0.92), ("empty-demo", 0.96), ("empty-outline", 0.96),
        ("item-demo", 0.93), ("item-variant", 0.92), ("item-header-footer", 0.96), ("input-file", 0.97),
        ("tabs-demo", 0.96), ("tabs-list", 0.94), ("tabs-line", 0.97), ("tabs-vertical", 0.97), ("tabs-vertical-line", 0.98), ("accordion-demo", 0.93), ("collapsible-demo", 0.96), ("collapsible-open", 0.96), ("breadcrumb-demo", 0.95),
        ("breadcrumb-separator", 0.95), ("pagination-demo", 0.96), ("button-group-demo", 0.95), ("button-group-size", 0.95), ("button-group-orientation", 0.95),
        ("scroll-area-demo", 0.96), ("scroll-area-horizontal", 0.97), ("table-demo", 0.94), ("navigation-menu-demo", 0.97), ("navigation-menu-open", 0.89),
        ("menubar-demo", 0.97), ("menubar-open", 0.96),
    ];

    [Fact]
    [Trait("AC", "FIDELITY-08")]
    public async Task EveryDisplayFixtureMatchesShadcnsRenderOnTheDesktopAsync()
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
