using Avalonia.Media;

namespace Aioros.Cosmo.Display;

/// <summary>
/// A color in CSS Color 4's OKLCH space (Björn Ottosson's Oklab in polar form), as the browser converts an sRGB color to
/// it and back, for the colors shadcn/ui's utilities derive with relative color syntax and <c>color-mix(in oklch, …)</c>.
/// </summary>
internal readonly record struct Oklch(double L, double C, double H, double Alpha)
{
    public static Oklch From(Color color)
    {
        static double Linear(byte channel)
        {
            var c = channel / 255d;
            return c <= 0.04045 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        double r = Linear(color.R), g = Linear(color.G), b = Linear(color.B);
        var l = Math.Cbrt(0.4122214708 * r + 0.5363325363 * g + 0.0514459929 * b);
        var m = Math.Cbrt(0.2119034982 * r + 0.6806995451 * g + 0.1073969566 * b);
        var s = Math.Cbrt(0.0883024619 * r + 0.2817188376 * g + 0.6299787005 * b);
        var lightness = 0.2104542553 * l + 0.7936177850 * m - 0.0040720468 * s;
        var a = 1.9779984951 * l - 2.4285922050 * m + 0.4505937099 * s;
        var bb = 0.0259040371 * l + 0.7827717662 * m - 0.8086757660 * s;
        var hue = Math.Atan2(bb, a) * 180 / Math.PI;
        return new Oklch(lightness, Math.Sqrt(a * a + bb * bb), hue < 0 ? hue + 360 : hue, color.A / 255d);
    }

    public Color ToColor()
    {
        static byte Gamma(double c)
        {
            var v = c <= 0.0031308 ? 12.92 * c : 1.055 * Math.Pow(c, 1 / 2.4) - 0.055;
            return (byte)Math.Round(Math.Clamp(v, 0, 1) * 255);
        }

        var radians = H * Math.PI / 180;
        double a = C * Math.Cos(radians), b = C * Math.Sin(radians);
        var l = Math.Pow(L + 0.3963377774 * a + 0.2158037573 * b, 3);
        var m = Math.Pow(L - 0.1055613458 * a - 0.0638541728 * b, 3);
        var s = Math.Pow(L - 0.0894841775 * a - 1.2914855480 * b, 3);
        return Color.FromArgb(
            (byte)Math.Round(Math.Clamp(Alpha, 0, 1) * 255),
            Gamma(4.0767416621 * l - 3.3077115913 * m + 0.2309699292 * s),
            Gamma(-1.2684380046 * l + 2.6097574011 * m - 0.3413193965 * s),
            Gamma(-0.0041960863 * l - 0.7034186147 * m + 1.7076147010 * s));
    }

    /// <summary>
    /// CSS Color 5's <c>color-mix(in oklch, this, other p%)</c>: lightness and chroma interpolated premultiplied by alpha, the
    /// hue the shorter way round, <paramref name="share"/> the other color's part.
    /// </summary>
    public Oklch Mix(Oklch other, double share)
    {
        var alpha = Alpha * (1 - share) + other.Alpha * share;
        var delta = other.H - H;
        delta -= 360 * Math.Round(delta / 360);
        var hue = H + delta * share;
        return alpha <= 0
            ? new Oklch(0, 0, 0, 0)
            : new Oklch(
                (L * Alpha * (1 - share) + other.L * other.Alpha * share) / alpha,
                (C * Alpha * (1 - share) + other.C * other.Alpha * share) / alpha,
                hue < 0 ? hue + 360 : hue % 360,
                alpha);
    }
}
