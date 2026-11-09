using System.Globalization;

namespace Aioros.Cosmo.Testing;

/// <summary>One shadow of a CSS <c>box-shadow</c> list: offsets, blur and spread in px and its color.</summary>
internal readonly record struct CssShadow(double X, double Y, double Blur, double Spread, CssColor Color, bool Inset)
{
    /// <summary>The visible shadows of a computed <c>box-shadow</c> (Tailwind's transparent placeholders dropped); none for <c>none</c>.</summary>
    public static IReadOnlyList<CssShadow> List(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Trim() == "none")
        {
            return [];
        }

        var parts = new List<string>();
        var depth = 0;
        var start = 0;
        for (var i = 0; i < text.Length; i++)
        {
            depth += text[i] switch { '(' => 1, ')' => -1, _ => 0 };
            if (text[i] == ',' && depth == 0)
            {
                parts.Add(text[start..i]);
                start = i + 1;
            }
        }

        parts.Add(text[start..]);
        return [.. parts.Select(One).Where(shadow => shadow.Color.A > 0.002)];
    }

    /// <summary>Whether two shadow lists are the same: lengths within 0.01 px and colors within 1/255.</summary>
    public static bool Same(IReadOnlyList<CssShadow> a, IReadOnlyList<CssShadow> b) =>
        a.Count == b.Count && a.Zip(b).All(pair => pair.First.Inset == pair.Second.Inset && pair.First.Color.Near(pair.Second.Color)
            && Math.Abs(pair.First.X - pair.Second.X) < 0.01 && Math.Abs(pair.First.Y - pair.Second.Y) < 0.01
            && Math.Abs(pair.First.Blur - pair.Second.Blur) < 0.01 && Math.Abs(pair.First.Spread - pair.Second.Spread) < 0.01);

    private static CssShadow One(string text)
    {
        var value = text.Trim();
        var inset = value.Contains("inset", StringComparison.Ordinal);
        value = value.Replace("inset", string.Empty, StringComparison.Ordinal).Trim();
        var close = value.IndexOf(')', StringComparison.Ordinal);
        string color;
        string rest;
        if (close > 0 && (value.StartsWith("rgb", StringComparison.Ordinal) || value.StartsWith("ok", StringComparison.Ordinal)))
        {
            color = value[..(close + 1)];
            rest = value[(close + 1)..];
        }
        else
        {
            var tokens = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            color = tokens.First(token => token.StartsWith('#'));
            rest = string.Join(' ', tokens.Where(token => !token.StartsWith('#')));
        }

        var lengths = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(token => double.Parse(token.Replace("px", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture)).ToList();
        while (lengths.Count < 4)
        {
            lengths.Add(0);
        }

        return new(lengths[0], lengths[1], lengths[2], lengths[3], CssColor.Parse(color), inset);
    }

    public override string ToString() => string.Create(CultureInfo.InvariantCulture, $"{(Inset ? "inset " : "")}{X} {Y} {Blur} {Spread} {Color}");
}
