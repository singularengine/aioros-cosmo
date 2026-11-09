using System.Collections.ObjectModel;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A row density a radio group chooses.</summary>
public enum Density
{
    Default,
    Comfortable,
    Compact,
}

/// <summary>The choice controls' pages' model: two-way bound flags, an enum chosen by radios, a toggle group's selection and commands reading them.</summary>
public sealed partial class ChoiceExamples : ObservableObject
{
    public ChoiceExamples() => Formats.CollectionChanged += (_, _) => OnPropertyChanged(nameof(FormatList));

    [ObservableProperty]
    public partial bool Notifications { get; set; }

    [ObservableProperty]
    public partial bool AirplaneMode { get; set; }

    [ObservableProperty]
    public partial bool Bold { get; set; }

    [ObservableProperty]
    public partial Density Density { get; set; } = Density.Comfortable;

    [ObservableProperty]
    public partial string Alignment { get; set; } = "Center";

    [ObservableProperty]
    public partial string Status { get; set; } = "Not saved yet";

    public IReadOnlyList<string> Alignments { get; } = ["Left", "Center", "Right"];

    public IReadOnlyList<string> FormatChoices { get; } = ["Bold", "Italic", "Underline"];

    /// <summary>The selected formats, bound to the multiple toggle group's <c>SelectedItems</c>.</summary>
    public ObservableCollection<string> Formats { get; } = ["Bold"];

    public string FormatList => Formats.Count == 0 ? "No format" : string.Join(", ", FormatChoices.Where(Formats.Contains));

    [RelayCommand]
    private void Save() => Status = Notifications ? "Notifications on" : "Notifications off";

    [RelayCommand]
    private void SaveAirplane() => Status = AirplaneMode ? "Airplane mode on" : "Airplane mode off";

    [RelayCommand]
    private void Apply() => Status = $"Density: {Density}";

    [RelayCommand]
    private void Align() => Status = $"Aligned {Alignment}";
}

/// <summary>Whether a bound enum is the converter's parameter, and back: checking a radio sets the model to its value.</summary>
public sealed class Is : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        string.Equals(value?.ToString(), parameter as string, StringComparison.Ordinal);

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true && parameter is string name ? Enum.Parse(targetType, name) : BindingOperations.DoNothing;
}
