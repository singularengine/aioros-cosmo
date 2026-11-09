using System.Runtime.CompilerServices;
using Avalonia.Animation;
using Avalonia.Controls;

namespace Aioros.Cosmo.Tests.Desktop;

/// <summary>
/// A clock a test gives a window so its transitions move only when the test pulses it, not with the platform's real
/// time: a transition begins at the first pulse after its value changed and runs as the pulses' times advance. Avalonia
/// keeps its clocks internal, so they are reached through compile-time bound accessors, as Aioros's engine reaches them.
/// </summary>
internal sealed class StillClock
{
    private const string Base = "Avalonia.Animation.ClockBase, Avalonia.Base";
    private const string Interface = "Avalonia.Animation.IClock, Avalonia.Base";

    private readonly object _clock = Create();

    private TimeSpan _now;

    public StillClock(Window window)
    {
        SetClock(window, _clock);
        Pulse(TimeSpan.Zero);
    }

    /// <summary>Moves the clock on by <paramref name="time"/> (none: starts what began since the last pulse) and runs the work it posted.</summary>
    public void Pulse(TimeSpan time)
    {
        _now += time;
        Pulse(_clock, _now);
        Avalonia.Threading.Dispatcher.UIThread.RunJobs();
    }

    [UnsafeAccessor(UnsafeAccessorKind.Constructor)]
    [return: UnsafeAccessorType(Base)]
    private static extern object Create();

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_Clock")]
    private static extern void SetClock(Animatable target, [UnsafeAccessorType(Interface)] object clock);

    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "Pulse")]
    private static extern void Pulse([UnsafeAccessorType(Base)] object clock, TimeSpan time);
}
