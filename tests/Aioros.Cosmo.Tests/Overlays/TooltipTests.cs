using Aioros.Cosmo.Testing;
using Aioros.Cosmo.Tests.Desktop;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Tests.Overlays;

public sealed class TooltipTests(DesktopSession desktop)
{
    private static readonly (ThemeVariant Variant, string Name)[] Themes = [(ThemeVariant.Light, "light"), (ThemeVariant.Dark, "dark")];

    private static ToolTip Tip(Window window) => window.GetVisualDescendants().OfType<ToolTip>().First(tip => tip.IsEffectivelyVisible);

    [Fact]
    [Trait("AC", "TOOLTIP-01")]
    public async Task TheOpenTipIsShadcnsTooltipContentAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var content = (await ReferenceMetrics.SlotsAsync("tooltip-open", name, "tooltip-content"))[0];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.TooltipOpen(), variant);
                var tip = Tip(window);
                var bubble = Windows.Part<Border>(tip, "PART_LayoutRoot");
                var errors = PopoverTests.Surface($"{name} tooltip", bubble, tip.Padding, window, content, 1, 1).ToList();
                var presenter = Windows.Part<Avalonia.Controls.Presenters.ContentPresenter>(tip, "PART_ContentPresenter");
                if (!Windows.Paint(tip.Foreground).Near(CssColor.Parse(content.Color)) || tip.FontSize != 12 || presenter.GetValue(TextBlock.LineHeightProperty) != 16)
                {
                    errors.Add($"{name}: text {Windows.Paint(tip.Foreground)} {tip.FontSize}/{presenter.GetValue(TextBlock.LineHeightProperty)}, shadcn {content.Color} 12/16");
                }

                var arrow = Windows.Part<Border>(tip, "PART_Arrow");
                var center = arrow.TranslatePoint(new Point(5, 5), bubble) ?? default;
                var turned = arrow.RenderTransform is { } transform && Math.Abs(transform.Value.M12 - Math.Sqrt(0.5)) < 0.001 && Math.Abs(transform.Value.M11 - Math.Sqrt(0.5)) < 0.001;
                if (arrow.Bounds.Width != 10 || arrow.CornerRadius.TopLeft != 2 || !turned || Math.Abs(center.X - bubble.Bounds.Width / 2) > 0.5 || Math.Abs(center.Y - (bubble.Bounds.Height - 2)) > 0.5
                    || !Windows.Paint(arrow.Background).Near(Windows.Paint(bubble.Background)))
                {
                    errors.Add($"{name}: arrow {arrow.Bounds.Size} radius {arrow.CornerRadius.TopLeft} turned {turned} center {center} in {bubble.Bounds.Size}");
                }

                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }

    [Fact]
    [Trait("AC", "TOOLTIP-02")]
    public async Task TheKeyboardAndTheDelaysOpenAndCloseTipsAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var first = OverlayFixtures.Outline("First");
            var second = OverlayFixtures.Outline("Second");
            var slow = OverlayFixtures.Outline("Slow");
            var below = OverlayFixtures.Outline("Below");
            second.Margin = new Thickness(0, 100, 0, 0);
            slow.Margin = new Thickness(0, 200, 0, 0);
            below.Margin = new Thickness(0, 300, 0, 0);
            ToolTip.SetTip(first, "First tip");
            ToolTip.SetTip(second, "Second tip");
            ToolTip.SetTip(slow, "Slow tip");
            ToolTip.SetTip(below, "Below tip");
            ToolTip.SetShowDelay(second, 700);
            ToolTip.SetShowDelay(slow, 700);
            ToolTip.SetPlacement(below, PlacementMode.Bottom);
            var window = Windows.Fixture(new Panel { Children = { first, second, slow, below } }, 500, ThemeVariant.Light);
            var steps = new List<string>();
            first.Focus(NavigationMethod.Tab);
            Windows.Settle();
            steps.Add($"tab {ToolTip.GetIsOpen(first)}");
            Windows.Key(window, Key.Escape);
            steps.Add($"escape {ToolTip.GetIsOpen(first)}");
            first.Focus(NavigationMethod.Tab);
            Windows.Settle();
            slow.Focus(NavigationMethod.Pointer);
            Windows.Settle();
            steps.Add($"blur {ToolTip.GetIsOpen(first)}");
            Windows.Hover(window, slow);
            steps.Add($"slow {ToolTip.GetIsOpen(slow)}");
            Windows.Away(window, new Point(600, 480));
            Windows.Hover(window, first);
            steps.Add($"hover {ToolTip.GetIsOpen(first)}");
            Windows.Hover(window, second);
            steps.Add($"skip {ToolTip.GetIsOpen(second)}");
            Windows.Hover(window, below);
            var tip = Tip(window);
            steps.Add($"below {Windows.Box(tip, window).Y >= Windows.Box(below, window).Bottom}");
            window.Close();
            return steps;
        });

        Assert.Equal(["tab True", "escape False", "blur False", "slow False", "hover True", "skip True", "below True"], found);
    }

    [Fact]
    [Trait("AC", "TOOLTIP-03")]
    public async Task TipsShowFormattedAndTemplatedModelValuesAsync()
    {
        var found = await desktop.RunAsync(() =>
        {
            var model = new OverlaysModel();
            var view = new OverlaysView { DataContext = model };
            var window = Windows.Show(view, ThemeVariant.Light);
            var texts = new List<string?>();
            Windows.Hover(window, view.FindControl<Button>("UnreadTip")!);
            texts.Add(Tip(window).GetVisualDescendants().OfType<TextBlock>().First().Text);
            model.Unread = 5;
            Windows.Settle();
            texts.Add(Tip(window).GetVisualDescendants().OfType<TextBlock>().First().Text);
            Windows.Hover(window, view.FindControl<Button>("NoteTip")!);
            texts.Add(Tip(window).GetVisualDescendants().OfType<TextBlock>().First().Text);
            window.Close();
            return texts;
        });

        Assert.Equal(["3 unread", "5 unread", "Note: Saved"], found);
    }

    [Fact]
    [Trait("AC", "KBD-05")]
    public async Task AKeyInATipIsShadcnsTooltipKeyAsync()
    {
        var wrong = new List<string>();
        foreach (var (variant, name) in Themes)
        {
            var expected = (await ReferenceMetrics.SlotsAsync("kbd-tooltip-open", name, "kbd"))[0];
            wrong.AddRange(await desktop.RunAsync(() =>
            {
                var window = OverlayFixtures.Show(OverlayFixtures.KbdTooltipOpen(), variant);
                var key = Tip(window).GetVisualDescendants().OfType<Aioros.Cosmo.Display.Kbd>().Single();
                var border = Windows.Part<Border>(key);
                var errors = Display.Looks.Near($"{name} key", Windows.Box(key, window), expected, 1)
                    .Concat(Display.Looks.Paints($"{name} key", border.Background, expected.Background))
                    .Concat(Display.Looks.Paints($"{name} key text", key.Foreground, expected.Color)).ToList();
                window.Close();
                return errors;
            }));
        }

        Assert.Empty(wrong);
    }
}
