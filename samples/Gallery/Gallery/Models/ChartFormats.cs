using System.Globalization;
using Avalonia.Data.Converters;

namespace Gallery.Models;

/// <summary>A month's first three letters, as shadcn/ui's examples write their ticks (<c>value.slice(0, 3)</c>); a category tick receives its text.</summary>
public sealed class MonthName : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var month) ? month.ToString("MMMM", CultureInfo.InvariantCulture)[..3] : value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>A tooltip line's value with its unit, as a <c>formatter</c> writes it.</summary>
public sealed class Visitors : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is double count ? count.ToString("N0", CultureInfo.InvariantCulture) + " visitors" : value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}

/// <summary>A tooltip's label as a <c>labelFormatter</c> writes it: the month and the year.</summary>
public sealed class MonthTitle : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is string text && DateTime.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out var month) ? month.ToString("MMMM yyyy", CultureInfo.InvariantCulture) : value;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotSupportedException();
}
