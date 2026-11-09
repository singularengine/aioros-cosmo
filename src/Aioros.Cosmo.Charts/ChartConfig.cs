using Aioros.Pictor;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;

namespace Aioros.Cosmo.Charts;

/// <summary>An entry of a <see cref="ChartConfig"/>: a data key's label and color (shadcn/ui's <c>{ desktop: { label, color } }</c>).</summary>
public sealed class ChartConfigItem
{
    /// <summary>The data key the entry names: the series whose <c>DataKey</c> it is take its label and color.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>The series' name in the tooltip and the legend (shadcn/ui's <c>label</c>); a series' own <c>Name</c> wins.</summary>
    public string? Label { get; set; }

    /// <summary>
    /// The series' color (shadcn/ui's <c>color</c>, the <c>--color-{key}</c> its series read): a resource key of the theme
    /// (<c>PictorPalette1</c>, as <c>var(--chart-1)</c> names a token), followed as the theme changes, or a CSS color
    /// written <c>#rrggbb</c> or as a function (<c>rgb(…)</c>); a series' own fill or stroke wins.
    /// </summary>
    public string? Color { get; set; }
}

/// <summary>
/// shadcn/ui's <c>ChartConfig</c>: each data key's label and color, declared once as a resource and given to a chart
/// with <see cref="ConfigProperty"/> (shadcn/ui's <c>ChartContainer config</c>). Each series of the chart whose
/// <c>DataKey</c> has an entry takes the entry's label as its <c>Name</c> and its color as its fill (a bar's, an area's, a
/// radar's, a radial bar's) or stroke (a line's; an area's too), where it sets none of its own. A pie's slices are named
/// and colored by its <c>NameKey</c> and <c>Cell</c>s, as shadcn/ui's pies read their rows' <c>fill</c>.
/// </summary>
public sealed class ChartConfig : AvaloniaList<ChartConfigItem>
{
    /// <summary>Defines the <c>Config</c> attached property.</summary>
    public static readonly AttachedProperty<ChartConfig?> ConfigProperty = AvaloniaProperty.RegisterAttached<ChartConfig, Chart, ChartConfig?>("Config");

    static ChartConfig() => ConfigProperty.Changed.AddClassHandler<Chart>(static (chart, _) => Attach(chart));

    /// <summary>The config a chart's series read their labels and colors from.</summary>
    /// <param name="chart">The chart.</param>
    /// <returns>The config, or null.</returns>
    public static ChartConfig? GetConfig(Chart chart)
    {
        ArgumentNullException.ThrowIfNull(chart);
        return chart.GetValue(ConfigProperty);
    }

    /// <summary>Gives a chart's series a config's labels and colors.</summary>
    /// <param name="chart">The chart.</param>
    /// <param name="value">The config.</param>
    public static void SetConfig(Chart chart, ChartConfig? value)
    {
        ArgumentNullException.ThrowIfNull(chart);
        chart.SetValue(ConfigProperty, value);
    }

    private static void Attach(Chart chart)
    {
        chart.Elements.CollectionChanged -= Changed;
        chart.Elements.CollectionChanged += Changed;
        Apply(chart);

        void Changed(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) => Apply(chart);
    }

    private static void Apply(Chart chart)
    {
        if (GetConfig(chart) is not { } config)
        {
            return;
        }

        foreach (var element in chart.Elements.OfType<StyledElement>().Where(static element => element is Series or Radar or RadialBar))
        {
            if (AvaloniaPropertyRegistry.Instance.FindRegistered(element, "DataKey") is { } dataKey && config.FirstOrDefault(item => item.Key == element.GetValue(dataKey) as string) is { } item)
            {
                Apply(element, item);
            }
        }
    }

    /// <summary>A series' label and color from its entry, where it sets none of its own.</summary>
    private static void Apply(StyledElement element, ChartConfigItem item)
    {
        var registry = AvaloniaPropertyRegistry.Instance;
        if (item.Label is { } label && registry.FindRegistered(element, "Name") is { } name && !element.IsSet(name))
        {
            element.SetValue(name, label);
        }

        foreach (var paint in item.Color is null ? [] : Paints(element))
        {
            if (registry.FindRegistered(element, paint) is { } property && !element.IsSet(property))
            {
                Paint(element, property, item.Color!);
            }
        }
    }

    /// <summary>What a series paints with its color: a line its stroke, an area both, any other its fill.</summary>
    private static string[] Paints(AvaloniaObject element) => element switch
    {
        Line => ["Stroke"],
        Area => ["Fill", "Stroke"],
        _ => ["Fill"],
    };

    /// <summary>A color: a resource key followed through the theme, or a CSS color set as it is.</summary>
    private static void Paint(StyledElement element, AvaloniaProperty property, string color)
    {
        if (color.StartsWith('#') || color.Contains('(', StringComparison.Ordinal))
        {
            element.SetValue(property, color);
            return;
        }

        element.Bind(property, element.GetResourceObservable(color));
    }
}
