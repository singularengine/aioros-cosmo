using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>The Slider, Progress, Input Group and Input OTP pages' model: bound numbers (a range's two values among them) and texts, and commands reading them.</summary>
public sealed partial class RangeExamples : ObservableObject
{
    [ObservableProperty]
    public partial double Volume { get; set; } = 50;

    [ObservableProperty]
    public partial double Done { get; set; } = 60;

    [ObservableProperty]
    public partial double Low { get; set; } = 200;

    [ObservableProperty]
    public partial double High { get; set; } = 800;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Count))]
    public partial string Query { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Code { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Status { get; set; } = "Nothing applied yet";

    public string Count => $"{Query.Length}/24";

    [RelayCommand]
    private void Apply() => Status = $"Volume {Volume:0}";

    [RelayCommand]
    private void ApplyRange() => Status = $"Budget ${Low:0} - ${High:0}";

    [RelayCommand]
    private void Advance() => Done = Math.Min(100, Done + 10);

    [RelayCommand]
    private void Verify() => Status = Code.Length == 6 ? $"Verified {Code}" : "Enter the 6 digits";
}
