using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>The Input and Textarea pages' model: two-way bound text, a bound label and a command reading the text.</summary>
public sealed partial class InputExamples : ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Greeting { get; set; } = "Nobody greeted yet";

    [ObservableProperty]
    public partial int Unread { get; set; } = 3;

    [ObservableProperty]
    public partial bool Locked { get; set; }

    [RelayCommand]
    private void Greet() => Greeting = $"Hello, {Name}";
}
