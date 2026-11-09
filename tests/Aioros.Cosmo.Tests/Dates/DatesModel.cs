using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aioros.Cosmo.Tests.Dates;

/// <summary>A plain view model for the calendars: one day, a range and several days.</summary>
public sealed class DatesModel : INotifyPropertyChanged
{
    private DateTime? _day = June(12);
    private DateTime? _start = June(9);
    private DateTime? _end = June(26);
    private IReadOnlyList<DateTime>? _days = [June(3), June(12)];
    private DateTime? _from;
    private DateTime? _to;

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>A day of June 2025, the month shadcn/ui's calendar fixtures show.</summary>
    public static DateTime June(int day) => new(2025, 6, day, 0, 0, 0, DateTimeKind.Unspecified);

    public DateTime? Day
    {
        get => _day;
        set => Set(ref _day, value);
    }

    public DateTime? Start
    {
        get => _start;
        set => Set(ref _start, value);
    }

    public DateTime? End
    {
        get => _end;
        set => Set(ref _end, value);
    }

    public DateTime? From
    {
        get => _from;
        set => Set(ref _from, value);
    }

    public DateTime? To
    {
        get => _to;
        set => Set(ref _to, value);
    }

    public IReadOnlyList<DateTime>? Days
    {
        get => _days;
        set => Set(ref _days, value);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
