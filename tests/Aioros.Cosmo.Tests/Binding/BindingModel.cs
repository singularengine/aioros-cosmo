using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Avalonia.Data.Converters;
using Aioros.Cosmo.Tests.Desktop;

namespace Aioros.Cosmo.Tests.Binding;

/// <summary>A plan a badge shows through its content template.</summary>
public sealed record PlanModel(string Name, string Price);

/// <summary>A plain view model: a counter changed by a command with a parameter, and a nullable note.</summary>
public sealed class BindingModel : INotifyPropertyChanged
{
    private int _count;

    private string? _note = "note";

    public BindingModel() => Add = new Command(parameter =>
    {
        Runs++;
        LastParameter = parameter;
        Count += int.Parse((string)parameter!, CultureInfo.InvariantCulture);
    });

    public event PropertyChangedEventHandler? PropertyChanged;

    public ICommand Add { get; }

    public int Runs { get; private set; }

    public object? LastParameter { get; private set; }

    public PlanModel Plan { get; } = new("Pro", "$12 per seat");

    public int Count
    {
        get => _count;
        set => Set(ref _count, value);
    }

    public string? Note
    {
        get => _note;
        set => Set(ref _note, value);
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}

/// <summary>Whether a count is over five.</summary>
public sealed class OverFive : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) => value is int count && count > 5;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
