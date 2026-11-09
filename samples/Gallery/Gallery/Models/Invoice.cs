namespace Gallery.Models;

/// <summary>An invoice of shadcn/ui's table example: its number, payment status and method, and amount.</summary>
public sealed class Invoice
{
    public string Number { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;

    public string Amount { get; set; } = string.Empty;
}
