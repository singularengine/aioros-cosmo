using System.Collections.ObjectModel;
using System.Globalization;
using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Markup;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class GestureTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static List<NotificationCard> Cards(Window window) => [.. window.GetVisualDescendants().OfType<NotificationCard>()];

    private static string Number(double value) => Math.Round(value, 1).ToString(CultureInfo.InvariantCulture);

    /// <summary>Presses the left button at a point, moves it by an offset in four steps while the clock runs the given time, runs a check before letting go, and lets go.</summary>
    private static string Drag(Window window, Point from, Vector by, ManualTime time, double milliseconds, Func<string> during)
    {
        window.MouseDown(from, MouseButton.Left, RawInputModifiers.None);
        Windows.Settle();
        for (var step = 1; step <= 4; step++)
        {
            time.Advance(TimeSpan.FromMilliseconds(milliseconds / 4));
            window.MouseMove(from + (by * step / 4), RawInputModifiers.LeftMouseButton);
            Windows.Settle();
        }

        var seen = during();
        window.MouseUp(from + by, MouseButton.Left, RawInputModifiers.None);
        Windows.Settle();
        return seen;
    }

    /// <summary>Plays the exits of the toasts closing, on the window's clock.</summary>
    private static void Leave()
    {
        Windows.Clock!.Pulse(TimeSpan.Zero);
        Windows.Clock.Pulse(TimeSpan.FromMilliseconds(400));
        Windows.Settle();
    }

    private static Matrix Moved(Visual visual) => visual.RenderTransform?.Value ?? Matrix.Identity;

    [Fact]
    [Trait("AC", "SONNER-08")]
    public async Task AToastIsSwipedAwayAsSonnersAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            (Windows.Clocked, Windows.Holding) = (true, false);
            try
            {
                var time = new ManualTime();
                var toasts = new ObservableCollection<Toast>();
                var window = Windows.Fixture(new Panel { Children = { new Toaster { Toasts = toasts, TimeProvider = time } } }, 400, ThemeVariant.Light);
                var steps = new List<string>();
                toasts.Add(new Toast { Title = "Swiped", ActionText = "Undo", ActionCommand = new Command(_ => { }), Duration = TimeSpan.FromDays(1) });
                Windows.Settle();
                Leave();
                var card = Cards(window)[^1];
                var at = Windows.Center(card) - new Vector(100, 0);
                string Back(Func<double> moved)
                {
                    Leave();
                    return Number(moved());
                }

                steps.Add($"declares {Web.GetSwipe(card)}");
                steps.Add(Drag(window, at, new Vector(20, 0), time, 1000, () => $"20 right {Number(Moved(card).M31)}") + $", back {Back(() => Moved(card).M31)} {card.IsClosing}");
                steps.Add(Drag(window, at, new Vector(-30, 0), time, 1000, () => $"30 left {Number(Moved(card).M31)}") + $", back {Back(() => Moved(card).M31)}");
                steps.Add(Drag(window, at, new Vector(0, -30), time, 1000, () => $"30 up {Number(Moved(card).M32)}") + $", back {Back(() => Moved(card).M32)}");
                var undo = card.GetVisualDescendants().OfType<Button>().Single(button => button.IsVisible);
                steps.Add(Drag(window, Windows.Center(undo), new Vector(60, 0), time, 1000, () => $"from the action {Number(Moved(card).M31)}"));
                steps.Add(Drag(window, at, new Vector(60, 0), time, 1000, () => $"60 right {Number(Moved(card).M31)}"));
                var swiped = card.Transitions?.OfType<TransitionBase>().Select(transition => $"{transition.Property?.Name} {transition.Duration.TotalMilliseconds}") ?? [];
                Windows.Clock!.Pulse(TimeSpan.Zero);
                Windows.Clock.Pulse(TimeSpan.FromMilliseconds(200));
                steps.Add($"leaves {card.Classes.Contains("swiped")} {Number(Moved(card).M31)} {card.Opacity} {card.IsClosing} over {string.Join(",", swiped)}");
                Leave();
                steps.Add($"gone {toasts.Count}");
                toasts.Add(new Toast { Title = "Flicked", Duration = TimeSpan.FromDays(1) });
                Windows.Settle();
                Leave();
                var flicked = Cards(window)[^1];
                Drag(window, Windows.Center(flicked), new Vector(0, 20), time, 100, () => string.Empty);
                steps.Add($"flicked {flicked.IsClosing} {flicked.Classes.Contains("swiped")}");
                window.Close();
                return steps;
            }
            finally
            {
                (Windows.Clocked, Windows.Holding) = (false, true);
            }
        });

        Assert.Equal(
            [
                "declares Down,Right", "20 right 20, back 0 False", "30 left -10, back 0", "30 up -10, back 0", "from the action 0",
                "60 right 60", "leaves True 416 0 True over Opacity 200,RenderTransform 200", "gone 0", "flicked True True",
            ],
            found);
    }

    [Fact]
    [Trait("AC", "SONNER-10")]
    public async Task AToastShowsSonnersCloseButtonAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var normal = (await ReferenceMetrics.SlotsAsync("sonner-close", name, "toast-close"))[0];
            var hover = (await ReferenceMetrics.SlotsAsync("sonner-close", name, "toast-close", "hover"))[0];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.SonnerClose(), variant);
                var close = Cards(window)[^1].GetVisualDescendants().OfType<Button>().Single(button => button.Name == "ToastClose");
                var errors = new List<string>();
                var box = Windows.Box(close, window);
                if (Math.Abs(box.X - normal.X) > 1 || Math.Abs(box.Y - normal.Y) > 1.5 || box.Width != normal.Width || box.Height != normal.Height || close.CornerRadius.TopLeft != 10)
                {
                    errors.Add($"{name}: close at {box} radius {close.CornerRadius.TopLeft}, shadcn {normal.X},{normal.Y} {normal.Width}x{normal.Height}");
                }

                errors.AddRange(Look($"{name} close", close, normal));
                Windows.Hover(window, close);
                errors.AddRange(Look($"{name} hovered close", close, hover));
                window.Close();
                return errors;
            }));
        }

        var behavior = await desktop.RunAsync(() =>
        {
            Windows.Clocked = true;
            try
            {
                var toasts = new ObservableCollection<Toast>();
                var window = Windows.Fixture(new Panel { Children = { new Toaster { Toasts = toasts, CloseButton = true } } }, 400, ThemeVariant.Light);
                toasts.Add(new Toast { Title = "Loading...", Kind = ToastKind.Loading });
                toasts.Add(new Toast { Title = "Without", CloseButton = false, Duration = TimeSpan.FromDays(1) });
                toasts.Add(new Toast { Title = "Closed", Duration = TimeSpan.FromDays(1) });
                Windows.Settle();
                string Shown() => string.Join(",", Cards(window).Select(card => card.GetVisualDescendants().OfType<Button>().Single(button => button.Name == "ToastClose").IsVisible));
                var shown = Shown();
                var close = Cards(window)[^1].GetVisualDescendants().OfType<Button>().Single(button => button.Name == "ToastClose");
                var declared = Web.GetCloses(close);
                Windows.Click(window, close);
                Leave();
                var left = string.Join(",", toasts.Select(toast => toast.Title));
                toasts[0].Resolve(ToastKind.Success, "Done");
                Windows.Settle();
                var resolved = Shown();
                window.Close();
                return $"{shown} {declared} {left} {resolved}";
            }
            finally
            {
                Windows.Clocked = false;
            }
        });

        Assert.Empty(wrong);
        Assert.Equal("False,False,True True Loading...,Without True,False", behavior);
    }

    private static IEnumerable<string> Look(string what, Button close, ElementMetrics expected)
    {
        var presenter = Windows.Part<ContentPresenter>(close, "PART_ContentPresenter");
        var path = close.GetVisualDescendants().OfType<Avalonia.Controls.Shapes.Path>().Single();
        return new[]
        {
            ("background", Windows.Paint(presenter.Background), CssColor.Parse(expected.Background)),
            ("border", Windows.Paint(presenter.BorderBrush), CssColor.Parse(expected.BorderColor)),
            ("icon", Windows.Paint(path.Stroke), CssColor.Parse(expected.Color)),
        }
        .Where(look => !look.Item2.Near(look.Item3)).Select(look => $"{what}: {look.Item1} {look.Item2}, shadcn {look.Item3}");
    }

    [Fact]
    [Trait("AC", "DRAWER-09")]
    public async Task ADrawerIsDraggedDownToCloseAsVaulsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var time = new ManualTime();
            var drawer = new Drawer
            {
                TimeProvider = time,
                Content = new StackPanel { Spacing = 8, Margin = new Thickness(16), Children = { new TextBox { Text = "Name" }, new Border { Height = 240 } } },
            };
            var window = Windows.Fixture(new Panel { Children = { drawer } }, 600, ThemeVariant.Light, host: true);
            drawer.IsOpen = true;
            Windows.Settle();
            var steps = new List<string>();
            var surface = DialogTests.Surface(window);
            var mask = window.GetVisualDescendants().OfType<Border>().Single(border => border.Classes.Contains("cosmo-overlay"));
            var height = surface.Bounds.Height;
            var at = surface.TranslatePoint(new Point(surface.Bounds.Width / 2, 10), window)!.Value;
            steps.Add($"declares {Web.GetSwipe(surface)} {Web.GetSwipeDistance(surface)} {Web.GetSwipeVelocity(surface)}");
            steps.Add(Drag(window, at, new Vector(0, height / 5), time, 1000, () => $"a fifth {Number(Moved(surface).M32 / height * 5)} mask {Number(mask.Opacity)}")
                + $", back {Number(Moved(surface).M32)} mask {mask.Opacity} {drawer.IsOpen}");
            steps.Add(Drag(window, at, new Vector(0, -30), time, 1000, () => $"30 up {Number(Moved(surface).M32)}") + $", back {Number(Moved(surface).M32)}");
            var text = surface.GetVisualDescendants().OfType<TextBox>().Single();
            steps.Add(Drag(window, Windows.Center(text), new Vector(0, 60), time, 1000, () => $"from the text box {Number(Moved(surface).M32)}") + $" {drawer.IsOpen}");
            Drag(window, at, new Vector(0, height / 3), time, 1000, () => string.Empty);
            steps.Add($"a third {drawer.IsOpen}");
            drawer.IsOpen = true;
            Windows.Settle();
            surface = DialogTests.Surface(window);
            steps.Add($"opened at rest {Number(Moved(surface).M32)}");
            Drag(window, surface.TranslatePoint(new Point(surface.Bounds.Width / 2, 10), window)!.Value, new Vector(0, 20), time, 20, () => string.Empty);
            steps.Add($"flicked {drawer.IsOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["declares Down 0.25 0.4", "a fifth 1 mask 0.8, back 0 mask 1 True", "30 up -11.5, back 0", "from the text box 0 True", "a third False", "opened at rest 0", "flicked False"], found);
    }

    [Fact]
    [Trait("AC", "DRAWER-11")]
    public async Task ADrawerOnTheRightIsAFullHeightPanelDraggedRightToCloseAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var time = new ManualTime();
            var drawer = new Drawer { Side = SheetSide.Right, TimeProvider = time, Content = new Border { Height = 200 } };
            var window = Windows.Fixture(new Panel { Children = { drawer } }, 800, ThemeVariant.Light, host: true, width: 1280, padding: 0);
            drawer.IsOpen = true;
            Windows.Settle();
            var steps = new List<string>();
            var surface = DialogTests.Surface(window);
            var box = Windows.Box(surface, (Control)window.Content!);
            var handle = surface.GetVisualDescendants().OfType<Border>().Single(border => border.Name == "PART_Handle");
            steps.Add($"{box} handle {handle.IsVisible} border {surface.BorderThickness} enters {Motion.GetEnterOffset(surface)} swipe {Web.GetSwipe(surface)}");
            var at = surface.TranslatePoint(new Point(20, surface.Bounds.Height / 2), window)!.Value;
            steps.Add(Drag(window, at, new Vector(surface.Bounds.Width / 10, 0), time, 1000, () => $"a tenth {Number(Moved(surface).M31 / surface.Bounds.Width * 10)}") + $", back {Number(Moved(surface).M31)} {drawer.IsOpen}");
            Drag(window, at, new Vector(surface.Bounds.Width / 3, 0), time, 1000, () => string.Empty);
            steps.Add($"a third {drawer.IsOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["896, 0, 384, 800 handle False border 1,0,0,0 enters 100%, 0% swipe Right", "a tenth 1, back 0 True", "a third False"], found);
    }
}
