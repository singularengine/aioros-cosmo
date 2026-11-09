using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using Avalonia;
using Avalonia.Data.Converters;
using Gallery.Models;

namespace Aioros.Cosmo.Tests.Display;

/// <summary>A link of a bound navigation menu.</summary>
public sealed record LinkModel(string Title, string Page);

/// <summary>A plain view model for the scroll area, the table, the navigation menu and the menubar.</summary>
public sealed class StructureModel : INotifyPropertyChanged
{
    private Vector _offset;
    private Invoice? _selected;
    private string _current = "Home";
    private bool _bookmarks;

    public StructureModel()
    {
        Top = new Desktop.Command(() => Offset = default);
        Add = new Desktop.Command(() =>
        {
            Invoices.Add(new Invoice { Number = $"INV00{Invoices.Count + 1}", Status = "Pending", Method = "PayPal", Amount = "$100.00" });
            OnPropertyChanged(nameof(Total));
        });
        Navigate = new Desktop.Command(parameter => Current = parameter as string ?? Current);
        Act = new Desktop.Command(parameter => Actions.Add(parameter as string ?? string.Empty));
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>The badge class of a payment status.</summary>
    public static readonly IValueConverter Paid = new FuncValueConverter<string, bool>(status => status == "Paid");

    public IReadOnlyList<string> Tags { get; } = [.. Enumerable.Range(1, 50).Reverse().Select(n => $"v1.2.0-beta.{n}")];

    public ObservableCollection<Invoice> Invoices { get; } =
    [
        new() { Number = "INV001", Status = "Paid", Method = "Credit Card", Amount = "$250.00" },
        new() { Number = "INV002", Status = "Pending", Method = "PayPal", Amount = "$150.00" },
        new() { Number = "INV003", Status = "Unpaid", Method = "Bank Transfer", Amount = "$350.00" },
        new() { Number = "INV004", Status = "Paid", Method = "Credit Card", Amount = "$450.00" },
        new() { Number = "INV005", Status = "Paid", Method = "PayPal", Amount = "$550.00" },
        new() { Number = "INV006", Status = "Pending", Method = "Bank Transfer", Amount = "$200.00" },
        new() { Number = "INV007", Status = "Unpaid", Method = "Credit Card", Amount = "$300.00" },
    ];

    public string Total => Invoices.Sum(invoice => decimal.Parse(invoice.Amount.TrimStart('$'), CultureInfo.InvariantCulture)).ToString("$#,0.00", CultureInfo.InvariantCulture);

    public IReadOnlyList<LinkModel> Links { get; } = [new("Introduction", "/docs"), new("Installation", "/docs/installation"), new("Typography", "/docs/typography")];

    public List<string> Actions { get; } = [];

    public System.Windows.Input.ICommand Top { get; }

    public System.Windows.Input.ICommand Add { get; }

    public System.Windows.Input.ICommand Navigate { get; }

    public System.Windows.Input.ICommand Act { get; }

    public Vector Offset { get => _offset; set => Set(ref _offset, value); }

    public Invoice? Selected { get => _selected; set => Set(ref _selected, value); }

    public string Current { get => _current; set => Set(ref _current, value); }

    public bool Bookmarks { get => _bookmarks; set => Set(ref _bookmarks, value); }

    private void Set<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (!EqualityComparer<T>.Default.Equals(field, value))
        {
            field = value;
            OnPropertyChanged(name);
        }
    }

    private void OnPropertyChanged(string? name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
