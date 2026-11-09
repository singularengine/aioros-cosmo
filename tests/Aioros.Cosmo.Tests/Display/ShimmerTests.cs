using Aioros.Cosmo.Display;
using Aioros.Cosmo.Overlays;
using Aioros.Cosmo.Tests.Desktop;
using Aioros.Markup;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Styling;

namespace Aioros.Cosmo.Tests.Display;

public sealed class ShimmerTests(DesktopSession desktop)
{
    private static readonly Color Text = Color.Parse("#737373");

    /// <summary>Where the sweep's highlight is across the text: the middle of the gradient's line.</summary>
    private static double Highlight(IBrush? brush) => brush is LinearGradientBrush gradient ? (gradient.StartPoint.Point.X + gradient.EndPoint.Point.X) / 2 : double.NaN;

    private static List<string> Sweep(ThemeVariant variant, FlowDirection direction, bool reduced)
    {
        var text = new TextBlock { Text = "Uploading the quarterly report", Foreground = new SolidColorBrush(Text), FontSize = 14, FlowDirection = direction };
        var window = Windows.Show(new StackPanel { Children = { text } }, variant);
        Motion.SetIsReduced(window, reduced);
        Web.SetShimmer(text, true);
        try
        {
            Windows.Clock!.Pulse(TimeSpan.Zero);
            var what = $"{variant} {direction} reduced {reduced}";
            if (reduced)
            {
                return text.Foreground is ISolidColorBrush { Color: var still } && still == Text ? [] : [$"{what}: {text.Foreground}"];
            }

            if (text.Foreground is not LinearGradientBrush gradient || gradient.GradientStops.Count != 5)
            {
                return [$"{what}: {text.Foreground}"];
            }

            var errors = Look(gradient, variant, what);

            var width = text.Bounds.Width;
            var ch = new Avalonia.Media.TextFormatting.TextLayout("0", new Typeface(text.FontFamily), 14, null).WidthIncludingTrailingWhitespace;
            var spread = 3 * ch + 40;
            var first = Highlight(text.Foreground);
            Windows.Clock.Pulse(TimeSpan.FromSeconds(1));
            var half = Highlight(text.Foreground);
            var (from, to) = direction == FlowDirection.RightToLeft ? (width + spread, -spread) : (-spread, width + spread);
            errors.AddRange(Math.Abs(first - from) <= 1 ? [] : [$"{what}: highlight starts at {first:0.##}, expected {from:0.##}"]);
            errors.AddRange(Math.Abs(half - (from + to) / 2) <= 1 ? [] : [$"{what}: highlight after 1 s at {half:0.##}, expected {(from + to) / 2:0.##}"]);
            Windows.Clock.Pulse(TimeSpan.FromSeconds(1));
            errors.AddRange(Math.Abs(Highlight(text.Foreground) - from) <= 1 ? [] : [$"{what}: after 2 s at {Highlight(text.Foreground):0.##}, expected {from:0.##} again"]);
            return errors;
        }
        finally
        {
            window.Close();
        }
    }

    private static List<string> Look(LinearGradientBrush gradient, ThemeVariant variant, string what)
    {
        var errors = new List<string>();
        var basis = Oklch.From(Text);
        var expected = variant == ThemeVariant.Dark
            ? basis with { L = Math.Max(0.8, basis.L + 0.4), Alpha = 1 }
            : basis with { Alpha = 0.2 };
        var middle = gradient.GradientStops[2];
        var outer = gradient.GradientStops[0];
        errors.AddRange(Near(middle.Color, expected.ToColor()) && middle.Offset == 0.5 ? [] : [$"{what}: highlight {middle.Color} at {middle.Offset}, expected {expected.ToColor()}"]);
        errors.AddRange(outer.Color == Text && gradient.GradientStops[4].Color == Text ? [] : [$"{what}: edges {outer.Color}"]);
        var direction110 = gradient.EndPoint.Point - gradient.StartPoint.Point;
        var angle = Math.Atan2(direction110.X, -direction110.Y) * 180 / Math.PI;
        errors.AddRange(Math.Abs(angle - 110) < 0.01 ? [] : [$"{what}: angle {angle}"]);
        return errors;
    }

    private static bool Near(Color a, Color b) => Math.Abs(a.R - b.R) <= 1 && Math.Abs(a.G - b.G) <= 1 && Math.Abs(a.B - b.B) <= 1 && Math.Abs(a.A - b.A) <= 1;

    [Fact]
    [Trait("AC", "THEME-09")]
    public async Task AShimmeringTextIsSweptByShadcnsHighlightAsync()
    {
        var wrong = await desktop.RunAsync(() =>
        {
            Windows.Clocked = true;
            try
            {
                return Sweep(ThemeVariant.Light, FlowDirection.LeftToRight, reduced: false)
                    .Concat(Sweep(ThemeVariant.Dark, FlowDirection.LeftToRight, reduced: false))
                    .Concat(Sweep(ThemeVariant.Light, FlowDirection.RightToLeft, reduced: false))
                    .Concat(Sweep(ThemeVariant.Light, FlowDirection.LeftToRight, reduced: true))
                    .ToList();
            }
            finally
            {
                Windows.Clocked = false;
            }
        });

        Assert.True(wrong.Count == 0, string.Join(Environment.NewLine, wrong));
    }
}
