using CommunityToolkit.Mvvm.ComponentModel;

namespace Gallery.Models;

/// <summary>The Calendar page's model: each example's selection, bound two way as shadcn/ui's examples keep theirs in state.</summary>
public sealed partial class DateExamples : ObservableObject
{
    [ObservableProperty]
    public partial DateTime? Day { get; set; } = June(12);

    [ObservableProperty]
    public partial DateTime? From { get; set; } = June(9);

    [ObservableProperty]
    public partial DateTime? To { get; set; } = June(26);

    [ObservableProperty]
    public partial IReadOnlyList<DateTime>? Days { get; set; } = [June(3), June(12), June(13), June(24)];

    [ObservableProperty]
    public partial DateTime? Inside { get; set; } = June(12);

    [ObservableProperty]
    public partial DateTime? Chosen { get; set; } = June(12);

    [ObservableProperty]
    public partial DateTime? Numbered { get; set; } = June(12);

    private static DateTime June(int day) => new(2025, 6, day, 0, 0, 0, DateTimeKind.Unspecified);
}
