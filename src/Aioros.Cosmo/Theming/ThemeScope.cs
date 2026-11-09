using Avalonia;
using Avalonia.Controls;

namespace Aioros.Cosmo.Theming;

/// <summary>A part of a window drawn in another color theme or radius: its content resolves Cosmo's tokens from the dictionaries <see cref="Color"/> and <see cref="Radius"/> choose, in the light or dark variant it inherits.</summary>
public sealed class ThemeScope : ContentControl
{
    /// <summary>Defines the <see cref="Color"/> property.</summary>
    public static readonly StyledProperty<CosmoColor> ColorProperty = AvaloniaProperty.Register<ThemeScope, CosmoColor>(nameof(Color));

    /// <summary>Defines the <see cref="Radius"/> property.</summary>
    public static readonly StyledProperty<CosmoRadius> RadiusProperty = AvaloniaProperty.Register<ThemeScope, CosmoRadius>(nameof(Radius));

    private readonly ResourceDictionary _tokens = new();

    /// <summary>Creates a scope in the default color theme and radius.</summary>
    public ThemeScope()
    {
        Resources.MergedDictionaries.Add(_tokens);
        CosmoResources.Apply(_tokens, Color, Radius);
    }

    /// <summary>The color theme of the scope's content.</summary>
    public CosmoColor Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>The radius of the scope's content.</summary>
    public CosmoRadius Radius
    {
        get => GetValue(RadiusProperty);
        set => SetValue(RadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        ArgumentNullException.ThrowIfNull(change);
        base.OnPropertyChanged(change);
        if (change.Property == ColorProperty || change.Property == RadiusProperty)
        {
            CosmoResources.Apply(_tokens, Color, Radius);
        }
    }
}
