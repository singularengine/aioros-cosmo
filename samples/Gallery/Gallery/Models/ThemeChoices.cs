using Aioros.Cosmo.Theming;

namespace Gallery.Models;

/// <summary>One choice of the theme customizer: its label, the link that picks it, its swatch and whether it is the current one.</summary>
public sealed record ThemeOption(string Label, string Href, string Swatch, bool IsCurrent);

/// <summary>The color theme and the radius a visitor asked for, as the query names them.</summary>
public sealed record ThemeSelection(string? Color, string? Radius);

/// <summary>The theme customizer's state: the color theme and the radius in the query, the links to the others, and the XAML that applies them.</summary>
public sealed class ThemeChoices
{
    /// <summary>The choices for the selection's color and radius (case-insensitive; the defaults otherwise).</summary>
    public ThemeChoices(ThemeSelection selection)
    {
        ArgumentNullException.ThrowIfNull(selection);
        Color = Enum.TryParse<CosmoColor>(selection.Color, ignoreCase: true, out var c) ? c : CosmoColor.Neutral;
        Radius = Enum.TryParse<CosmoRadius>(selection.Radius, ignoreCase: true, out var r) ? r : CosmoRadius.Default;
        Colors = [.. Swatches.Select(s => new ThemeOption(s.Color.ToString(), Href(s.Color, Radius), s.Swatch, s.Color == Color))];
        Radii = [.. Rems.Select(s => new ThemeOption(s.Label, Href(Color, s.Radius), "#00000000", s.Radius == Radius))];
    }

    public CosmoColor Color { get; }

    public CosmoRadius Radius { get; }

    public IReadOnlyList<ThemeOption> Colors { get; }

    public IReadOnlyList<ThemeOption> Radii { get; }

    private static readonly (CosmoColor Color, string Swatch)[] Swatches =
    [
        (CosmoColor.Neutral, "#171717"), (CosmoColor.Zinc, "#18181B"), (CosmoColor.Slate, "#0F172B"), (CosmoColor.Stone, "#1C1917"), (CosmoColor.Gray, "#101828"),
        (CosmoColor.Red, "#E7000B"), (CosmoColor.Rose, "#EC003F"), (CosmoColor.Orange, "#F54900"), (CosmoColor.Green, "#5EA500"),
        (CosmoColor.Blue, "#1447E6"), (CosmoColor.Yellow, "#FDC700"), (CosmoColor.Violet, "#7F22FE"),
    ];

    private static readonly (CosmoRadius Radius, string Label)[] Rems =
    [
        (CosmoRadius.None, "0"), (CosmoRadius.Small, "0.3"), (CosmoRadius.Medium, "0.5"), (CosmoRadius.Default, "0.625"), (CosmoRadius.Large, "0.75"), (CosmoRadius.ExtraLarge, "1.0"),
    ];

    /// <summary>The XAML that applies the chosen theme to a whole application.</summary>
    public string Code => $"<Application.Styles>\n  <FluentTheme />\n  <c:CosmoTheme Color=\"{Color}\" Radius=\"{Radius}\" />\n</Application.Styles>";

    /// <summary>The XAML that applies the chosen theme to a part of a window.</summary>
    public string ScopeCode => $"<c:ThemeScope Color=\"{Color}\" Radius=\"{Radius}\">\n  …\n</c:ThemeScope>";

    private static string Href(CosmoColor color, CosmoRadius radius) =>
        $"/themes?color={color.ToString().ToLowerInvariant()}&radius={radius.ToString().ToLowerInvariant()}";
}
