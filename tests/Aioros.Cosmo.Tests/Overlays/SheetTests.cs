using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class SheetTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static IEnumerable<string> Look(string what, DialogSurface surface, ElementMetrics content, Thickness border, double radius)
    {
        var root = Windows.Part<Border>(surface, "PART_Root");
        if (!Windows.Paint(root.Background).Near(CssColor.Parse(content.Background)) || root.BorderThickness != border || root.CornerRadius.TopLeft != radius
            || (border != default && !Windows.Paint(root.BorderBrush).Near(CssColor.Parse(content.BorderColor))))
        {
            yield return $"{what}: {Windows.Paint(root.Background)} border {root.BorderThickness} {Windows.Paint(root.BorderBrush)} radius {root.CornerRadius}, shadcn {content.Background} {border} {content.BorderColor} {radius}";
        }
    }

    [Fact]
    [Trait("AC", "SHEET-01")]
    public async Task OpenSheetsAreShadcnsSheetContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var right = await ReferenceMetrics.OfAsync("sheet-open", name);
            var top = await ReferenceMetrics.OfAsync("sheet-top", name);
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.SheetOpen(), variant);
                var surface = DialogTests.Surface(window);
                var content = right.Single(element => element.Slot == "sheet-content");
                var errors = Look($"{name} right", surface, content, new Thickness(1, 0, 0, 0), 0).ToList();
                errors.AddRange(DialogTests.Box($"{name} right", surface, window, content));
                errors.AddRange(DialogTests.Header($"{name} right", surface, window, right.Single(element => element.Slot == "sheet-title"), right.Single(element => element.Slot == "sheet-description")));
                var buttons = surface.GetVisualDescendants().OfType<DialogFooter>().First().Children.OfType<Button>().ToList();
                errors.AddRange(DialogTests.Box($"{name} save", buttons[0], window, right.Single(element => element.Text == "Save changes")));
                errors.AddRange(DialogTests.Box($"{name} close", buttons[1], window, right.Single(element => element.Slot == "sheet-close")));
                window.Close();
                window = OverlayFixtures.Show(OverlayFixtures.SheetTop(), variant);
                surface = DialogTests.Surface(window);
                content = top.Single(element => element.Slot == "sheet-content");
                errors.AddRange(Look($"{name} top", surface, content, new Thickness(0, 0, 0, 1), 0));
                errors.AddRange(DialogTests.Box($"{name} top", surface, window, content));
                errors.AddRange(DialogTests.Header($"{name} top", surface, window, top.Single(element => element.Slot == "sheet-title"), top.Single(element => element.Slot == "sheet-description")));
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "SHEET-02")]
    public async Task LeftAndBottomSheetsCoverTheirEdgesAndCloseAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var steps = new List<string>();
            foreach (var (side, width) in new[] { (SheetSide.Left, 400.0), (SheetSide.Bottom, 640.0) })
            {
                var sheet = new Sheet { Side = side, Content = new StackPanel { Spacing = 8, Margin = new Thickness(16), Children = { new TextBox { Text = "First" }, new Button { Content = "Last" } } } };
                var trigger = OverlayFixtures.Outline("Open");
                trigger.Click += (_, _) => sheet.IsOpen = true;
                var window = Windows.Fixture(new Panel { Children = { trigger, sheet } }, 400, ThemeVariant.Light, host: true);
                window.Width = width;
                Windows.Settle();
                Windows.Click(window, trigger);
                var surface = DialogTests.Surface(window);
                var root = Windows.Part<Border>(surface, "PART_Root");
                steps.Add($"{side} {Windows.Box(surface, window)} {root.BorderThickness}");
                var first = Windows.Focused(window) as TextBox;
                Windows.Key(window, Key.Tab);
                Windows.Key(window, Key.Tab);
                Windows.Key(window, Key.Tab);
                steps.Add($"focus {first?.Text} cycles {(Windows.Focused(window) as TextBox)?.Text}");
                Windows.Key(window, Key.Escape);
                steps.Add($"escape {sheet.IsOpen} {ReferenceEquals(Windows.Focused(window), trigger)}");
                Windows.Click(window, trigger);
                DialogTests.Corner(window, side == SheetSide.Left ? new Point(390, 10) : new Point(10, 10));
                steps.Add($"mask {sheet.IsOpen}");
                window.Close();
            }

            return steps;
        });

        Assert.Equal(["Left 0, 0, 300, 400 0,0,1,0", "focus First cycles First", "escape False True", "mask False", "Bottom 0, 287, 640, 113 0,1,0,0", "focus First cycles First", "escape False True", "mask False"], found);
    }

    [Fact]
    [Trait("AC", "SHEET-03")]
    public async Task TheSheetsOpenStateBindsTwoWayAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            model.SheetOpen = true;
            Windows.Settle();
            var surface = DialogTests.Surface(window);
            var shown = (surface.Classes.Contains("left"), surface.GetVisualDescendants().OfType<TextBlock>().Any(text => text.Text == "Pedro Duarte"));
            surface.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Escape);
            window.Close();
            return (shown, model.SheetOpen);
        });

        Assert.Equal(((true, true), false), found);
    }

    [Fact]
    [Trait("AC", "DRAWER-01")]
    public async Task TheOpenDrawerIsShadcnsDrawerContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var reference = await ReferenceMetrics.OfAsync("drawer-open", name);
            var tokens = await ReferenceMetrics.TokensAsync("neutral", name);
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.DrawerOpen(), variant);
                var surface = DialogTests.Surface(window);
                var content = reference.Single(element => element.Slot == "drawer-content");
                var errors = Look($"{name} drawer", surface, content, new Thickness(0, 1, 0, 0), 10).ToList();
                var root = Windows.Part<Border>(surface, "PART_Root");
                if (root.CornerRadius.BottomLeft != 0)
                {
                    errors.Add($"{name}: drawer radius {root.CornerRadius}");
                }

                errors.AddRange(DialogTests.Box($"{name} drawer", surface, window, content, 0.5, 3));
                var handle = Windows.Part<Border>(surface, "PART_Handle");
                var handleBox = Windows.Box(handle, surface);
                if (handle.Bounds.Size != new Size(100, 8) || handleBox.Y != 17 || handleBox.X != 270 || !Windows.Paint(handle.Background).Near(CssColor.Parse(tokens["muted"].Css)))
                {
                    errors.Add($"{name}: handle {handleBox} {Windows.Paint(handle.Background)}");
                }

                errors.AddRange(DialogTests.Box($"{name} title", Windows.Part<Avalonia.Controls.Presenters.ContentPresenter>(surface.GetVisualDescendants().OfType<DialogHeader>().First(), "PART_Title"), window, reference.Single(element => element.Slot == "drawer-title"), 1, 3));
                var buttons = surface.GetVisualDescendants().OfType<DialogFooter>().First().Children.OfType<Button>().ToList();
                errors.AddRange(DialogTests.Box($"{name} submit", buttons[0], window, reference.Single(element => element.Text == "Submit"), 0.5, 3));
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "DRAWER-02")]
    public async Task TheDrawerTrapsTheFocusClosesAndKeepsToEightyPercentAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var drawer = new Drawer { Content = new StackPanel { Spacing = 8, Margin = new Thickness(16), Children = { new TextBox { Text = "First" }, new Border { Height = 800 }, new Button { Content = "Last" } } } };
            var trigger = OverlayFixtures.Outline("Open");
            trigger.Click += (_, _) => drawer.IsOpen = true;
            var window = Windows.Fixture(new Panel { Children = { trigger, drawer } }, 500, ThemeVariant.Light, host: true);
            Windows.Click(window, trigger);
            var steps = new List<string> { $"height {DialogTests.Surface(window).Bounds.Height} focus {(Windows.Focused(window) as TextBox)?.Text}" };
            Windows.Key(window, Key.Tab);
            Windows.Key(window, Key.Tab);
            steps.Add($"cycles {(Windows.Focused(window) as TextBox)?.Text}");
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {drawer.IsOpen} {ReferenceEquals(Windows.Focused(window), trigger)}");
            Windows.Click(window, trigger);
            DialogTests.Corner(window, new Point(600, 10));
            steps.Add($"mask {drawer.IsOpen}");
            window.Close();
            return steps;
        });

        Assert.Equal(["height 400 focus First", "cycles First", "escape False True", "mask False"], found);
    }

    [Fact]
    [Trait("AC", "DRAWER-03")]
    public async Task ACloseButtonClosesTheDrawerAndTellsTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            model.DrawerOpen = true;
            Windows.Settle();
            Windows.Click(window, DialogTests.Surface(window).GetVisualDescendants().OfType<Button>().First(button => button.Content as string == "Cancel"));
            var open = window.GetVisualDescendants().OfType<DialogSurface>().Any();
            window.Close();
            return (open, model.DrawerOpen);
        });

        Assert.Equal((false, false), found);
    }
}
