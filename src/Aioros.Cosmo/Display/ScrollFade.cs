using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's <c>scroll-fade</c> utilities on the desktop for a <see cref="ScrollViewer"/> marked with
/// <see cref="Web.ScrollFadeProperty"/>, the same hint Aioros writes as the utilities on the web: an opacity mask fading
/// the content out at its scrolled edges (<c>y</c> the top and the bottom, <c>x</c> the start and the end, or one of
/// <c>t</c>, <c>b</c>, <c>s</c>, <c>e</c>), each fade <c>min(12%, 40px)</c> long, grown with <c>ease-in-out</c> over
/// the first and last 96 px of the scroll and absent while the content fits. Cosmo's theme registers it.
/// </summary>
internal static class ScrollFade
{
    private const double Reveal = 96;

    private static bool _registered;

    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        Web.ScrollFadeProperty.Changed.AddClassHandler<ScrollViewer>((scroll, _) => Paint(scroll));
        foreach (var property in (AvaloniaProperty[])[ScrollViewer.OffsetProperty, ScrollViewer.ExtentProperty, ScrollViewer.ViewportProperty, Visual.BoundsProperty])
        {
            property.Changed.AddClassHandler<ScrollViewer>((scroll, _) =>
            {
                if (Web.GetScrollFade(scroll) is not null)
                {
                    Paint(scroll);
                }
            });
        }
    }

    /// <summary>The lengths of the fades at the start and the end of an axis, from its scroll offset, its scroll range and its length.</summary>
    internal static (double Start, double End) Fades(double offset, double range, double length)
    {
        if (range <= 0.5)
        {
            return (0, 0);
        }

        var size = Math.Min(0.12 * length, 40);
        var start = size * EaseInOut(Math.Clamp(offset / Reveal, 0, 1));
        var end = size * (1 - EaseInOut(Math.Clamp((offset - (range - Reveal)) / Reveal, 0, 1)));
        return (start, end);
    }

    private static void Paint(ScrollViewer scroll)
    {
        var kind = Web.GetScrollFade(scroll);
        var vertical = kind is "y" or "t" or "b";
        var length = vertical ? scroll.Bounds.Height : scroll.Bounds.Width;
        if (kind is not ("y" or "x" or "t" or "b" or "s" or "e") || length <= 0)
        {
            scroll.ClearValue(Visual.OpacityMaskProperty);
            return;
        }

        var (start, end) = vertical
            ? Fades(scroll.Offset.Y, Range(scroll, vertical: true), length)
            : Fades(scroll.Offset.X, Range(scroll, vertical: false), length);
        start = kind is "b" or "e" ? 0 : start;
        end = kind is "t" or "s" ? 0 : end;
        if (start <= 0 && end <= 0)
        {
            scroll.ClearValue(Visual.OpacityMaskProperty);
            return;
        }

        scroll.OpacityMask = new LinearGradientBrush
        {
            StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
            EndPoint = vertical ? new RelativePoint(0, 1, RelativeUnit.Relative) : new RelativePoint(1, 0, RelativeUnit.Relative),
            GradientStops =
            {
                new GradientStop(Colors.Transparent, 0),
                new GradientStop(Colors.Black, start / length),
                new GradientStop(Colors.Black, 1 - end / length),
                new GradientStop(Colors.Transparent, 1),
            },
        };
    }

    /// <summary>
    /// How far an axis scrolls: its extent past its viewport, or, on an axis whose scrolling is disabled (CSS's
    /// <c>overflow: hidden</c>, still a scroll container whose fades the browser shows), how far its content reaches past it.
    /// </summary>
    private static double Range(ScrollViewer scroll, bool vertical)
    {
        var disabled = (vertical ? scroll.VerticalScrollBarVisibility : scroll.HorizontalScrollBarVisibility) == ScrollBarVisibility.Disabled;
        if (!disabled)
        {
            return vertical ? scroll.Extent.Height - scroll.Viewport.Height : scroll.Extent.Width - scroll.Viewport.Width;
        }

        var reach = scroll.GetVisualDescendants().OfType<Control>().Where(static control => control.IsVisible)
            .Select(control => control.TranslatePoint(new Point(control.Bounds.Width, control.Bounds.Height), scroll))
            .Where(static point => point.HasValue)
            .Select(point => vertical ? point!.Value.Y : point!.Value.X)
            .DefaultIfEmpty(0)
            .Max();
        return reach - (vertical ? scroll.Bounds.Height : scroll.Bounds.Width);
    }

    /// <summary>CSS's <c>ease-in-out</c>, <c>cubic-bezier(0.42, 0, 0.58, 1)</c>, at a progress.</summary>
    internal static double EaseInOut(double progress)
    {
        if (progress is <= 0 or >= 1)
        {
            return Math.Clamp(progress, 0, 1);
        }

        static double Curve(double t, double a, double b) => 3 * (1 - t) * (1 - t) * t * a + 3 * (1 - t) * t * t * b + t * t * t;
        double low = 0, high = 1, middle = progress;
        for (var i = 0; i < 40; i++)
        {
            middle = (low + high) / 2;
            if (Curve(middle, 0.42, 0.58) < progress)
            {
                low = middle;
            }
            else
            {
                high = middle;
            }
        }

        return Curve(middle, 0, 1);
    }
}
