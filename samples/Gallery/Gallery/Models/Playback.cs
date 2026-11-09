using Avalonia.Threading;

namespace Gallery.Models;

/// <summary>
/// Plays a scripted stream back on a clock, as shadcn/ui's examples play their mock chat transport: each step waits its
/// delay on <see cref="Clock"/> (the system's, or a fake one in tests), then runs on Avalonia's thread, which the page
/// and its live stream read. Playing again drops what was left of the previous script.
/// </summary>
public sealed class Playback
{
    private int version;

    /// <summary>The clock the steps wait on.</summary>
    public TimeProvider Clock { get; set; } = TimeProvider.System;

    /// <summary>Plays the steps, each after its delay from the one before.</summary>
    /// <param name="script">The steps and their delays.</param>
    public void Play(IEnumerable<(TimeSpan Delay, Action Step)> script) =>
        Next(new Queue<(TimeSpan, Action)>(script), Interlocked.Increment(ref version));

    /// <summary>Drops what is left of the script playing, as its page is gone.</summary>
    public void Stop() => Interlocked.Increment(ref version);

    private void Next(Queue<(TimeSpan Delay, Action Step)> steps, int playing)
    {
        while (steps.TryPeek(out var step) && step.Delay <= TimeSpan.Zero)
        {
            steps.Dequeue();
            Post(step.Step, playing);
        }

        if (!steps.TryDequeue(out var waiting))
        {
            return;
        }

        ITimer? timer = null;
        timer = Clock.CreateTimer(_ =>
        {
            timer?.Dispose();
            if (Volatile.Read(ref version) == playing)
            {
                Post(waiting.Step, playing);
                Next(steps, playing);
            }
        }, null, waiting.Delay, Timeout.InfiniteTimeSpan);
    }

    private void Post(Action step, int playing) =>
        Dispatcher.UIThread.Post(() =>
        {
            if (Volatile.Read(ref version) == playing)
            {
                step();
            }
        });
}
