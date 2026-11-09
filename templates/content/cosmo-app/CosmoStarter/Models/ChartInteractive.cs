using System.ComponentModel;
using System.Globalization;

namespace CosmoStarter.Models;

/// <summary>
/// shadcn/ui's interactive chart examples: the last three months of daily visitors, shown over the time range the
/// area chart's select chooses, and the series the bar and line charts' header toggles choose.
/// </summary>
public sealed class ChartInteractive : INotifyPropertyChanged
{
    private static readonly DateTime Reference = new(2024, 6, 30, 0, 0, 0, DateTimeKind.Unspecified);

    private static readonly int[] Days = [90, 30, 7];

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The daily visitors of shadcn/ui's interactive examples.</summary>
    public IReadOnlyList<ChartRow> Daily { get; } = VisitorSamples.Daily;

    /// <summary>The chosen time range: the last 3 months, 30 days or 7 days (shadcn/ui's <c>90d</c>, <c>30d</c>, <c>7d</c>).</summary>
    public int TimeRange
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(TimeRange), nameof(Range));
            }
        }
    }

    /// <summary>The chosen series: desktop (0) or mobile (1).</summary>
    public int ActiveChart
    {
        get;
        set
        {
            if (field != value)
            {
                field = value;
                Changed(nameof(ActiveChart), nameof(DesktopShown), nameof(MobileShown));
            }
        }
    }

    /// <summary>The days of the chosen time range: from its count of days before June 30, 2024.</summary>
    public IReadOnlyList<ChartRow> Range => [.. Daily.Where(row => row.Date >= Reference.AddDays(-Days[Math.Clamp(TimeRange, 0, Days.Length - 1)]))];

    /// <summary>Whether the desktop series is shown.</summary>
    public bool DesktopShown => ActiveChart == 0;

    /// <summary>Whether the mobile series is shown.</summary>
    public bool MobileShown => ActiveChart == 1;

    /// <summary>The desktop visitors of the three months, as the header shows them.</summary>
    public string DesktopTotal => Total(static row => row.Desktop);

    /// <summary>The mobile visitors of the three months, as the header shows them.</summary>
    public string MobileTotal => Total(static row => row.Mobile);

    private string Total(Func<ChartRow, double?> value) => Daily.Sum(row => value(row) ?? 0).ToString("#,0", CultureInfo.InvariantCulture);

    private void Changed(params string[] names)
    {
        foreach (var name in names)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
