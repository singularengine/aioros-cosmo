using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia.Data.Converters;

namespace Aioros.Cosmo.Tests.Composites;

/// <summary>A plain view model for the resizable group and the carousel.</summary>
public sealed class CompositesModel : INotifyPropertyChanged
{
    private double _split = 30;
    private double _minimum = 0.1;
    private int _index;

    public CompositesModel()
    {
        Reset = new Desktop.Command(() => Split = 50);
        Add = new Desktop.Command(() => Slides.Add($"Slide {Slides.Count + 1}"));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>A fraction (0.4) as a percentage (40).</summary>
    public static readonly IValueConverter Percent = new FuncValueConverter<double, double>(fraction => fraction * 100);

    /// <summary>The 0-based index and the count as <c>Slide 2 of 5</c>.</summary>
    public static readonly IMultiValueConverter SlideOf = new FuncMultiValueConverter<int, string>(values =>
    {
        var list = values.ToList();
        return string.Format(CultureInfo.InvariantCulture, "Slide {0} of {1}", list[0] + 1, list[1]);
    });

    public System.Windows.Input.ICommand Reset { get; }

    public System.Windows.Input.ICommand Add { get; }

    public ObservableCollection<string> Slides { get; } = ["Slide 1", "Slide 2", "Slide 3", "Slide 4", "Slide 5"];

    public double Split
    {
        get => _split;
        set => Set(ref _split, value);
    }

    public double Minimum
    {
        get => _minimum;
        set => Set(ref _minimum, value);
    }

    public int Index
    {
        get => _index;
        set => Set(ref _index, value);
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
