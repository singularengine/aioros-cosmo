using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace Aioros.Cosmo.Display;

/// <summary>
/// Radix ScrollArea's <c>type="hover"</c>: a scroll area's bars show while the pointer is over it and hide
/// <see cref="HideDelayProperty"/> milliseconds after it leaves (Radix's <c>scrollHideDelay</c>, 600 ms in shadcn/ui), coming
/// back at once if the pointer returns first. The bars show while the area carries the <c>bars</c> class.
/// </summary>
public static class ScrollAreaBars
{
    /// <summary>Defines the delay, in milliseconds, before the bars hide once the pointer leaves; 0 leaves the bars alone.</summary>
    public static readonly AttachedProperty<double> HideDelayProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>("HideDelay", typeof(ScrollAreaBars));

    /// <summary>Defines the clock the delay follows, inherited down the tree (the system's by default).</summary>
    public static readonly AttachedProperty<TimeProvider> ClockProperty =
        AvaloniaProperty.RegisterAttached<Control, TimeProvider>("Clock", typeof(ScrollAreaBars), TimeProvider.System, inherits: true);

    private static readonly AttachedProperty<ITimer?> TimerProperty = AvaloniaProperty.RegisterAttached<ScrollViewer, ITimer?>("Timer", typeof(ScrollAreaBars));

    static ScrollAreaBars()
    {
        HideDelayProperty.Changed.AddClassHandler<ScrollViewer>((area, e) =>
        {
            if (e.OldValue is double old && old > 0)
            {
                area.PointerEntered -= Entered;
                area.PointerExited -= Exited;
                area.DetachedFromVisualTree -= Detached;
            }

            if (e.NewValue is double delay && delay > 0)
            {
                area.PointerEntered += Entered;
                area.PointerExited += Exited;
                area.DetachedFromVisualTree += Detached;
            }
        });
    }

    /// <summary>Gets <see cref="HideDelayProperty"/>.</summary>
    public static double GetHideDelay(ScrollViewer area) => area.GetValue(HideDelayProperty);

    /// <summary>Sets <see cref="HideDelayProperty"/>.</summary>
    public static void SetHideDelay(ScrollViewer area, double value) => area.SetValue(HideDelayProperty, value);

    /// <summary>Gets <see cref="ClockProperty"/>.</summary>
    public static TimeProvider GetClock(Control control) => control.GetValue(ClockProperty);

    /// <summary>Sets <see cref="ClockProperty"/>.</summary>
    public static void SetClock(Control control, TimeProvider value) => control.SetValue(ClockProperty, value);

    private static void Entered(object? sender, EventArgs e)
    {
        var area = (ScrollViewer)sender!;
        Stop(area);
        area.Classes.Set("bars", true);
    }

    private static void Exited(object? sender, EventArgs e)
    {
        var area = (ScrollViewer)sender!;
        Stop(area);
        ITimer? timer = null;
        timer = GetClock(area).CreateTimer(_ => Dispatcher.UIThread.Post(() =>
        {
            if (area.GetValue(TimerProperty) == timer)
            {
                Stop(area);
                area.Classes.Set("bars", false);
            }
        }), null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
        area.SetValue(TimerProperty, timer);
        timer.Change(TimeSpan.FromMilliseconds(GetHideDelay(area)), Timeout.InfiniteTimeSpan);
    }

    private static void Detached(object? sender, EventArgs e)
    {
        var area = (ScrollViewer)sender!;
        Stop(area);
        area.Classes.Set("bars", false);
    }

    private static void Stop(ScrollViewer area)
    {
        area.GetValue(TimerProperty)?.Dispose();
        area.SetValue(TimerProperty, null);
    }
}
