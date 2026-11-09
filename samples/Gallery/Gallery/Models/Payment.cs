using System.ComponentModel;
using System.Globalization;
using Aioros.Cosmo.Tables;
using Avalonia.Data.Converters;

namespace Gallery.Models;

/// <summary>A payment of shadcn/ui's data table example: its id, amount, status and email, and its selection.</summary>
public sealed class Payment : IDataTableRow
{
    private bool _selected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Id { get; init; } = string.Empty;

    public decimal Amount { get; init; }

    public string Status { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public bool IsSelected
    {
        get => _selected;
        set
        {
            if (_selected != value)
            {
                _selected = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
            }
        }
    }
}

/// <summary>shadcn/ui's data table example: five payments, filtered by email, sorted, paged and selected through a <see cref="DataTableSource{T}"/>.</summary>
public sealed class PaymentTable
{
    /// <summary>A status as shadcn's cell shows it (<c>capitalize</c>).</summary>
    public static readonly IValueConverter Capitalize = new FuncValueConverter<string?, string>(text => string.IsNullOrEmpty(text) ? string.Empty : char.ToUpperInvariant(text[0]) + text[1..]);

    /// <summary>An email as shadcn's cell shows it (<c>lowercase</c>).</summary>
    public static readonly IValueConverter Lower = new FuncValueConverter<string?, string>(text => text?.ToLowerInvariant() ?? string.Empty);

    /// <summary>An amount as shadcn's cell formats it (US dollars).</summary>
    public static readonly IValueConverter Dollars = new FuncValueConverter<decimal, string>(amount => "$" + amount.ToString("#,0.00", CultureInfo.InvariantCulture));

    public PaymentTable() => Source = Build(Examples, 10);

    /// <summary>shadcn/ui's five example payments.</summary>
    public static IEnumerable<Payment> Examples =>
    [
        new() { Id = "m5gr84i9", Amount = 316, Status = "success", Email = "ken99@example.com" },
        new() { Id = "3u1reuv4", Amount = 242, Status = "success", Email = "Abe45@example.com" },
        new() { Id = "derv1ws0", Amount = 837, Status = "processing", Email = "Monserrat44@example.com" },
        new() { Id = "5kma53ae", Amount = 874, Status = "success", Email = "Silas22@example.com" },
        new() { Id = "bhqecj4p", Amount = 721, Status = "failed", Email = "carmella@example.com" },
    ];

    /// <summary>The table of shadcn's example over <paramref name="payments"/>: the selection, status, email, amount and actions columns, filtered by email.</summary>
    public static DataTableSource<Payment> Build(IEnumerable<Payment> payments, int pageSize)
    {
        var source = new DataTableSource<Payment>(payments) { FilterColumn = "email", PageSize = pageSize };
        return source.Column("select", string.Empty, canSort: false, canHide: false)
            .Column("status", "Status", payment => payment.Status)
            .Column("email", "Email", payment => payment.Email)
            .Column("amount", "Amount", payment => payment.Amount)
            .Column("actions", string.Empty, canSort: false, canHide: false);
    }

    public DataTableSource<Payment> Source { get; }
}

/// <summary>The Data Table page's model: shadcn's five payments and seven more, five a page.</summary>
public sealed class PaymentLedger
{
    public PaymentLedger() => Source = PaymentTable.Build(
        [
            .. PaymentTable.Examples,
            new() { Id = "q1w2e3r4", Amount = 129, Status = "pending", Email = "olivia@example.com" },
            new() { Id = "t5y6u7i8", Amount = 456, Status = "success", Email = "liam@example.com" },
            new() { Id = "o9p0a1s2", Amount = 98, Status = "failed", Email = "noah@example.com" },
            new() { Id = "d3f4g5h6", Amount = 610, Status = "processing", Email = "emma@example.com" },
            new() { Id = "j7k8l9z0", Amount = 275, Status = "success", Email = "ava@example.com" },
            new() { Id = "x1c2v3b4", Amount = 932, Status = "pending", Email = "lucas@example.com" },
            new() { Id = "n5m6q7w8", Amount = 188, Status = "success", Email = "mia@example.com" },
        ],
        5);

    public DataTableSource<Payment> Source { get; }
}

