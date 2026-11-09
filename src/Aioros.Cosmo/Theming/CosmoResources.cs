using Avalonia.Controls;

namespace Aioros.Cosmo.Theming;

/// <summary>The token dictionaries a color theme and a radius choose: what <see cref="CosmoTheme"/> and <see cref="ThemeScope"/> merge.</summary>
public static class CosmoResources
{
    /// <summary>A new dictionary holding a color theme's light and dark tokens.</summary>
    public static ResourceDictionary Colors(CosmoColor color) => color switch
    {
        CosmoColor.Zinc => new ZincTokens(),
        CosmoColor.Slate => new SlateTokens(),
        CosmoColor.Stone => new StoneTokens(),
        CosmoColor.Gray => new GrayTokens(),
        CosmoColor.Red => new RedTokens(),
        CosmoColor.Rose => new RoseTokens(),
        CosmoColor.Orange => new OrangeTokens(),
        CosmoColor.Green => new GreenTokens(),
        CosmoColor.Blue => new BlueTokens(),
        CosmoColor.Yellow => new YellowTokens(),
        CosmoColor.Violet => new VioletTokens(),
        _ => new NeutralTokens(),
    };

    /// <summary>A new dictionary holding a radius choice's scale.</summary>
    public static ResourceDictionary Radius(CosmoRadius radius) => radius switch
    {
        CosmoRadius.None => new RadiusNoneTokens(),
        CosmoRadius.Small => new RadiusSmallTokens(),
        CosmoRadius.Medium => new RadiusMediumTokens(),
        CosmoRadius.Large => new RadiusLargeTokens(),
        CosmoRadius.ExtraLarge => new RadiusExtraLargeTokens(),
        _ => new RadiusDefaultTokens(),
    };

    /// <summary>Fills <paramref name="tokens"/> with the color theme's and the radius's dictionaries, replacing what it held.</summary>
    public static void Apply(IResourceDictionary tokens, CosmoColor color, CosmoRadius radius)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        tokens.MergedDictionaries.Clear();
        tokens.MergedDictionaries.Add(Colors(color));
        tokens.MergedDictionaries.Add(Radius(radius));
    }
}
