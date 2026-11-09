using System.Text.Json;

namespace Aioros.Cosmo.Testing;

/// <summary>One element of a fixture as Chromium computed it: its box relative to the fixture and the styles the fidelity checks compare.</summary>
internal sealed record ElementMetrics(string Tag, string Text, double X, double Y, double Width, double Height, string Background, string Color, string BorderColor, string BorderWidth, string Radius, string Shadow, string FontSize, string FontWeight, string Opacity, string Decoration, string? PaddingLeft = null, string? PaddingTop = null, string? Slot = null, string? LineHeight = null, string? PaddingBottom = null, string? Outline = null);

/// <summary>shadcn/ui's own render of each fixture (<c>tools/shadcn/out/metrics.json</c>, written by <c>tools/shadcn/capture.mjs</c>).</summary>
internal static class ReferenceMetrics
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    /// <summary>The elements of a fixture in a theme, in a state (<c>normal</c>, <c>hover</c> or <c>focus</c>).</summary>
    public static async Task<IReadOnlyList<ElementMetrics>> OfAsync(string fixture, string theme, string state = "normal")
    {
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Repository.Path("tools", "shadcn", "out", "metrics.json")));
        return document.RootElement.GetProperty($"{fixture}.{theme}").GetProperty(state).Deserialize<List<ElementMetrics>>(Options) ?? [];
    }

    /// <summary>The elements of a fixture whose <c>data-slot</c> is <paramref name="slot"/>, in document order.</summary>
    public static async Task<IReadOnlyList<ElementMetrics>> SlotsAsync(string fixture, string theme, string slot, string state = "normal") =>
        [.. (await OfAsync(fixture, theme, state)).Where(element => element.Slot == slot)];

    /// <summary>The CSS value of every token of a color theme as Chromium resolved it, with the RGBA its 2D canvas painted for it.</summary>
    public static async Task<IReadOnlyDictionary<string, (string Css, int[] Rgba)>> TokensAsync(string color, string theme)
    {
        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(Repository.Path("tools", "shadcn", "out", "metrics.json")));
        return document.RootElement.GetProperty($"tokens.{color}.{theme}").EnumerateObject()
            .ToDictionary(token => token.Name, token => (token.Value.GetProperty("css").GetString()!, token.Value.GetProperty("rgba").EnumerateArray().Select(v => v.GetInt32()).ToArray()), StringComparer.Ordinal);
    }

    /// <summary>The oklch values of every token of every color theme as the pinned registry states them (<c>tests/Aioros.Cosmo.Tests/Reference/themes.json</c>).</summary>
    public static async Task<IReadOnlyDictionary<string, IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>>> ThemesAsync()
    {
        var path = Repository.Path("tests", "Aioros.Cosmo.Tests", "Reference", "themes.json");
        return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<string, string>>>>(await File.ReadAllTextAsync(path))!
            .ToDictionary(theme => theme.Key, theme => (IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>>)theme.Value.ToDictionary(mode => mode.Key, mode => (IReadOnlyDictionary<string, string>)mode.Value, StringComparer.Ordinal), StringComparer.Ordinal);
    }
}
