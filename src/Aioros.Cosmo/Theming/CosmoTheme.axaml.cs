using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Aioros.Cosmo.Theming;

/// <summary>Cosmo's design tokens and control themes: include it once in the application's styles, after <c>FluentTheme</c>.</summary>
public sealed class CosmoTheme : Styles
{
    private readonly ResourceDictionary _tokens = new();

    private CosmoColor _color;

    private CosmoRadius _radius;

    /// <summary>Loads the tokens of the default color theme and radius, and the control themes.</summary>
    public CosmoTheme()
    {
        Forms.RadioKeyboard.Register();
        Overlays.OverlayDefaults.Register();
        Navigation.TabsKeyboard.Register();
        Display.CarouselControls.Register();
        Display.TextBalance.Register();
        Display.Shimmer.Register();
        Display.ScrollFade.Register();
        AvaloniaXamlLoader.Load(this);
        Resources.MergedDictionaries.Insert(0, _tokens);
        CosmoResources.Apply(_tokens, _color, _radius);
    }

    /// <summary>The application's color theme (<see cref="CosmoColor.Neutral"/> by default).</summary>
    public CosmoColor Color
    {
        get => _color;
        set
        {
            _color = value;
            CosmoResources.Apply(_tokens, _color, _radius);
        }
    }

    /// <summary>The application's radius (<see cref="CosmoRadius.Default"/>, 0.625rem, by default).</summary>
    public CosmoRadius Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            CosmoResources.Apply(_tokens, _color, _radius);
        }
    }
}
