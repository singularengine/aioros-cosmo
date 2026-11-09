using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Aioros.Cosmo.Tests.Forms;

/// <summary>A fruit a combo box shows through its item template; its code is the selected value.</summary>
public sealed record FruitModel(string Name, string Code);

/// <summary>A plain view model for the form controls: text, flags, numbers, a selection and errors reported through <see cref="INotifyDataErrorInfo"/>.</summary>
public sealed class FormsModel : INotifyPropertyChanged, INotifyDataErrorInfo
{
    private readonly Dictionary<string, List<string>> _errors = new(StringComparer.Ordinal);
    private string _name = string.Empty;
    private string _email = string.Empty;
    private int _count = 3;
    private bool _locked;
    private bool _invalid;
    private FruitModel? _fruit;
    private string? _code;
    private bool _agree;

    public FormsModel() => Toggle = new Desktop.Command(parameter =>
    {
        Toggles++;
        LastParameter = parameter;
    });

    public System.Windows.Input.ICommand Toggle { get; }

    public int Toggles { get; private set; }

    public object? LastParameter { get; private set; }

    public bool Agree { get => _agree; set => Set(ref _agree, value); }

    private bool _second;
    private bool _airplane;
    private bool _bold;
    private string? _alignment;

    public IReadOnlyList<string> Alignments { get; } = ["Left", "Center", "Right"];

    public bool Second { get => _second; set => Set(ref _second, value); }

    public bool Airplane { get => _airplane; set => Set(ref _airplane, value); }

    public bool Bold { get => _bold; set => Set(ref _bold, value); }

    public string? Alignment { get => _alignment; set => Set(ref _alignment, value); }

    private double _volume = 50;

    public double Volume { get => _volume; set => Set(ref _volume, value); }

    public event PropertyChangedEventHandler? PropertyChanged;

    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;

    public IReadOnlyList<FruitModel> Fruits { get; } = [new("Apple", "apple"), new("Banana", "banana"), new("Carrot", "carrot")];

    public bool HasErrors => _errors.Count > 0;

    public string Name { get => _name; set => Set(ref _name, value); }

    public string Email { get => _email; set => Set(ref _email, value); }

    public int Count { get => _count; set => Set(ref _count, value); }

    public bool Locked { get => _locked; set => Set(ref _locked, value); }

    public bool Invalid { get => _invalid; set => Set(ref _invalid, value); }

    public FruitModel? Fruit { get => _fruit; set => Set(ref _fruit, value); }

    public string? Code { get => _code; set => Set(ref _code, value); }

    public IEnumerable GetErrors(string? propertyName) =>
        propertyName is not null && _errors.TryGetValue(propertyName, out var errors) ? errors : Array.Empty<string>();

    /// <summary>Reports <paramref name="message"/> for a property, or clears its errors when it is null.</summary>
    public void Report(string propertyName, string? message)
    {
        if (message is null)
        {
            _errors.Remove(propertyName);
        }
        else
        {
            _errors[propertyName] = [message];
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
