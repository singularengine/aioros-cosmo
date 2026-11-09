using System.Text.Json;
using Avalonia.Platform;

namespace Aioros.Cosmo.Chat;

/// <summary>A token of highlighted code: its text and the key of the code brush it is drawn with (null for the code's own text).</summary>
/// <param name="Text">The token's text.</param>
/// <param name="Brush">The resource key of one of Cosmo's code brushes, or null.</param>
public readonly record struct CodeToken(string Text, string? Brush);

/// <summary>
/// Highlights code for the fenced blocks of a <c>c:Markdown</c> (Aioros.Cosmo.Markdown), as shadcn/ui's markdown highlights them with shiki; Cosmo
/// looks one up under the <c>CosmoCodeHighlighter</c> resource key and leaves the code plain without one.
/// </summary>
public interface ICodeHighlighter
{
    /// <summary>The tokens of each line of the code, or null for code it cannot highlight.</summary>
    /// <param name="code">The code.</param>
    /// <param name="language">Its fence's language.</param>
    /// <returns>One list of tokens per line, or null.</returns>
    IReadOnlyList<IReadOnlyList<CodeToken>>? Highlight(string code, string language);
}

/// <summary>
/// The code brushes of shiki's github-light and github-dark colors, so tokens shiki wrote when the app was built are drawn
/// in Cosmo's themes: <see cref="Key"/> maps a token's light and dark colors to the brush that holds them.
/// </summary>
public static class CodeBrushes
{
    private static readonly Dictionary<(string Light, string Dark), string> Keys = Build();

    /// <summary>The key of the code brush drawn in a token's light and dark colors, or null for colors Cosmo has no brush for.</summary>
    /// <param name="light">The token's github-light color, <c>#rrggbb</c> or <c>#rrggbbaa</c>.</param>
    /// <param name="dark">The token's github-dark color.</param>
    /// <returns>The brush's resource key, or null.</returns>
    public static string? Key(string light, string dark) =>
        Keys.TryGetValue((light.ToUpperInvariant(), dark.ToUpperInvariant()), out var key) ? key : null;

    private static Dictionary<(string Light, string Dark), string> Build()
    {
        var keys = new Dictionary<(string, string), string>
        {
            [("#24292E", "#E1E4E8")] = "CosmoCodeTextBrush",
            [("#D73A49", "#F97583")] = "CosmoCodeKeywordBrush",
            [("#032F62", "#9ECBFF")] = "CosmoCodeStringBrush",
            [("#6F42C1", "#B392F0")] = "CosmoCodeFunctionBrush",
            [("#005CC5", "#79B8FF")] = "CosmoCodeConstantBrush",
            [("#E36209", "#FFAB70")] = "CosmoCodeVariableBrush",
            [("#6A737D", "#6A737D")] = "CosmoCodeCommentBrush",
            [("#22863A", "#85E89D")] = "CosmoCodeTagBrush",
            [("#24292E80", "#E1E4E880")] = "CosmoCodeTextDimBrush",
        };
        (string Name, string Light, string Dark)[] terminal =
        [
            ("Black", "#24292E", "#586069"), ("Blue", "#0366D6", "#2188FF"), ("BrightBlack", "#959DA5", "#959DA5"), ("BrightBlue", "#005CC5", "#79B8FF"),
            ("BrightCyan", "#3192AA", "#56D4DD"), ("BrightGreen", "#22863A", "#85E89D"), ("BrightMagenta", "#5A32A3", "#B392F0"), ("BrightRed", "#CB2431", "#F97583"),
            ("BrightWhite", "#D1D5DA", "#FAFBFC"), ("BrightYellow", "#B08800", "#FFEA7F"), ("Cyan", "#1B7C83", "#39C5CF"), ("Green", "#28A745", "#34D058"),
            ("Magenta", "#5A32A3", "#B392F0"), ("Red", "#D73A49", "#EA4A5A"), ("White", "#6A737D", "#D1D5DA"), ("Yellow", "#DBAB09", "#FFEA7F"),
        ];
        foreach (var (name, light, dark) in terminal)
        {
            keys.TryAdd((light, dark), $"CosmoCodeAnsi{name}Brush");
        }

        return keys;
    }
}

/// <summary>
/// A <see cref="ICodeHighlighter"/> reading the tokens shiki wrote when the app was built (Cosmo's <c>tools/shadcn/highlight.mjs</c>,
/// the radix base's highlighter: github-light and github-dark), as Cosmo highlights its code blocks when their XAML is
/// written: a JSON document of <c>{name: {source: {code, language}, lines: [[[text, light, dark], …], …]}}</c> at
/// <see cref="Source"/>. A code that begins one it holds in the same language (a fence still streaming in) takes that
/// code's tokens cut where it ends, as shiki highlights the text received so far; any other code is left plain.
/// </summary>
public sealed class ShikiTokens : ICodeHighlighter
{
    private Dictionary<(string Code, string Language), IReadOnlyList<IReadOnlyList<CodeToken>>>? _tokens;

    /// <summary>The address of the tokens' JSON document, an <c>avares://</c> asset.</summary>
    public Uri? Source { get; set; }

    /// <inheritdoc />
    public IReadOnlyList<IReadOnlyList<CodeToken>>? Highlight(string code, string language)
    {
        var tokens = _tokens ??= Load();
        if (tokens.TryGetValue((code, language), out var lines))
        {
            return lines;
        }

        return code.Length == 0 ? null : tokens.FirstOrDefault(entry => entry.Key.Language == language && entry.Key.Code.StartsWith(code, StringComparison.Ordinal)).Value is { } whole ? Cut(whole, code) : null;
    }

    private static List<IReadOnlyList<CodeToken>> Cut(IReadOnlyList<IReadOnlyList<CodeToken>> lines, string code)
    {
        var cut = new List<IReadOnlyList<CodeToken>>();
        var rows = code.Split('\n');
        for (var index = 0; index < rows.Length && index < lines.Count; index++)
        {
            var left = rows[index].Length;
            var line = new List<CodeToken>();
            foreach (var token in lines[index])
            {
                if (left <= 0)
                {
                    break;
                }

                line.Add(token.Text.Length <= left ? token : token with { Text = token.Text[..left] });
                left -= token.Text.Length;
            }

            cut.Add(line);
        }

        return cut;
    }

    private Dictionary<(string, string), IReadOnlyList<IReadOnlyList<CodeToken>>> Load()
    {
        var tokens = new Dictionary<(string, string), IReadOnlyList<IReadOnlyList<CodeToken>>>();
        if (Source is null)
        {
            return tokens;
        }

        using var stream = AssetLoader.Open(Source);
        using var document = JsonDocument.Parse(stream);
        foreach (var entry in document.RootElement.EnumerateObject().Select(static entry => entry.Value).Where(static entry => entry.GetProperty("lines").ValueKind == JsonValueKind.Array))
        {
            var source = entry.GetProperty("source");
            tokens[(source.GetProperty("code").GetString() ?? string.Empty, source.GetProperty("language").GetString() ?? string.Empty)] =
                [.. entry.GetProperty("lines").EnumerateArray().Select(line => (IReadOnlyList<CodeToken>)[.. line.EnumerateArray().Select(Token)])];
        }

        return tokens;
    }

    private static CodeToken Token(JsonElement token) =>
        new(token[0].GetString() ?? string.Empty, CodeBrushes.Key(token[1].GetString() ?? string.Empty, token[2].GetString() ?? string.Empty));
}
