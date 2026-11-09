using Aioros.Cosmo.Forms;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Styling;
using Avalonia.VisualTree;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Forms;

public sealed class SelectScrollTests(DesktopSession desktop)
{
    private static Border Open(Window window)
    {
        var combo = All<ComboBox>((Control)window.Content!).Single();
        combo.IsDropDownOpen = true;
        Windows.Settle();
        Windows.Settle();
        return (Border)Windows.Part<Popup>(combo, "PART_Popup").Child!;
    }

    private static List<string> Boxes(string what, Border surface, IReadOnlyList<ElementMetrics> reference)
    {
        var content = reference.Single(element => element.Slot == "select-content");
        var errors = new List<string>();
        if (Math.Abs(surface.Bounds.Width - content.Width) > 1.5 || Math.Abs(surface.Bounds.Height - content.Height) > 1.5)
        {
            errors.Add($"{what} list {surface.Bounds.Size}, shadcn {content.Width}x{content.Height}");
        }

        var up = surface.GetVisualDescendants().OfType<SelectScrollUpButton>().Single();
        var down = surface.GetVisualDescendants().OfType<SelectScrollDownButton>().Single();
        var scroller = surface.GetVisualDescendants().OfType<ScrollViewer>().Single();
        errors.AddRange(scroller.VerticalScrollBarVisibility == Avalonia.Controls.Primitives.ScrollBarVisibility.Hidden ? [] : [$"{what} scroll bar {scroller.VerticalScrollBarVisibility}"]);
        foreach (var (button, slot) in new (Control Button, string Slot)[] { (up, "select-scroll-up-button"), (down, "select-scroll-down-button") })
        {
            var expected = reference.SingleOrDefault(element => element.Slot == slot);
            if (button.IsVisible != expected is not null)
            {
                errors.Add($"{what} {slot} shown {button.IsVisible}, shadcn {expected is not null}");
            }
            else if (expected is not null)
            {
                errors.AddRange(Near($"{what} {slot}", Windows.Box(button, surface), expected, content));
            }
        }

        var viewport = new Rect(Windows.Box(scroller, surface).Position, scroller.Viewport);
        var shown = surface.GetVisualDescendants().OfType<ComboBoxItem>().Where(item => item.IsEffectivelyVisible && Windows.Box(item, surface).Intersects(viewport)).ToList();
        var top = reference.SingleOrDefault(element => element.Slot == "select-scroll-up-button") is { } shownUp ? shownUp.Y + shownUp.Height : content.Y + 1;
        var bottom = reference.SingleOrDefault(element => element.Slot == "select-scroll-down-button") is { } shownDown ? shownDown.Y : content.Y + content.Height - 1;
        var expectedItems = reference.Where(element => element.Slot is "select-item" or "select-label" && element.Y + element.Height > top + 0.5 && element.Y < bottom - 0.5).ToList();
        foreach (var expected in expectedItems)
        {
            var item = shown.FirstOrDefault(found => found.Content?.ToString() == expected.Text.Replace("&amp;", "&", StringComparison.Ordinal));
            errors.AddRange(item is null ? [$"{what} {expected.Text} not shown"] : Near($"{what} {expected.Text}", Windows.Box(item, surface), expected, content));
        }

        return errors;
    }

    private static IEnumerable<string> Near(string what, Rect found, ElementMetrics expected, ElementMetrics content)
    {
        var box = new Rect(expected.X - content.X, expected.Y - content.Y, expected.Width, expected.Height);
        return Math.Abs(found.X - box.X) <= 1.5 && Math.Abs(found.Y - box.Y) <= 1.5 && Math.Abs(found.Width - box.Width) <= 1.5 && Math.Abs(found.Height - box.Height) <= 1.5
            ? []
            : [$"{what}: {found}, shadcn {box}"];
    }

    [Fact]
    [Trait("AC", "SELECT-07")]
    public async Task AnOverflowingListShowsItsScrollButtonsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["select-scrollable", "select-scrolled"])
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (window, _, reference, theme) =>
            {
                var surface = Open(window);
                var errors = Boxes($"{fixture} {theme}", surface, reference).ToList();
                All<ComboBox>((Control)window.Content!).Single().IsDropDownOpen = false;
                return errors;
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "SELECT-11")]
    public async Task TheListKeepsTenPixelsFromTheWindowsEdgeAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["select-scrollable", "select-scrolled"])
        {
            wrong.AddRange(await FixtureAsync(desktop, fixture, (window, _, reference, theme) =>
            {
                var surface = Open(window);
                var content = reference.Single(element => element.Slot == "select-content");
                var root = (Control)window.Content!;
                var at = surface.PointToScreen(default) - root.PointToScreen(default);
                All<ComboBox>(root).Single().IsDropDownOpen = false;
                return Math.Abs(at.X - content.X) <= 1.5 && Math.Abs(at.Y - content.Y) <= 1.5 ? [] : [$"{fixture} {theme}: list at {at.X},{at.Y}, shadcn {content.X},{content.Y}"];
            }));
        }

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }

    [Fact]
    [Trait("AC", "SELECT-08")]
    public async Task TheScrollButtonsScrollWhilePointedAtAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var time = new ManualTime();
            var window = DisplayFixtures.Show("select-scrollable", ThemeVariant.Light);
            var surface = Open(window);
            var up = surface.GetVisualDescendants().OfType<SelectScrollUpButton>().Single();
            var down = surface.GetVisualDescendants().OfType<SelectScrollDownButton>().Single();
            var scroller = surface.GetVisualDescendants().OfType<ScrollViewer>().Single();
            up.Time = time;
            down.Time = time;
            var steps = new List<string>();
            void Tick(int count)
            {
                for (var i = 0; i < count; i++)
                {
                    time.Advance(TimeSpan.FromMilliseconds(50));
                    Windows.Settle();
                }
            }

            string State() => $"{scroller.Offset.Y} up {up.IsVisible} down {down.IsVisible}";
            Windows.Hover(window, down);
            Tick(1);
            steps.Add(State());
            Tick(2);
            steps.Add(State());
            Windows.Away(window, new Point(2, window.Height - 2));
            Tick(3);
            steps.Add(State());
            var top = TopLevel.GetTopLevel(down)!;
            top.MouseDown(Windows.Center(down), Avalonia.Input.MouseButton.Left, Avalonia.Input.RawInputModifiers.None);
            Tick(1);
            top.MouseUp(Windows.Center(down), Avalonia.Input.MouseButton.Left, Avalonia.Input.RawInputModifiers.None);
            steps.Add(State());
            Windows.Hover(window, down);
            Tick(60);
            steps.Add($"end {scroller.Offset.Y == scroller.Extent.Height - scroller.Viewport.Height} up {up.IsVisible} down {down.IsVisible}");
            var end = scroller.Offset.Y;
            Tick(2);
            steps.Add($"stopped {scroller.Offset.Y == end}");
            Windows.Hover(window, up);
            Tick(1);
            steps.Add($"back {end - scroller.Offset.Y}");
            All<ComboBox>((Control)window.Content!).Single().IsDropDownOpen = false;
            window.Close();
            return steps;
        });

        Assert.Equal(["32 up True down True", "96 up True down True", "96 up True down True", "128 up True down True", "end True up True down False", "stopped True", "back 32"], found);
    }
}
