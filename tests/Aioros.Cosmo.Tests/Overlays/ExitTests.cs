using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

/// <summary>An overlay part's exit: the look it leaves for, the length and easing of its transitions, and its transform's origin.</summary>
internal sealed record Exit(double Opacity, double Scale, RelativePoint Offset, int Milliseconds, double[] Easing, RelativePoint Origin);

public sealed class ExitTests(DesktopSession desktop)
{
    private static readonly double[] Ease = [0.25, 0.1, 0.25, 1];

    private static readonly RelativePoint Top = new(0.5, 0, RelativeUnit.Relative);

    private static readonly RelativePoint Start = new(0, 0, RelativeUnit.Relative);

    private static readonly RelativePoint Bottom = new(0.5, 1, RelativeUnit.Relative);

    /// <summary>tw-animate-css's <c>animate-out fade-out-0 zoom-out-95</c> over <paramref name="milliseconds"/> with <c>ease</c>.</summary>
    private static Exit Fades(RelativePoint origin, int milliseconds = 150, double x = 0) => new(0, 0.95, new RelativePoint(x, 0, RelativeUnit.Absolute), milliseconds, Ease, origin);

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

    private static List<Image> Ghosts(Window window) => [.. OverlayLayer.GetOverlayLayer(window)?.Children.OfType<Image>() ?? []];

    /// <summary>Runs <paramref name="body"/> with windows on a still clock and transitions running, as a screen plays them.</summary>
    private Task<List<string>> ClockedAsync(Func<List<string>> body) =>
        desktop.RunAsync(() =>
        {
            Windows.Holding = false;
            Windows.Clocked = true;
            try
            {
                return body();
            }
            finally
            {
                Windows.Holding = true;
                Windows.Clocked = false;
            }
        });

    private static void None(IEnumerable<string> errors)
    {
        var all = errors.ToList();
        Assert.True(all.Count == 0, string.Join(Environment.NewLine, all));
    }

    private static void Pulse(int milliseconds) => Windows.Clock!.Pulse(TimeSpan.FromMilliseconds(milliseconds));

    /// <summary>
    /// Opens a popup's overlay, lets its entrance end, closes it and returns what differs from <paramref name="expected"/>
    /// on the pictures its surfaces leave as: one per surface, where the surface stood, passing clicks through, with its
    /// transitions and origin, at its exit look once they have run and gone after; then none when motion is reduced.
    /// </summary>
    private Task<List<string>> LeavesAsync(string what, Func<bool, Window> show, Func<Window, IReadOnlyList<Control>> surfaces, Action<Window> close, Exit expected) =>
        ClockedAsync(() =>
        {
            var errors = new List<string>();
            var window = show(false);
            Pulse(0);
            Pulse(600);
            var boxes = surfaces(window).Select(surface => Windows.Box(surface, window)).ToList();
            close(window);
            Pulse(0);
            var ghosts = Ghosts(window);
            errors.AddRange(Placed(what, window, ghosts, boxes, expected));
            Pulse(expected.Milliseconds);
            errors.AddRange(ghosts.SelectMany(ghost => Left(what, ghost, expected)));
            Pulse(1);
            errors.AddRange(Ghosts(window).Count == 0 ? [] : [$"{what}: {Ghosts(window).Count} pictures left after the exit"]);
            window.Close();
            var reduced = show(true);
            Pulse(0);
            close(reduced);
            Pulse(0);
            errors.AddRange(Ghosts(reduced).Count == 0 ? [] : [$"{what}: a picture leaves with c:Motion.IsReduced"]);
            reduced.Close();
            return errors;
        });

    /// <summary>The pictures leaving, one where each surface stood, passing clicks through, with the surface's transitions and origin.</summary>
    private static IEnumerable<string> Placed(string what, Window window, List<Image> ghosts, List<Rect> boxes, Exit expected)
    {
        if (ghosts.Count != boxes.Count)
        {
            yield return $"{what}: {ghosts.Count} pictures leaving, expected {boxes.Count}";
        }

        foreach (var box in boxes)
        {
            if (ghosts.MinBy(candidate => Math.Abs(Windows.Box(candidate, window).X - box.X) + Math.Abs(Windows.Box(candidate, window).Y - box.Y)) is not { } ghost)
            {
                continue;
            }

            var at = Windows.Box(ghost, window);
            if (Math.Abs(at.X - box.X) > 0.5 || Math.Abs(at.Y - box.Y) > 0.5 || Math.Abs(at.Width - box.Width) > 0.5 || Math.Abs(at.Height - box.Height) > 0.5)
            {
                yield return $"{what} picture at {at}, the surface stood at {box}";
            }

            if (ghost.IsHitTestVisible)
            {
                yield return $"{what} picture takes clicks";
            }

            foreach (var error in Leaving(what, ghost, expected))
            {
                yield return error;
            }
        }
    }

    /// <summary>A part's transitions and origin as it starts to leave.</summary>
    private static IEnumerable<string> Leaving(string what, Control part, Exit expected)
    {
        if (part.RenderTransformOrigin != expected.Origin)
        {
            yield return $"{what} origin {part.RenderTransformOrigin}, expected {expected.Origin}";
        }

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
            var transition = (part.Transitions ?? []).OfType<TransitionBase>().FirstOrDefault(candidate => candidate.Property == property);
            if (transition is null || transition.Duration != TimeSpan.FromMilliseconds(expected.Milliseconds) || transition.Easing is not SplineEasing spline
                || !new[] { spline.X1, spline.Y1, spline.X2, spline.Y2 }.SequenceEqual(expected.Easing))
            {
                yield return $"{what} {property.Name} transition {transition?.Duration} {transition?.Easing}, expected {expected.Milliseconds} ms ({string.Join(",", expected.Easing)})";
            }
        }
    }

    /// <summary>A part's look once its exit has played.</summary>
    private static IEnumerable<string> Left(string what, Control part, Exit expected)
    {
        var matrix = part.RenderTransform?.Value ?? Matrix.Identity;
        var opacity = double.IsNaN(expected.Opacity) ? 1 : expected.Opacity;
        var scale = double.IsNaN(expected.Scale) ? 1 : expected.Scale;
        var offset = expected.Offset.ToPixels(part.Bounds.Size);
        if (Math.Abs(part.Opacity - opacity) > 0.01 || Math.Abs(matrix.M11 - scale) > 0.001 || Math.Abs(matrix.M22 - scale) > 0.001
            || Math.Abs(matrix.M31 - offset.X) > 0.5 || Math.Abs(matrix.M32 - offset.Y) > 0.5)
        {
            yield return $"{what} left at opacity {part.Opacity}, {matrix}; expected opacity {opacity}, scale {scale}, offset {offset}";
        }
    }

    /// <summary>
    /// Opens an overlay Cosmo adds itself, lets it enter, closes it and returns what differs: its parts stay, passing
    /// clicks through and <c>:closing</c>, with their transitions, reach their exit looks and are gone after the longest;
    /// with reduced motion they are gone at once.
    /// </summary>
    private Task<List<string>> StaysAsync(string what, Func<bool, Window> show, Func<Window, IReadOnlyList<Control>> parts, Action<Window> close, IReadOnlyList<Exit> expected, Func<Window, IEnumerable<string>>? meanwhile = null) =>
        ClockedAsync(() =>
        {
            var errors = new List<string>();
            var window = show(false);
            Pulse(0);
            Pulse(600);
            var leaving = parts(window);
            close(window);
            Pulse(0);
            foreach (var (part, exit) in leaving.Zip(expected))
            {
                errors.AddRange(part.IsAttachedToVisualTree() ? [] : [$"{what} {part.GetType().Name} gone at once"]);
                errors.AddRange(part.IsHitTestVisible ? [$"{what} {part.GetType().Name} takes clicks while it leaves"] : []);
                errors.AddRange(part.Classes.Contains(":closing") ? [] : [$"{what} {part.GetType().Name} is not :closing"]);
                errors.AddRange(Leaving($"{what} {part.GetType().Name}", part, exit));
            }

            errors.AddRange(meanwhile?.Invoke(window) ?? []);
            var longest = expected.Max(exit => exit.Milliseconds);
            Pulse(longest - 1);
            errors.AddRange(leaving.Zip(expected).SelectMany(pair => Left($"{what} {pair.First.GetType().Name}", pair.First, pair.Second)));
            Pulse(2);
            errors.AddRange(leaving.Where(part => part.IsAttachedToVisualTree()).Select(part => $"{what} {part.GetType().Name} still there after {longest} ms"));
            window.Close();
            var reduced = show(true);
            Pulse(0);
            var gone = parts(reduced);
            close(reduced);
            Pulse(0);
            errors.AddRange(gone.Where(part => part.IsAttachedToVisualTree()).Select(part => $"{what} {part.GetType().Name} stays with c:Motion.IsReduced"));
            reduced.Close();
            return errors;
        });

    [Fact]
    [Trait("AC", "POPOVER-07")]
    public async Task APopoverLeavesAsAFadingPictureAsync()
    {
        None((await LeavesAsync("popover", reduced => Overlay(OverlayFixtures.PopoverOpen, reduced), window => [Root(PopoverTests.Presenter(window))], window => Windows.Key(window, Key.Escape), Fades(Top)))
            .Concat(await LeavesAsync("combobox", reduced => Plain(new Search.Combobox { Items = { "Next.js", "SvelteKit" } }, reduced, window => Windows.Click(window, window.GetVisualDescendants().OfType<Button>().First())),
                window => [Root(Visible<FlyoutPresenter>(window, "combobox"))], window => Windows.Key(window, Key.Escape), Fades(Top))));
    }

    [Fact]
    [Trait("AC", "HOVERCARD-07")]
    public async Task AHoverCardLeavesAsAFadingPictureAsync() =>
        None(await LeavesAsync("hover card", reduced => Overlay(OverlayFixtures.HoverCardOpen, reduced), window => [Root(Visible<FlyoutPresenter>(window, "hover-card"))],
            window =>
            {
                Windows.Away(window, new Point(620, 4));
                FlyoutBase.GetAttachedFlyout(window.GetVisualDescendants().OfType<HyperlinkButton>().First())!.Hide();
            }, Fades(Top)));

    [Fact]
    [Trait("AC", "DROPDOWN-08")]
    public async Task ADropdownMenuAndItsSubMenuLeaveAsFadingPicturesAsync() =>
        None(await LeavesAsync("dropdown menu", reduced =>
        {
            var window = Overlay(OverlayFixtures.DropdownMenuOpen, reduced);
            SubMenu(window).IsSubMenuOpen = true;
            Windows.Settle();
            return window;
        }, window => [Root(Visible<MenuFlyoutPresenter>(window)), Windows.Part<Popup>(SubMenu(window), "PART_Popup").Child!],
            window => window.GetVisualDescendants().OfType<Button>().First(button => button.Flyout is not null).Flyout!.Hide(), Fades(Start)));

    private static MenuItem SubMenu(Window window) => Visible<MenuFlyoutPresenter>(window).GetVisualDescendants().OfType<MenuItem>().First(item => item.Header as string == "Invite users");

    [Fact]
    [Trait("AC", "CONTEXTMENU-07")]
    public async Task AContextMenuLeavesAsAFadingPictureAsync() =>
        None(await LeavesAsync("context menu", reduced => Overlay(OverlayFixtures.ContextMenuOpen, reduced), window => [Root(Visible<ContextMenu>(window))], window => Windows.Key(window, Key.Escape), Fades(Start)));

    [Fact]
    [Trait("AC", "MENUBAR-08")]
    public async Task AMenubarsMenuLeavesAsAFadingPictureAsync() =>
        None(await LeavesAsync("menubar menu", reduced => Display("menubar-demo", reduced, window => Windows.Click(window, window.GetVisualDescendants().OfType<MenuItem>().First())),
            window => [Windows.Part<Popup>(window.GetVisualDescendants().OfType<MenuItem>().First(), "PART_Popup").Child!], window => Windows.Key(window, Key.Escape), Fades(Start)));

    [Fact]
    [Trait("AC", "SELECT-13")]
    public async Task ASelectsListLeavesAsAFadingPictureAsync() =>
        None(await LeavesAsync("select", reduced =>
        {
            var combo = new ComboBox { Width = 180, Items = { new ComboBoxItem { Content = "Apple" }, new ComboBoxItem { Content = "Banana" } } };
            return Plain(combo, reduced, _ =>
            {
                combo.IsDropDownOpen = true;
                Windows.Settle();
            });
        }, window => [Windows.Part<Popup>(window.GetVisualDescendants().OfType<ComboBox>().First(), "PART_Popup").Child!],
            window => window.GetVisualDescendants().OfType<ComboBox>().First().IsDropDownOpen = false, Fades(Top)));

    [Fact]
    [Trait("AC", "TOOLTIP-06")]
    public async Task ATooltipLeavesAsAFadingPictureAsync() =>
        None(await LeavesAsync("tooltip", reduced => Overlay(OverlayFixtures.TooltipOpen, reduced), window => [Root(Visible<ToolTip>(window))],
            window => ToolTip.SetIsOpen(window.GetVisualDescendants().OfType<Button>().First(), false), Fades(Bottom)));

    [Fact]
    [Trait("AC", "NAVMENU-08")]
    public async Task ANavigationMenusContentSlidesBetweenItemsAsync() =>
        None(await ClockedAsync(() =>
        {
            var errors = new List<string>();
            var window = Display("navigation-menu-demo", false, _ => { });
            var triggers = window.GetVisualDescendants().OfType<Navigation.NavigationMenuTrigger>().ToList();
            var menu = window.GetVisualDescendants().OfType<Navigation.NavigationMenu>().First();
            Windows.Click(window, triggers[0]);
            Pulse(0);
            Pulse(600);
            menu.Enter(triggers[1]);
            Windows.Settle();
            Pulse(0);
            errors.AddRange(Slid("forward", window, 208, -208));
            Pulse(600);
            menu.Enter(triggers[0]);
            Windows.Settle();
            Pulse(0);
            errors.AddRange(Slid("back", window, -208, 208));
            Pulse(600);
            triggers[1].Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Enter);
            Windows.Settle();
            Pulse(0);
            errors.AddRange(Slid("pressed", window, 208, -208));
            Pulse(600);
            triggers[1].Flyout!.Hide();
            Pulse(0);
            var alone = Ghosts(window).SingleOrDefault();
            Pulse(200);
            errors.AddRange(alone is null ? ["the content closing alone leaves no picture"] : Left("closing alone", alone, Fades(RelativePoint.Center, 200)));
            window.Close();
            return errors;
        }));

    /// <summary>The content entering from <paramref name="from"/> and a picture of the replaced one leaving toward <paramref name="to"/>, each with its fade and zoom over 200 ms.</summary>
    private static List<string> Slid(string what, Window window, double from, double to)
    {
        var errors = new List<string>();
        var entering = Root(Visible<FlyoutPresenter>(window, "navigation-menu"));
        var start = entering.RenderTransform?.Value ?? Matrix.Identity;
        errors.AddRange(Math.Abs(entering.Opacity) < 0.01 && Math.Abs(start.M11 - 0.95) < 0.001 && Math.Abs(start.M31 - from) < 0.5 ? [] : [$"{what}: the content enters at opacity {entering.Opacity}, {start}; expected 0, 95% and {from} px"]);
        var leaving = Ghosts(window).LastOrDefault();
        if (leaving is null)
        {
            errors.Add($"{what}: no picture of the replaced content");
            return errors;
        }

        Pulse(200);
        errors.AddRange(Left($"{what} leaving", leaving, Fades(RelativePoint.Center, 200, to)));
        return errors;
    }

    [Fact]
    [Trait("AC", "DIALOG-09")]
    public async Task ADialogAndItsMaskStayWhileTheyLeaveAsync()
    {
        var wrong = await StaysAsync("dialog", reduced => Overlay(OverlayFixtures.DialogOpen, reduced),
            window => [Visible<DialogSurface>(window), Visible<Border>(window, "cosmo-overlay")], window => Windows.Key(window, Key.Escape),
            [new Exit(0, 0.95, default, 200, Ease, RelativePoint.Center), new Exit(0, double.NaN, default, 150, Ease, RelativePoint.Center)],
            window => Windows.Focused(window) is Button { Content: "Open Dialog" } ? [] : [$"dialog: the focus is on {Windows.Focused(window)} while it leaves, not its trigger"]);
        var reopened = await ClockedAsync(() =>
        {
            var window = Overlay(OverlayFixtures.DialogOpen, false);
            Pulse(0);
            Pulse(600);
            var first = Visible<DialogSurface>(window);
            Windows.Key(window, Key.Escape);
            Pulse(0);
            Pulse(50);
            Windows.Click(window, window.GetVisualDescendants().OfType<Button>().First(button => button.Content as string == "Open Dialog"));
            Pulse(0);
            var surfaces = window.GetVisualDescendants().OfType<DialogSurface>().ToList();
            var errors = new List<string>();
            errors.AddRange(first.IsAttachedToVisualTree() ? ["dialog: the leaving surface stays once it is opened again"] : []);
            errors.AddRange(surfaces.Count == 1 && surfaces[0].Content is not null ? [] : [$"dialog opened again: {surfaces.Count} surfaces, content {surfaces.FirstOrDefault()?.Content}"]);
            window.Close();
            return errors;
        });
        None(wrong.Concat(reopened));
    }

    [Fact]
    [Trait("AC", "ALERTDIALOG-08")]
    public async Task AnAlertDialogStaysWhileItLeavesAsync() =>
        None(await StaysAsync("alert dialog", reduced => Overlay(OverlayFixtures.AlertDialogOpen, reduced), window => [Visible<DialogSurface>(window)],
            window => Windows.Key(window, Key.Escape), [new Exit(0, 0.95, default, 200, Ease, RelativePoint.Center)]));

    [Fact]
    [Trait("AC", "SHEET-08")]
    public async Task ASheetSlidesOutOfItsSideAsync()
    {
        double[] easing = [0.4, 0, 0.2, 1];
        var sides = new (SheetSide Side, double X, double Y)[] { (SheetSide.Right, 1, 0), (SheetSide.Left, -1, 0), (SheetSide.Top, 0, -1), (SheetSide.Bottom, 0, 1) };
        var wrong = new List<string>();
        foreach (var (side, x, y) in sides)
        {
            wrong.AddRange(await StaysAsync($"{side} sheet", reduced => Sheet(side, reduced), window => [Visible<Border>(window, "cosmo-overlay"), Visible<DialogSurface>(window)], window => Windows.Key(window, Key.Escape),
                [new Exit(0, double.NaN, default, 150, Ease, RelativePoint.Center), new Exit(double.NaN, double.NaN, new RelativePoint(x, y, RelativeUnit.Relative), 300, easing, RelativePoint.Center)]));
        }

        None(wrong);
    }

    private static Window Sheet(SheetSide side, bool reduced)
    {
        var sheet = new Sheet { Side = side, Content = new DialogHeader { Title = "Edit profile", Description = "Make changes to your profile here." } };
        var button = OverlayFixtures.Outline("Open");
        button.Click += (_, _) => sheet.IsOpen = true;
        return Plain(new Panel { Children = { button, sheet } }, reduced, window => Windows.Click(window, button), host: true);
    }

    [Fact]
    [Trait("AC", "DRAWER-08")]
    public async Task ADrawerSlidesDownAsVaulsAsync() =>
        None(await StaysAsync("drawer", reduced => Overlay(OverlayFixtures.DrawerOpen, reduced), window => [Visible<DialogSurface>(window)],
            window => Windows.Key(window, Key.Escape), [new Exit(double.NaN, double.NaN, new RelativePoint(0, 1, RelativeUnit.Relative), 500, [0.32, 0.72, 0, 1], RelativePoint.Center)]));

    [Fact]
    [Trait("AC", "SONNER-07")]
    public async Task AToastFallsAndTheStackFollowsAsync() =>
        None(await ClockedAsync(() => [.. Falls(reduced: false), .. Falls(reduced: true)]));

    private static List<string> Falls(bool reduced)
    {
        var fall = new Exit(0, double.NaN, new RelativePoint(0, 1, RelativeUnit.Relative), 400, Ease, RelativePoint.Center);
        var errors = new List<string>();
        var window = Overlay(OverlayFixtures.SonnerStack, reduced);
        Pulse(0);
        Pulse(600);
        var toaster = window.GetVisualDescendants().OfType<Toaster>().First();
        var cards = window.GetVisualDescendants().OfType<NotificationCard>().ToList();
        var front = Windows.Part<Border>(cards[^1], "PART_Root");
        toaster.Close(toaster.Toasts!.Cast<Toast>().Last());
        Pulse(0);
        Windows.Settle();
        Pulse(0);
        if (reduced)
        {
            errors.AddRange(cards[^1].IsAttachedToVisualTree() ? ["toast stays with c:Motion.IsReduced"] : []);
            window.Close();
            return errors;
        }

        errors.AddRange(cards[^1].IsAttachedToVisualTree() ? [] : ["toast gone at once"]);
        errors.AddRange(Leaving("toast", front, fall));
        Pulse(399);
        errors.AddRange(cards[^1].IsAttachedToVisualTree() ? [] : ["toast gone before its 400 ms exit played"]);
        errors.AddRange(Left("toast", front, fall));
        var next = cards[^2].RenderTransform?.Value ?? Matrix.Identity;
        errors.AddRange(Math.Abs(next.M11 - 1) < 0.001 && Math.Abs(next.M32) < 0.5 ? [] : [$"the toast behind at {next} once the front one left, expected at the front"]);
        Pulse(2);
        Windows.Settle();
        errors.AddRange(cards[^1].IsAttachedToVisualTree() ? ["toast still there after 400 ms"] : []);
        window.Close();
        return errors;
    }

    [Fact]
    [Trait("AC", "THEME-08")]
    public async Task ALeavingOverlayPassesClicksThroughAndOpensAgainAsync() =>
        None(await ClockedAsync(() =>
        {
            var errors = new List<string>();
            var clicks = 0;
            var under = new Button { Content = "Under", Width = 300, Height = 200, Margin = new Thickness(0, 60, 0, 0), HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top };
            under.Click += (_, _) => clicks++;
            var trigger = OverlayFixtures.Outline("Open");
            trigger.Flyout = new Flyout { Content = new TextBlock { Text = "Popover", Height = 160 } };
            var window = Plain(new Panel { Children = { under, trigger } }, false, w => Windows.Click(w, trigger));
            Pulse(0);
            Pulse(600);
            trigger.Flyout.Hide();
            Pulse(0);
            errors.AddRange(Ghosts(window).Count == 1 ? [] : [$"{Ghosts(window).Count} pictures leaving the popover"]);
            Windows.Click(window, under);
            errors.AddRange(clicks == 1 ? [] : ["the click under the leaving popover did not reach the button"]);
            Windows.Click(window, trigger);
            Pulse(0);
            errors.AddRange(Ghosts(window).Count == 0 ? [] : ["the popover opened again leaves a picture of it"]);
            errors.AddRange(trigger.Flyout.IsOpen ? [] : ["the popover did not open again"]);
            window.Close();
            var dialog = Overlay(OverlayFixtures.DialogOpen, false);
            Pulse(0);
            Pulse(600);
            Windows.Key(dialog, Key.Escape);
            Pulse(0);
            var opener = dialog.GetVisualDescendants().OfType<Button>().First(button => button.Content as string == "Open Dialog");
            var hit = dialog.InputHitTest(opener.TranslatePoint(new Point(4, 4), dialog) ?? default);
            errors.AddRange(hit is Visual visual && (ReferenceEquals(visual, opener) || opener.IsVisualAncestorOf(visual)) ? [] : [$"the leaving dialog takes the click at its trigger ({hit})"]);
            dialog.Close();
            return errors;
        }));
}
