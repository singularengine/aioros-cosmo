using System.Globalization;
using System.Text.RegularExpressions;

namespace Aioros.Cosmo.Testing;

/// <summary>
/// A color as CSS states it, in sRGB with 8-bit channels and an alpha in 0..1: <c>oklch()</c> and <c>oklab()</c> are
/// converted with CSS Color 4's OKLab matrices and sRGB transfer function, clipped per channel as Chromium does on an
/// sRGB display; <c>rgb()</c>, <c>rgba()</c>, <c>#rrggbb</c>, <c>#aarrggbb</c> (Avalonia's order) and <c>transparent</c> are read as they are.
/// </summary>
internal readonly partial record struct CssColor(int R, int G, int B, double A)
{
    public static CssColor Transparent { get; } = new(0, 0, 0, 0);

    public static CssColor Parse(string text)
    {
        var value = text.Trim();
        if (value == "transparent")
        {
            return Transparent;
        }

        if (value.StartsWith('#'))
        {
            var hex = value[1..];
            var argb = uint.Parse(hex.Length == 6 ? "FF" + hex : hex, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            return new((int)((argb >> 16) & 0xFF), (int)((argb >> 8) & 0xFF), (int)(argb & 0xFF), ((argb >> 24) & 0xFF) / 255.0);
        }

        var match = Function().Match(value);
        if (!match.Success)
        {
            throw new FormatException($"Not a CSS color: {text}");
        }

        var parts = match.Groups[2].Value.Replace(",", " ", StringComparison.Ordinal).Replace("/", " / ", StringComparison.Ordinal)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var slash = Array.IndexOf(parts, "/");
        var channels = slash < 0 ? parts : parts[..slash];
        var alpha = slash >= 0 ? Number(parts[slash + 1], 1) : channels.Length == 4 ? Number(channels[3], 1) : 1;
        return match.Groups[1].Value switch
        {
            "oklch" => FromOklab(Number(channels[0], 1), Number(channels[1], 0.4) * Math.Cos(Number(channels[2], 1) * Math.PI / 180), Number(channels[1], 0.4) * Math.Sin(Number(channels[2], 1) * Math.PI / 180), alpha),
            "oklab" => FromOklab(Number(channels[0], 1), Number(channels[1], 0.4), Number(channels[2], 0.4), alpha),
            _ => new((int)Math.Round(Number(channels[0], 255)), (int)Math.Round(Number(channels[1], 255)), (int)Math.Round(Number(channels[2], 255)), alpha),
        };
    }

    public static CssColor FromOklab(double l, double a, double b, double alpha)
    {
        var l1 = Math.Pow(l + 0.3963377773761749 * a + 0.2158037573099136 * b, 3);
        var m1 = Math.Pow(l - 0.1055613458156586 * a - 0.0638541728258133 * b, 3);
        var s1 = Math.Pow(l - 0.0894841775298119 * a - 1.2914855480194092 * b, 3);
        return new(
            Channel(4.0767416621 * l1 - 3.3077115913 * m1 + 0.2309699292 * s1),
            Channel(-1.2684380046 * l1 + 2.6097574011 * m1 - 0.3413193965 * s1),
            Channel(-0.0041960863 * l1 - 0.7034186147 * m1 + 1.7076147010 * s1),
            alpha);
    }

    /// <summary>Whether two colors are the same within <paramref name="channels"/>/255 per channel and 1/255 of alpha; two fully transparent colors are the same.</summary>
    public bool Near(CssColor other, int channels = 1) =>
        (A < 0.002 && other.A < 0.002)
        || (Math.Abs(R - other.R) <= channels && Math.Abs(G - other.G) <= channels && Math.Abs(B - other.B) <= channels && Math.Abs(A - other.A) <= 1.5 / 255);

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"rgba({R},{G},{B},{A:0.###})");

    private static int Channel(double linear)
    {
        var encoded = linear <= 0.0031308 ? 12.92 * linear : 1.055 * Math.Pow(linear, 1 / 2.4) - 0.055;
        return (int)Math.Round(Math.Clamp(encoded, 0, 1) * 255);
    }

    private static double Number(string text, double percentOf) =>
        text.EndsWith('%') ? double.Parse(text[..^1], CultureInfo.InvariantCulture) / 100 * percentOf : double.Parse(text, CultureInfo.InvariantCulture);

    [GeneratedRegex(@"^(oklch|oklab|rgba?)\((.*)\)$")]
    private static partial Regex Function();
}
