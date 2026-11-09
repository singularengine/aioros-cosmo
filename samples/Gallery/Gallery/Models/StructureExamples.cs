using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Gallery.Models;

/// <summary>A link of a bound navigation menu: its title, description and page.</summary>
public sealed record MenuLink(string Title, string Description, string Page);

/// <summary>The scroll area, table, navigation menu and menubar pages' model.</summary>
public sealed partial class StructureExamples : ObservableObject
{
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

    public string Count => $"{Invoices.Count} invoices";

    [ObservableProperty]
    public partial Invoice? SelectedInvoice { get; set; }

    public IReadOnlyList<MenuLink> Links { get; } =
    [
        new("Introduction", "Re-usable components built using Radix UI and Tailwind CSS.", "/docs"),
        new("Installation", "How to install dependencies and structure your app.", "/docs/installation"),
        new("Typography", "Styles for headings, paragraphs, lists...etc", "/docs/primitives/typography"),
    ];

    [ObservableProperty]
    public partial string CurrentPage { get; set; } = "/";

    [ObservableProperty]
    public partial bool ShowBookmarks { get; set; }

    [ObservableProperty]
    public partial string LastAction { get; set; } = "Nothing yet";

    [RelayCommand]
    private void AddInvoice()
    {
        Invoices.Add(new Invoice { Number = $"INV{Invoices.Count + 1:000}", Status = "Pending", Method = "PayPal", Amount = "$100.00" });
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(Count));
    }

    [RelayCommand]
    private void Navigate(string? page) => CurrentPage = page ?? CurrentPage;

    [RelayCommand]
    private void Act(string? action) => LastAction = action ?? LastAction;
}
