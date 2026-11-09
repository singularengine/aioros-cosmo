using System.Globalization;
using System.Text.RegularExpressions;
using Avalonia.Data.Converters;

namespace Aioros.Cosmo.Dates;

/// <summary>
/// A date as date-fns's <c>format(date, "PPP")</c> writes it for shadcn/ui's date picker: the long date of the culture
/// without its weekday, with the day's English ordinal in English (<c>June 12th, 2025</c>), and read back from that text,
/// so it is a <see cref="Avalonia.Controls.CalendarDatePicker.TextConverter"/>.
/// </summary>
public sealed partial class DateText : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is DateTime date ? Format(date, culture) : null;

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not string text || string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var plain = Ordinal().Replace(text.Trim(), "$1");
        return DateTime.TryParseExact(plain, Pattern(culture), culture, DateTimeStyles.AllowWhiteSpaces, out var exact) ? exact
            : DateTime.TryParse(plain, culture, DateTimeStyles.AllowWhiteSpaces, out var date) ? date
            : null;
    }

    /// <summary>The date in date-fns's <c>PPP</c> for the culture.</summary>
    public static string Format(DateTime date, CultureInfo culture) =>
        English(culture)
            ? string.Create(culture, $"{date.ToString("MMMM", culture)} {date.Day}{Suffix(date.Day)}, {date.Year}")
            : date.ToString(Pattern(culture), culture);

    private static string Pattern(CultureInfo culture) =>
        English(culture) ? "MMMM d, yyyy" : Weekday().Replace(culture.DateTimeFormat.LongDatePattern, string.Empty).Trim(' ', ',');

    /// <summary>English, or the invariant culture, whose names are English, as date-fns's default locale is <c>en-US</c>.</summary>
    private static bool English(CultureInfo culture) => culture.TwoLetterISOLanguageName is "en" or "iv";

    private static string Suffix(int day) => (day % 100) is 11 or 12 or 13 ? "th" : (day % 10) switch
    {
        1 => "st",
        2 => "nd",
        3 => "rd",
        _ => "th",
    };

    [GeneratedRegex(@"(\d+)(st|nd|rd|th)\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex Ordinal();

    [GeneratedRegex(@"dddd,?\s*|,?\s*dddd", RegexOptions.CultureInvariant)]
    private static partial Regex Weekday();
}
