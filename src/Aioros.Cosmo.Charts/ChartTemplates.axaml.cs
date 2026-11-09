using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Charts;

/// <summary>The templates the chart contents build from: the tooltip's box per indicator and the legend's entry.</summary>
public sealed class ChartTemplates : ResourceDictionary
{
    /// <summary>Loads the dictionary's compiled XAML.</summary>
    public ChartTemplates() => AvaloniaXamlLoader.Load(this);

    /// <summary>The one dictionary every content builds from.</summary>
    internal static ChartTemplates Shared { get; } = new();

    /// <summary>The template under a key.</summary>
    internal static IDataTemplate Template(string key) => (IDataTemplate)Shared[key]!;
}
