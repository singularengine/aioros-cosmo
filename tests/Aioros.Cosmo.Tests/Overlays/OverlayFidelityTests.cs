using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class OverlayFidelityTests(DesktopSession desktop)
{
    /// <summary>Each fixture, its floor and whether the pointer stays on the trigger once the overlay is open (tooltips, hover cards).</summary>
    private static readonly (Func<OverlayFixture> Build, double Floor, bool Stays)[] Fixtures =
    [
        (OverlayFixtures.PopoverOpen, 0.94, false),
        (OverlayFixtures.TooltipOpen, 0.96, true),
        (OverlayFixtures.KbdTooltipOpen, 0.98, true),
        (OverlayFixtures.HoverCardOpen, 0.93, true),
        (OverlayFixtures.DropdownMenuOpen, 0.95, false),
        (OverlayFixtures.DropdownMenuCheckboxes, 0.95, false),
        (OverlayFixtures.ContextMenuOpen, 0.95, false),
        (OverlayFixtures.DialogOpen, 0.94, false),
        (OverlayFixtures.AlertDialogOpen, 0.94, false),
        (OverlayFixtures.SheetOpen, 0.94, false),
        (OverlayFixtures.SheetTop, 0.94, false),
        (OverlayFixtures.DrawerOpen, 0.94, false),
        (OverlayFixtures.SonnerStack, 0.94, false),
        (OverlayFixtures.SonnerTypes, 0.94, false),
        (OverlayFixtures.SonnerClose, 0.94, false),
    ];

    [Fact]
    [Trait("AC", "FIDELITY-06")]
    public async Task EveryOverlayFixtureMatchesShadcnsRenderOnTheDesktopAsync()
    {
        var verdicts = new List<string>();
        var wrong = new List<string>();
        foreach (var (variant, theme) in new[] { (ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark") })
        {
            foreach (var (build, floor, stays) in Fixtures)
            {
                var (name, matched, size, expected) = await desktop.RunAsync<(string, double, (int, int), (int, int))>(() =>
                {
                    var fixture = build();
                    var window = OverlayFixtures.Show(fixture, variant);
                    if (!stays)
                    {
                        Windows.Away(window, new Point(2, fixture.Height - 2));
                    }

                    window.FocusManager?.Focus(null);
                    using var ours = Windows.Capture(window);
                    using var reference = Pixels.Reference(fixture.Name, theme);
                    window.Close();
                    return (fixture.Name, Pixels.Matched(ours, reference), (ours.Width, ours.Height), (reference.Width, reference.Height));
                });
                verdicts.Add($"{name}.{theme}: {matched:P2}");
                if (size != expected || matched < floor)
                {
                    wrong.Add($"{name}.{theme}: {size} (shadcn {expected}), {matched:P2} of pixels within {Pixels.Tolerance}/255, floor {floor:P0}");
                }
            }
        }

        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, verdicts));
        Assert.Empty(wrong);
    }
}
