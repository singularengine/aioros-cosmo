using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

/// <summary>An overlay surface's entrance: the look it is shown at, the length and easing of its transitions, and its transform's origin.</summary>
internal sealed record Entrance(double Opacity, double Scale, RelativePoint Offset, int Milliseconds, double[] Easing, RelativePoint Origin);

public sealed class MotionTests(DesktopSession desktop)
{
    private static readonly double[] Ease = [0.25, 0.1, 0.25, 1];

    private static readonly RelativePoint Top = new(0.5, 0, RelativeUnit.Relative);

    private static readonly RelativePoint Start = new(0, 0, RelativeUnit.Relative);

    private static readonly RelativePoint Bottom = new(0.5, 1, RelativeUnit.Relative);

    /// <summary>tw-animate-css's <c>fade-in-0 zoom-in-95</c> with a <c>slide-in-from-*-2</c>, over its 150 ms with <c>ease</c>.</summary>
    private static Entrance Pops(double x, double y, RelativePoint origin) => new(0, 0.95, new RelativePoint(x, y, RelativeUnit.Absolute), 150, Ease, origin);

    /// <summary>A slide over the surface's own size (<c>slide-in-from-right</c>'s 100%), with no fade or zoom.</summary>
    private static Entrance Slides(double x, double y, int milliseconds, double[] easing) => new(double.NaN, double.NaN, new RelativePoint(x, y, RelativeUnit.Relative), milliseconds, easing, RelativePoint.Center);

    /// <summary>
    /// Shows a fixture with its transitions running, so an overlay opens as it does on screen, and returns what differs
    /// from <paramref name="expected"/> on the surface <paramref name="surface"/> finds: as it is shown, its transitions, and
    /// once they have run; then shows it again in a window asking for less motion, where it opens at rest.
    /// </summary>
    private Task<List<string>> EntersAsync(string what, Func<bool, Window> show, Func<Window, Control> surface, Entrance expected) =>
        desktop.RunAsync(() =>
        {
            var errors = new List<string>();
            Windows.Holding = false;
            Windows.Clocked = true;
            try
            {
                var window = show(false);
                var shown = surface(window);
                Windows.Clock!.Pulse(TimeSpan.Zero);
                errors.AddRange(Shown(what, shown, expected));
                errors.AddRange(Transitions(what, shown, expected));
                Windows.Clock.Pulse(TimeSpan.FromMilliseconds(expected.Milliseconds));
                errors.AddRange(Rest(what, shown));
                window.Close();
                var reduced = show(true);
                Windows.Clock.Pulse(TimeSpan.Zero);
                errors.AddRange(Rest(what, surface(reduced)).Select(error => error + " with c:Motion.IsReduced"));
                reduced.Close();
            }
            finally
            {
                Windows.Holding = true;
                Windows.Clocked = false;
            }

            return errors;
        });

    private static IEnumerable<string> Shown(string what, Control surface, Entrance expected)
    {
        var matrix = surface.RenderTransform?.Value ?? Matrix.Identity;
        var opacity = double.IsNaN(expected.Opacity) ? 1 : expected.Opacity;
        var scale = double.IsNaN(expected.Scale) ? 1 : expected.Scale;
        var offset = expected.Offset.ToPixels(surface.Bounds.Size);
        if (Math.Abs(surface.Opacity - opacity) > 0.01 || Math.Abs(matrix.M11 - scale) > 0.001 || Math.Abs(matrix.M22 - scale) > 0.001
            || Math.Abs(matrix.M31 - offset.X) > 0.5 || Math.Abs(matrix.M32 - offset.Y) > 0.5)
        {
            yield return $"{what} shown at opacity {surface.Opacity}, {matrix}; expected opacity {opacity}, scale {scale}, offset {offset}";
        }

        if (surface.RenderTransformOrigin != expected.Origin)
        {
            yield return $"{what} origin {surface.RenderTransformOrigin}, expected {expected.Origin}";
        }
    }

    private static IEnumerable<string> Transitions(string what, Control surface, Entrance expected)
    {
        var properties = new List<AvaloniaProperty>();
        if (!double.IsNaN(expected.Opacity))
        {
            properties.Add(Visual.OpacityProperty);
        }

        if (!double.IsNaN(expected.Scale) || expected.Offset != default)
        {
            properties.Add(Visual.RenderTransformProperty);
        }

        foreach (var property in properties)
        {
            var transition = (surface.Transitions ?? []).OfType<TransitionBase>().FirstOrDefault(candidate => candidate.Property == property);
            if (transition is null || transition.Duration != TimeSpan.FromMilliseconds(expected.Milliseconds) || transition.Easing is not SplineEasing spline
                || !new[] { spline.X1, spline.Y1, spline.X2, spline.Y2 }.SequenceEqual(expected.Easing))
            {
                yield return $"{what} {property.Name} transition {transition?.Duration} {transition?.Easing}, expected {expected.Milliseconds} ms ({string.Join(",", expected.Easing)})";
            }
        }
    }

    private static IEnumerable<string> Rest(string what, Control surface)
    {
        if (surface.Opacity != 1 || surface.RenderTransform?.Value is { IsIdentity: false })
        {
            yield return $"{what} at rest at opacity {surface.Opacity}, {surface.RenderTransform?.Value}";
        }
    }

    private static Window Overlay(Func<OverlayFixture> fixture, bool reduced)
    {
        var made = fixture();
        var window = Windows.Fixture(made.Content, made.Height, ThemeVariant.Light, made.Host);
        Motion.SetIsReduced(window, reduced);
        window.Styles.AddRange(made.Styles);
        Windows.Settle();
        made.Open(window);
        return window;
    }

    private static Window Display(string name, bool reduced, Action<Window> open)
    {
        var window = DisplayFixtures.Show(name, ThemeVariant.Light);
        Motion.SetIsReduced(window, reduced);
        open(window);
        return window;
    }

    private static Window Plain(Control content, bool reduced, Action<Window> open, bool host = false)
    {
        var window = Windows.Fixture(content, 600, ThemeVariant.Light, host);
        Motion.SetIsReduced(window, reduced);
        open(window);
        return window;
    }

    private static Control Root(Control surface) => surface.GetVisualChildren().OfType<Control>().First();

    private static T Visible<T>(Window window, string? name = null)
        where T : Control => window.GetVisualDescendants().OfType<T>().First(control => control.IsEffectivelyVisible && (name is null || control.Classes.Contains(name)));

    private static Window Sheet(SheetSide side, bool reduced)
    {
        var sheet = new Sheet { Side = side, Content = new DialogHeader { Title = "Edit profile", Description = "Make changes to your profile here." } };
        var button = OverlayFixtures.Outline("Open");
        button.Click += (_, _) => sheet.IsOpen = true;
        return Plain(new Panel { Children = { button, sheet } }, reduced, window => Windows.Click(window, button), host: true);
    }

    [Fact]
    [Trait("AC", "POPOVER-05")]
    public async Task APopoverFadesZoomsAndSlidesInAsync()
    {
        Assert.Empty((await EntersAsync("popover", reduced => Overlay(OverlayFixtures.PopoverOpen, reduced), window => Root(PopoverTests.Presenter(window)), Pops(0, -8, Top)))
            .Concat(await EntersAsync("combobox", reduced => Plain(new Search.Combobox { Items = { "Next.js", "SvelteKit" } }, reduced, window => Windows.Click(window, window.GetVisualDescendants().OfType<Button>().First())), window => Root(Visible<FlyoutPresenter>(window, "combobox")), Pops(0, -8, Top))));
    }

    [Fact]
    [Trait("AC", "HOVERCARD-05")]
    public async Task AHoverCardFadesZoomsAndSlidesInAsync() =>
        Assert.Empty(await EntersAsync("hover card", reduced => Overlay(OverlayFixtures.HoverCardOpen, reduced), window => Root(Visible<FlyoutPresenter>(window, "hover-card")), Pops(0, -8, Top)));

    [Fact]
    [Trait("AC", "DROPDOWN-06")]
    public async Task ADropdownMenuAndItsSubMenuFadeZoomAndSlideInAsync() =>
        Assert.Empty((await EntersAsync("dropdown menu", reduced => Overlay(OverlayFixtures.DropdownMenuOpen, reduced), window => Root(Visible<MenuFlyoutPresenter>(window)), Pops(0, -8, Start)))
            .Concat(await EntersAsync("sub-menu", reduced =>
            {
                var window = Overlay(OverlayFixtures.DropdownMenuOpen, reduced);
                Visible<MenuFlyoutPresenter>(window).GetVisualDescendants().OfType<MenuItem>().First(item => item.Header as string == "Invite users").IsSubMenuOpen = true;
                Windows.Settle();
                return window;
            }, window => Windows.Part<Popup>(Visible<MenuFlyoutPresenter>(window).GetVisualDescendants().OfType<MenuItem>().First(item => item.Header as string == "Invite users"), "PART_Popup").Child as Control ?? window, Pops(-8, 0, Start))));

    [Fact]
    [Trait("AC", "CONTEXTMENU-05")]
    public async Task AContextMenuFadesZoomsAndSlidesInFromTheLeftAsync() =>
        Assert.Empty(await EntersAsync("context menu", reduced => Overlay(OverlayFixtures.ContextMenuOpen, reduced), window => Root(Visible<ContextMenu>(window)), Pops(-8, 0, Start)));

    [Fact]
    [Trait("AC", "MENUBAR-06")]
    public async Task AMenubarsMenuFadesZoomsAndSlidesInAsync() =>
        Assert.Empty(await EntersAsync("menubar menu", reduced => Display("menubar-demo", reduced, window => Windows.Click(window, window.GetVisualDescendants().OfType<MenuItem>().First())),
            window => Windows.Part<Popup>(window.GetVisualDescendants().OfType<MenuItem>().First(), "PART_Popup").Child as Control ?? window, Pops(0, -8, Start)));

    [Fact]
    [Trait("AC", "SELECT-05")]
    public async Task ASelectsListFadesAndZoomsInAsync() =>
        Assert.Empty(await EntersAsync("select", reduced =>
        {
            var combo = new ComboBox { Width = 180, Items = { new ComboBoxItem { Content = "Apple" }, new ComboBoxItem { Content = "Banana" } } };
            return Plain(combo, reduced, _ =>
            {
                combo.IsDropDownOpen = true;
                Windows.Settle();
            });
        }, window => Windows.Part<Popup>(window.GetVisualDescendants().OfType<ComboBox>().First(), "PART_Popup").Child as Control ?? window, new Entrance(0, 0.95, default, 150, Ease, Top)));

    [Fact]
    [Trait("AC", "TOOLTIP-05")]
    public async Task ATooltipFadesZoomsAndSlidesUpAsync() =>
        Assert.Empty(await EntersAsync("tooltip", reduced => Overlay(OverlayFixtures.TooltipOpen, reduced), window => Root(Visible<ToolTip>(window)), Pops(0, 8, Bottom)));

    [Fact]
    [Trait("AC", "NAVMENU-06")]
    public async Task ANavigationMenusContentFadesAndZoomsInAsync() =>
        Assert.Empty(await EntersAsync("navigation menu", reduced => Display("navigation-menu-demo", reduced, window => Windows.Click(window, window.GetVisualDescendants().OfType<Navigation.NavigationMenuTrigger>().First())),
            window => Root(Visible<FlyoutPresenter>(window, "navigation-menu")), new Entrance(0, 0.95, default, 200, Ease, RelativePoint.Center)));

    [Fact]
    [Trait("AC", "DIALOG-06")]
    public async Task ADialogFadesAndZoomsInOverItsFadingMaskAsync() =>
        Assert.Empty((await EntersAsync("dialog", reduced => Overlay(OverlayFixtures.DialogOpen, reduced), window => Visible<DialogSurface>(window), new Entrance(0, 0.95, default, 200, Ease, RelativePoint.Center)))
            .Concat(await EntersAsync("mask", reduced => Overlay(OverlayFixtures.DialogOpen, reduced), window => Visible<Border>(window, "cosmo-overlay"), new Entrance(0, double.NaN, default, 150, Ease, RelativePoint.Center))));

    [Fact]
    [Trait("AC", "ALERTDIALOG-05")]
    public async Task AnAlertDialogFadesAndZoomsInAsync() =>
        Assert.Empty(await EntersAsync("alert dialog", reduced => Overlay(OverlayFixtures.AlertDialogOpen, reduced), window => Visible<DialogSurface>(window), new Entrance(0, 0.95, default, 200, Ease, RelativePoint.Center)));

    [Fact]
    [Trait("AC", "SHEET-05")]
    public async Task ASheetSlidesInFromItsSideAsync()
    {
        double[] easing = [0.4, 0, 0.2, 1];
        var sides = new (SheetSide Side, double X, double Y)[] { (SheetSide.Right, 1, 0), (SheetSide.Left, -1, 0), (SheetSide.Top, 0, -1), (SheetSide.Bottom, 0, 1) };
        var wrong = new List<string>();
        foreach (var (side, x, y) in sides)
        {
            wrong.AddRange(await EntersAsync($"{side} sheet", reduced => Sheet(side, reduced), window => Visible<DialogSurface>(window), Slides(x, y, 500, easing)));
        }

        wrong.AddRange(await EntersAsync("sheet mask", reduced => Sheet(SheetSide.Right, reduced), window => Visible<Border>(window, "cosmo-overlay"), new Entrance(0, double.NaN, default, 150, Ease, RelativePoint.Center)));
        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "DRAWER-05")]
    public async Task ADrawerSlidesUpAsVaulsAsync() =>
        Assert.Empty(await EntersAsync("drawer", reduced => Overlay(OverlayFixtures.DrawerOpen, reduced), window => Visible<DialogSurface>(window), Slides(0, 1, 500, [0.32, 0.72, 0, 1])));

    [Fact]
    [Trait("AC", "SONNER-05")]
    public async Task AToastRisesAndTheStackFollowsAsSonnersAsync()
    {
        var wrong = await EntersAsync("toast", reduced => Overlay(OverlayFixtures.SonnerStack, reduced), window => Windows.Part<Border>(window.GetVisualDescendants().OfType<NotificationCard>().Last(), "PART_Root"),
            new Entrance(0, double.NaN, new RelativePoint(0, 1, RelativeUnit.Relative), 400, Ease, RelativePoint.Center));
        var stack = await desktop.RunAsync(() =>
        {
            Windows.Holding = false;
            Windows.Clocked = true;
            var window = Overlay(OverlayFixtures.SonnerStack, false);
            Windows.Clock!.Pulse(TimeSpan.Zero);
            Windows.Clock.Pulse(TimeSpan.FromMilliseconds(400));
            (Windows.Holding, Windows.Clocked) = (true, false);
            var cards = window.GetVisualDescendants().OfType<NotificationCard>().ToList();
            var errors = cards.SelectMany((card, index) => Transitions($"card {index}", card, new Entrance(0, 1, new RelativePoint(0, 1, RelativeUnit.Absolute), 400, Ease, RelativePoint.Center))).ToList();
            var behind = cards[1].RenderTransform?.Value ?? Matrix.Identity;
            if (Math.Abs(behind.M11 - 0.95) > 0.001 || Math.Abs(behind.M32 + 14) > 0.5)
            {
                errors.Add($"the toast behind the front one at {behind}, expected 95% and 14 px up");
            }

            window.Close();
            return errors;
        });
        Assert.Empty(wrong.Concat(stack));
    }
}
