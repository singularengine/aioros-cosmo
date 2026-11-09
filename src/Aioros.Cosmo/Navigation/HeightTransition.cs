using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Aioros.Cosmo.Navigation;

/// <summary>
/// Radix's accordion animation (<c>accordion-down</c> and <c>accordion-up</c>: the content's height from 0 to its full
/// height and back, 200 ms, <c>ease-out</c>) as an Avalonia page transition for <see cref="Expander.ContentTransition"/>:
/// the content stays visible while it closes, and its height is the layout's again once the transition ends. Frames come
/// from the top level's animation frames, times from <see cref="Time"/>.
/// </summary>
public sealed class HeightTransition : IPageTransition
{
    /// <summary>How long the content takes to open or close (200 ms).</summary>
    public TimeSpan Duration { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>The curve of the height over time (CSS <c>ease-out</c>, <c>cubic-bezier(0, 0, 0.58, 1)</c>).</summary>
    public Easing Easing { get; set; } = new SplineEasing(0, 0, 0.58, 1);

    /// <summary>The clock the transition reads.</summary>
    public TimeProvider Time { get; set; } = TimeProvider.System;

    /// <inheritdoc />
    public async Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
    {
        if ((to ?? from) is not Layoutable content)
        {
            return;
        }

        var opening = to is not null;
        content.SetCurrentValue(Visual.IsVisibleProperty, true);
        content.Measure(new Size(content.Bounds.Width > 0 ? content.Bounds.Width : double.PositiveInfinity, double.PositiveInfinity));
        var full = content.DesiredSize.Height;
        var started = Time.GetTimestamp();
        try
        {
            while (!cancellationToken.IsCancellationRequested && TopLevel.GetTopLevel(content) is { } top)
            {
                content.SetCurrentValue(Visual.IsVisibleProperty, true);
                var progress = Math.Clamp(Time.GetElapsedTime(started) / Duration, 0, 1);
                var eased = Easing.Ease(progress);
                content.Height = full * (opening ? eased : 1 - eased);
                if (progress >= 1)
                {
                    break;
                }

                await FrameAsync(top);
            }
        }
        finally
        {
            content.ClearValue(Layoutable.HeightProperty);
            content.SetCurrentValue(Visual.IsVisibleProperty, opening);
        }
    }

    private static Task FrameAsync(TopLevel top)
    {
        var frame = new TaskCompletionSource();
        top.RequestAnimationFrame(_ => frame.TrySetResult());
        return frame.Task;
    }
}
