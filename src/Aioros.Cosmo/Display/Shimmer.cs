using Aioros.Cosmo.Overlays;
using Aioros.Markup;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.TextFormatting;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's <c>shimmer</c> utility on the desktop for a <see cref="TextBlock"/> marked with
/// <see cref="Web.ShimmerProperty"/> (inherited, so a control's texts follow it), the same hint Aioros writes as the
/// utility on the web: the text's color swept by a lighter highlight along a 110° gradient
/// <c>200% + 2 × (3ch + 40px)</c> wide, its position moved from <c>100%</c> to <c>0</c> over two seconds, linear and
/// endless, reversed right to left; the highlight is the text's color at a fifth of its alpha (lightness at least 0.8 and
/// alpha plus 0.4 in the dark theme). Still with <see cref="Motion.IsReducedProperty"/> and while Aioros writes the tree
/// for the web. Cosmo's theme registers it.
/// </summary>
internal static class Shimmer
{
    /// <summary>Where the sweep is, from 0 (the gradient's position at 100%) to 1 (at 0), the property the animation drives.</summary>
    internal static readonly AttachedProperty<double> PhaseProperty = AvaloniaProperty.RegisterAttached<TextBlock, double>("Phase", typeof(Shimmer));

    private static readonly AttachedProperty<Running?> RunningProperty = AvaloniaProperty.RegisterAttached<TextBlock, Running?>("Running", typeof(Shimmer));

    private static readonly Animation Sweep = new()
    {
        Duration = TimeSpan.FromSeconds(2),
        IterationCount = IterationCount.Infinite,
        Children =
        {
            new KeyFrame { Cue = new Cue(0), Setters = { new Setter(PhaseProperty, 0d) } },
            new KeyFrame { Cue = new Cue(1), Setters = { new Setter(PhaseProperty, 1d) } },
        },
    };

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        Web.ShimmerProperty.Changed.AddClassHandler<TextBlock>((text, change) =>
        {
            text.AttachedToVisualTree -= Attached;
            text.DetachedFromVisualTree -= Detached;
            if (change.GetNewValue<bool>())
            {
                text.AttachedToVisualTree += Attached;
                text.DetachedFromVisualTree += Detached;
            }

            Update(text);
        });
        Motion.IsReducedProperty.Changed.AddClassHandler<TextBlock>((text, _) => Update(text));
        PhaseProperty.Changed.AddClassHandler<TextBlock>((text, _) => Paint(text));
        Visual.BoundsProperty.Changed.AddClassHandler<TextBlock>((text, _) => Paint(text));
    }

    private static void Attached(object? sender, VisualTreeAttachmentEventArgs e) => Update((TextBlock)sender!);

    private static void Detached(object? sender, VisualTreeAttachmentEventArgs e) => Stop((TextBlock)sender!);

    /// <summary>The brush the text is painted with at a phase of the sweep, for its base color, size and font.</summary>
    internal static IBrush Brush(Color color, bool dark, Size size, double ch, double phase, bool rightToLeft)
    {
        var spread = 3 * ch + 40;
        var width = 2 * size.Width + 2 * spread;
        var position = rightToLeft ? phase : 1 - phase;
        var left = position * (size.Width - width);
        const double Angle = 110 * Math.PI / 180;
        var direction = new Vector(Math.Sin(Angle), -Math.Cos(Angle));
        var length = Math.Abs(width * Math.Sin(Angle)) + Math.Abs(size.Height * Math.Cos(Angle));
        var center = new Point(left + width / 2, size.Height / 2);
        var start = center - direction * (length / 2);
        var end = center + direction * (length / 2);
        var basis = Oklch.From(color);
        var highlight = dark
            ? basis with { L = Math.Max(0.8, basis.L + 0.4), Alpha = Math.Min(1, basis.Alpha + 0.4) }
            : basis with { Alpha = basis.Alpha * 0.2 };
        var middle = highlight.Mix(basis, 0.5).ToColor();
        var edge = length <= 0 ? 0 : spread / length;
        return new LinearGradientBrush
        {
            StartPoint = new RelativePoint(start, RelativeUnit.Absolute),
            EndPoint = new RelativePoint(end, RelativeUnit.Absolute),
            GradientStops =
            {
                new GradientStop(color, 0.5 - edge),
                new GradientStop(middle, 0.5 - edge / 2),
                new GradientStop(highlight.ToColor(), 0.5),
                new GradientStop(middle, 0.5 + edge / 2),
                new GradientStop(color, 0.5 + edge),
            },
        };
    }

    private static bool Shimmers(TextBlock text) =>
        Web.GetShimmer(text) && !Motion.GetIsReduced(text) && !Web.GetIsWritten(text) && text.IsAttachedToVisualTree();

    private static void Update(TextBlock text)
    {
        if (!Shimmers(text))
        {
            Stop(text);
            return;
        }

        if (text.GetValue(RunningProperty) is not null)
        {
            return;
        }

        var running = new Running(new CancellationTokenSource());
        text.SetValue(RunningProperty, running);
        _ = Sweep.RunAsync(text, running.Cancel.Token);
        Paint(text);
    }

    private static void Stop(TextBlock text)
    {
        if (text.GetValue(RunningProperty) is not { } running)
        {
            return;
        }

        running.Cancel.Cancel();
        running.Cancel.Dispose();
        running.Look?.Dispose();
        text.ClearValue(RunningProperty);
        text.ClearValue(PhaseProperty);
    }

    private static void Paint(TextBlock text)
    {
        if (text.GetValue(RunningProperty) is not { } running || text.Bounds.Width <= 0)
        {
            return;
        }

        running.Look?.Dispose();
        running.Look = null;
        if (text.Foreground is not ISolidColorBrush solid)
        {
            return;
        }

        var brush = Brush(solid.Color, text.ActualThemeVariant == ThemeVariant.Dark, text.Bounds.Size, Ch(text), text.GetValue(PhaseProperty), text.FlowDirection == FlowDirection.RightToLeft);
        running.Look = text.SetValue(TextBlock.ForegroundProperty, brush, BindingPriority.Animation);
    }

    private static double Ch(TextBlock text) =>
        new TextLayout("0", new Typeface(text.FontFamily, text.FontStyle, text.FontWeight, text.FontStretch), text.FontSize, null).WidthIncludingTrailingWhitespace;

    private sealed class Running(CancellationTokenSource cancel)
    {
        public CancellationTokenSource Cancel { get; } = cancel;

        public IDisposable? Look { get; set; }
    }
}
