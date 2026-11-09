using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class PopoverTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    /// <summary>The differences between an open surface's border and shadcn/ui's element: colors, border, radius, padding, shadow and box.</summary>
    internal static IEnumerable<string> Surface(string what, Border surface, Thickness padding, Control root, ElementMetrics expected, double vertical = 2.5, double horizontal = 0.5)
    {
        var look = (Windows.Paint(surface.Background), Windows.Paint(surface.BorderBrush), surface.BorderThickness.Top, surface.CornerRadius.TopLeft, Windows.Shadows(surface.BoxShadow));
        if (!look.Item1.Near(CssColor.Parse(expected.Background)) || (look.Item3 > 0 && !look.Item2.Near(CssColor.Parse(expected.BorderColor))) || look.Item3 != Px(expected.BorderWidth)
            || look.Item4 != Px(expected.Radius) || !CssShadow.Same(look.Item5, CssShadow.List(expected.Shadow)))
        {
            yield return $"{what}: {look.Item1} {look.Item2} {look.Item3} {look.Item4} {string.Join(", ", look.Item5)}, shadcn {expected.Background} {expected.BorderColor} {expected.BorderWidth} {expected.Radius} {expected.Shadow}";
        }

        if (expected.PaddingLeft is { } left && (padding.Left + surface.BorderThickness.Left != Px(left) + Px(expected.BorderWidth) || padding.Top != Px(expected.PaddingTop!)))
        {
            yield return $"{what}: padding {padding}, shadcn {expected.PaddingLeft} {expected.PaddingTop}";
        }

        var box = Windows.Box(surface, root);
        if (Math.Abs(box.X - expected.X) > horizontal || Math.Abs(box.Width - expected.Width) > horizontal || Math.Abs(box.Y - expected.Y) > vertical || Math.Abs(box.Height - expected.Height) > vertical)
        {
            yield return $"{what}: box {box}, shadcn {expected.X:0.##},{expected.Y:0.##} {expected.Width:0.##}x{expected.Height:0.##}";
        }
    }

    internal static double Px(string value) => double.Parse(value.Replace("px", string.Empty, StringComparison.Ordinal), System.Globalization.CultureInfo.InvariantCulture);

    internal static FlyoutPresenter Presenter(Window window) => window.GetVisualDescendants().OfType<FlyoutPresenter>().First(presenter => presenter.IsEffectivelyVisible);

    [Fact]
    [Trait("AC", "POPOVER-01")]
    public async Task TheOpenFlyoutIsShadcnsPopoverContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var content = (await ReferenceMetrics.SlotsAsync("popover-open", name, "popover-content"))[0];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.PopoverOpen(), variant);
                var presenter = Presenter(window);
                var errors = Surface($"{name} popover", Windows.Part<Border>(presenter, "LayoutRoot"), presenter.Padding, window, content).ToList();
                if (!Windows.Paint(presenter.Foreground).Near(CssColor.Parse(content.Color)))
                {
                    errors.Add($"{name}: text {Windows.Paint(presenter.Foreground)}, shadcn {content.Color}");
                }

                window.Close();
                var plain = new Button { Content = "Plain", Flyout = new Flyout { Content = "Text" }, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
                var second = Windows.Fixture(plain, 300, variant);
                Windows.Click(second, plain);
                if (Presenter(second).Bounds.Width != 288)
                {
                    errors.Add($"{name}: plain flyout {Presenter(second).Bounds.Width} px, shadcn 288");
                }

                second.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "POPOVER-02")]
    public async Task TheKeyboardOpensFocusesAndClosesTheFlyoutAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var box = new TextBox { Text = "100%", Width = 120 };
            var flyout = new Flyout { Content = new StackPanel { Spacing = 8, Children = { box, new Button { Content = "Apply" } } } };
            var trigger = OverlayFixtures.Outline("Open");
            trigger.Flyout = flyout;
            var window = Windows.Fixture(new Panel { Children = { trigger } }, 400, ThemeVariant.Light);
            trigger.Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Enter);
            var steps = new List<string> { $"return {flyout.IsOpen} {ReferenceEquals(Windows.Focused(window), box)}" };
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {flyout.IsOpen} {ReferenceEquals(Windows.Focused(window), trigger)}");
            Windows.Click(window, trigger);
            var gap = Windows.Box(Presenter(window), window).Y - Windows.Box(trigger, window).Bottom;
            window.MouseDown(new Point(600, 380), MouseButton.Left, RawInputModifiers.None);
            window.MouseUp(new Point(600, 380), MouseButton.Left, RawInputModifiers.None);
            Windows.Settle();
            steps.Add($"outside {flyout.IsOpen} gap {gap}");
            window.Close();
            return steps;
        });

        Assert.Equal(["return True True", "escape False True", "outside False gap 4"], found);
    }

    [Fact]
    [Trait("AC", "POPOVER-03")]
    public async Task TheOpenStateBindsTwoWayAndTheContentBindsTheModelAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var trigger = view.FindControl<Button>("PopoverTrigger")!;
            model.PopoverOpen = true;
            Windows.Settle();
            var presenter = Presenter(window);
            var shown = presenter.GetVisualDescendants().OfType<TextBlock>().First().Text;
            Windows.Click(window, presenter.GetVisualDescendants().OfType<Button>().First());
            var ran = string.Join(",", model.Ran);
            presenter.GetVisualDescendants().OfType<Button>().First().Focus(NavigationMethod.Tab);
            Windows.Key(window, Key.Escape);
            var closed = (trigger.Flyout!.IsOpen, model.PopoverOpen);
            window.Close();
            return (shown, ran, closed);
        });

        Assert.Equal(("Layer: Background", "apply", (false, false)), found);
    }
}
