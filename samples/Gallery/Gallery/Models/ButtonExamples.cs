using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>The Button page's model: a command whose parameter the page shows once it ran.</summary>
public sealed partial class ButtonExamples : ObservableObject
{
    [ObservableProperty]
    public partial string Status { get; set; } = "Not saved yet";

    [RelayCommand]
    private void Save(string? kind) => Status = $"Saved as {kind}";
}
