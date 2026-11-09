using Avalonia.Markup.Xaml;
using Avalonia.Styling;

namespace Aioros.Cosmo.Charts;

/// <summary>
/// shadcn/ui's <c>ChartContainer</c> for Aioros.Pictor's charts, added after <c>c:CosmoTheme</c>: every chart writes its
/// texts in Cosmo's font at 12 px (<c>text-xs</c>); its colors come from the Pictor keys each of Cosmo's color themes
/// defines (the palette from <c>--chart-1</c> to <c>--chart-5</c>, the grid at <c>border/50</c>, muted ticks and cursors).
/// </summary>
public sealed class CosmoChartsTheme : Styles
{
    /// <summary>Loads the theme's compiled XAML.</summary>
    public CosmoChartsTheme() => AvaloniaXamlLoader.Load(this);
}
