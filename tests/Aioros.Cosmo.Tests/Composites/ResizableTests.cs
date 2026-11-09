using Aioros.Cosmo.Layout;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Cosmo.Tests.Display;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Styling;
using static Aioros.Cosmo.Tests.Display.Looks;

namespace Aioros.Cosmo.Tests.Composites;

public sealed class ResizableTests(DesktopSession desktop)
{
    [Fact]
    [Trait("AC", "RESIZABLE-02")]
    public async Task PanelsAndHandlesAreShadcnsAsync()
    {
        var wrong = new List<string>();
        foreach (var fixture in (string[])["resizable-demo", "resizable-handle", "resizable-vertical"])
        {
            wrong.AddRange(await Looks.FixtureAsync(desktop, fixture, (window, root, reference, theme) => Check(window, root, reference, $"{theme} {fixture}", fixture == "resizable-handle")));
        }

        Assert.Empty(wrong);
    }

    private static List<string> Check(Window window, Control root, IReadOnlyList<ElementMetrics> reference, string what, bool grip)
    {
        var panels = All<ResizablePanel>(root);
        var handles = All<ResizableHandle>(root);
        var expectedPanels = reference.Where(element => element.Slot == "resizable-panel").ToList();
        var expectedHandles = reference.Where(element => element.Slot == "resizable-handle").ToList();
        var errors = panels.Count == expectedPanels.Count && handles.Count == expectedHandles.Count ? [] : new List<string> { $"{what}: {panels.Count} panels, {handles.Count} handles" };
        errors.AddRange(panels.Zip(expectedPanels).SelectMany((pair, i) => Near($"{what} panel {i}", Windows.Box(pair.First, root), pair.Second, 1)));
        errors.AddRange(handles.Zip(expectedHandles).SelectMany((pair, i) => Near($"{what} handle {i}", Windows.Box(pair.First, root), pair.Second, 1)
            .Concat(Paints($"{what} handle {i}", Windows.Part<Border>(pair.First, "PART_Line").Background, pair.Second.Background))));
        if (grip)
        {
            errors.AddRange(Grip(window, root, handles[0], what));
        }

        return errors;
    }

    private static List<string> Grip(Window window, Control root, ResizableHandle handle, string what)
    {
        var grip = Windows.Part<Border>(handle, "PART_Grip");
        var icon = Windows.Part<Viewbox>(grip);
        var errors = grip.IsEffectivelyVisible && grip.Bounds.Size == new Size(12, 16) && grip.CornerRadius.TopLeft == 2 && grip.BorderThickness == new Thickness(1) && icon.Bounds.Width == 10 ? [] : new List<string> { $"{what}: grip {grip.Bounds} {grip.CornerRadius} {icon.Bounds}" };
        errors.AddRange(Paints($"{what} grip border", grip.BorderBrush, window, "CosmoBorderBrush").Concat(Paints($"{what} grip fill", grip.Background, window, "CosmoBorderBrush"))
            .Concat(Paints($"{what} grip icon", Windows.Part<Avalonia.Controls.Shapes.Path>(icon).Stroke, window, "CosmoForegroundBrush")));
        var center = Windows.Box(grip, root).Center;
        errors.AddRange(Math.Abs(center.X - Windows.Box(handle, root).Center.X) <= 0.5 ? [] : [$"{what}: grip off the line {center}"]);
        handle.Focus(NavigationMethod.Tab);
        Windows.Settle();
        var ring = Windows.Part<Border>(handle, "PART_Ring");
        var line = Windows.Box(handle, root);
        errors.AddRange(ring.IsEffectivelyVisible && Windows.Box(ring, root) == line.Inflate(2) && ring.BorderThickness == new Thickness(1) ? [] : [$"{what}: ring {ring.IsEffectivelyVisible} {Windows.Box(ring, root)}"]);
        errors.AddRange(Paints($"{what} ring", ring.BorderBrush, window, "CosmoRingBrush"));
        return errors;
    }

    [Fact]
    [Trait("AC", "RESIZABLE-03")]
    public async Task HandlesResizeWithTheLibrarysKeysAndTheMouseAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var first = new ResizablePanel { DefaultSize = 20, MinSize = 10, MaxSize = 40 };
            var middle = new ResizablePanel { DefaultSize = 50, IsCollapsible = true };
            var last = new ResizablePanel { DefaultSize = 30 };
            var one = new ResizableHandle();
            var two = new ResizableHandle();
            var group = new ResizablePanelGroup { Width = 602, Height = 200, Children = { first, one, middle, two, last } };
            var window = Windows.Show(new Border { Margin = new Thickness(24), Child = group, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top }, ThemeVariant.Light);
            string Sizes() => string.Join('/', group.Panels.Select(panel => Math.Round(panel.Size).ToString(System.Globalization.CultureInfo.InvariantCulture)));
            var steps = new List<string> { Sizes() };
            one.Focus(NavigationMethod.Tab);
            foreach (var key in new[] { Key.Right, Key.End, Key.Home, Key.End })
            {
                Windows.Key(window, key);
                steps.Add(Sizes());
            }

            var grip = Windows.Center(one);
            window.MouseDown(grip, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(grip - new Point(150, 0), RawInputModifiers.LeftMouseButton);
            window.MouseMove(grip - new Point(300, 0), RawInputModifiers.LeftMouseButton);
            window.MouseUp(grip - new Point(300, 0), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            steps.Add(Sizes());
            two.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Enter);
            steps.Add(Sizes());
            Windows.Key(window, Key.Enter);
            steps.Add(Sizes());
            Windows.Key(window, Key.F6);
            var next = Windows.Focused(window) == one;
            window.KeyPress(Key.F6, RawInputModifiers.Shift, PhysicalKey.None, null);
            Windows.Settle();
            var back = Windows.Focused(window) == two;
            Windows.Key(window, Key.Up);
            Windows.Key(window, Key.Down);
            steps.Add(Sizes());
            window.Close();
            return (string.Join(' ', steps), next, back);
        });

        Assert.Equal(("20/50/30 25/45/30 40/30/30 10/60/30 40/30/30 10/60/30 10/0/90 10/60/30 10/60/30", true, true), found);
    }

    [Fact]
    [Trait("AC", "RESIZABLE-04")]
    public async Task TheBoundSizeFollowsTheModelBothWaysAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new CompositesModel();
            var view = new CompositesView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var group = view.FindControl<ResizablePanelGroup>("Split")!;
            var handle = group.Handles[0];
            var grip = Windows.Center(handle);
            window.MouseDown(grip, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(grip + new Point(60, 0), RawInputModifiers.LeftMouseButton);
            window.MouseUp(grip + new Point(60, 0), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            var dragged = model.Split > 35;
            Windows.Click(window, view.FindControl<Button>("Reset")!);
            var halves = Math.Abs(group.Panels[0].Bounds.Width - group.Panels[1].Bounds.Width) <= 1;
            model.Minimum = 0.4;
            Windows.Settle();
            grip = Windows.Center(handle);
            window.MouseDown(grip, MouseButton.Left, RawInputModifiers.None);
            window.MouseMove(grip - new Point(200, 0), RawInputModifiers.LeftMouseButton);
            window.MouseUp(grip - new Point(200, 0), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            var floor = Math.Round(model.Split);
            window.Close();
            return (dragged, halves, floor);
        });

        Assert.Equal((true, true, 40d), found);
    }

    [Fact]
    [Trait("AC", "RESIZABLE-05")]
    public async Task AHandleNextToABoundPanelPostsItsPositionAsync()
    {
        var delays = await desktop.RunAsync(() =>
        {
            var view = new CompositesView { DataContext = new CompositesModel() };
            var plain = new ResizablePanelGroup { Width = 300, Height = 100, Children = { new ResizablePanel(), new ResizableHandle(), new ResizablePanel() } };
            var window = Windows.Show(new StackPanel { Children = { view, plain } }, ThemeVariant.Light);
            var found = (Aioros.Markup.Web.GetDelay(view.FindControl<ResizablePanelGroup>("Split")!.Handles[0]), Aioros.Markup.Web.GetDelay(plain.Handles[0]));
            window.Close();
            return found;
        });

        Assert.Equal((150d, double.NaN), delays);
    }
}
