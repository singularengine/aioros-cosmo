namespace Aioros.Cosmo.Tests.Desktop;

/// <summary>A clock the tests move by hand: timers fire when <see cref="Advance"/> passes their due time, periodic ones again every period.</summary>
internal sealed class ManualTime : TimeProvider
{
    private readonly List<ManualTimer> _timers = [];
    private long _now;

    public override long TimestampFrequency => TimeSpan.TicksPerSecond;

    public override long GetTimestamp() => _now;

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        var timer = new ManualTimer(this, callback, state);
        timer.Change(dueTime, period);
        return timer;
    }

    /// <summary>Moves the clock and fires the timers whose time came, in order.</summary>
    public void Advance(TimeSpan time)
    {
        var end = _now + time.Ticks;
        while (_timers.Where(timer => timer.Due is { } due && due <= end).OrderBy(timer => timer.Due).FirstOrDefault() is { } next)
        {
            _now = next.Due!.Value;
            next.Due = next.Period is { } period ? _now + period : null;
            next.Fire();
        }

        _now = end;
    }

    private sealed class ManualTimer(ManualTime time, TimerCallback callback, object? state) : ITimer
    {
        public long? Due { get; set; }

        public long? Period { get; private set; }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            Due = dueTime == Timeout.InfiniteTimeSpan ? null : time._now + dueTime.Ticks;
            Period = period == Timeout.InfiniteTimeSpan || period <= TimeSpan.Zero ? null : period.Ticks;
            if (!time._timers.Contains(this))
            {
                time._timers.Add(this);
            }

            return true;
        }

        public void Fire() => callback(state);

        public void Dispose() => time._timers.Remove(this);

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
