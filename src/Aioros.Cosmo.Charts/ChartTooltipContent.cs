using Aioros.Pictor;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Charts;

/// <summary>How a tooltip line marks its series (shadcn/ui's <c>indicator</c>).</summary>
public enum ChartIndicator
{
    /// <summary>A 10 px square of the series' color.</summary>
    Dot,

    /// <summary>A 4 px bar of the series' color along the line.</summary>
    Line,

    /// <summary>A dashed 1.5 px outline of the series' color along the line, no wider than its borders (<c>w-0 border-dashed</c>).</summary>
    Dashed,
}

/// <summary>
/// shadcn/ui's <c>ChartTooltipContent</c> as a Pictor tooltip's <c>ContentTemplate</c>: a box on <c>--background</c> with a
/// <c>border/50</c> border, 8 px corners and <c>shadow-xl</c>, the label above a row per line of the payload (Pictor's
/// <c>TooltipItems</c>, as shadcn/ui maps <c>payload</c>; or, for a single line marked by a line or dashes, the label
/// nested beside it), each row its indicator, its series' name muted and its value in the mono font. It brings its rows'
/// template (<see cref="ItemTemplate"/>), so a tooltip needs no other.
/// </summary>
public sealed class ChartTooltipContent : IDataTemplate, ITooltipLinesTemplate
{
    /// <summary>How each line marks its series.</summary>
    public ChartIndicator Indicator { get; set; }

    /// <summary>Whether the label is left out (shadcn/ui's <c>hideLabel</c>).</summary>
    public bool HideLabel { get; set; }

    /// <summary>Whether the lines have no indicator (shadcn/ui's <c>hideIndicator</c>).</summary>
    public bool HideIndicator { get; set; }

    /// <summary>The template of each row: its indicator, name and value.</summary>
    public IDataTemplate ItemTemplate => ChartTemplates.Template(Key + "Line");

    private string Key => HideIndicator ? "None" : Indicator.ToString();

    /// <inheritdoc />
    public Control? Build(object? param)
    {
        var key = Key;
        if (ChartTemplates.Template(key).Build(param) is not { } box)
        {
            return null;
        }

        if (HideLabel)
        {
            foreach (var label in box.GetLogicalDescendants().OfType<TextBlock>().Where(static text => text.Name is "PART_Label" or "PART_NestedLabel"))
            {
                label.IsVisible = false;
            }
        }

        return box;
    }

    /// <inheritdoc />
    public bool Match(object? data) => data is TooltipContentContext;
}

/// <summary>
/// shadcn/ui's <c>ChartLegendContent</c> as a Pictor legend's <c>Template</c>: the entries in a wrapping row centered
/// below a 12 px padding (<c>flex items-center justify-center gap-4 pt-3</c>), each drawn by the legend's
/// <c>ContentTemplate</c>, <see cref="ChartLegendItem"/>.
/// </summary>
public sealed class ChartLegendContent : IDataTemplate
{
    /// <summary>The gap between the entries and between their lines (shadcn/ui's <c>gap-4</c>, 16 px).</summary>
    public double Spacing { get; set; } = 16;

    /// <summary>The share of the row each entry takes, its content centered in it (<c>*:basis-1/4</c> is 0.25); unset, an entry is as wide as its content.</summary>
    public double ItemBasis { get; set; } = double.NaN;

    /// <inheritdoc />
    public Control? Build(object? param) => new Border
    {
        Padding = new Thickness(0, 12, 0, 0),
        Child = new LegendItems { Orientation = Orientation.Horizontal, Spacing = Spacing, ItemBasis = ItemBasis },
    };

    /// <inheritdoc />
    public bool Match(object? data) => data is LegendContext;
}

/// <summary>An entry of shadcn/ui's <c>ChartLegendContent</c> as a Pictor legend's <c>ContentTemplate</c>: an 8 px square of its series' color beside its name, 6 px apart.</summary>
public sealed class ChartLegendItem : IDataTemplate
{
    /// <inheritdoc />
    public Control? Build(object? param) => ChartTemplates.Template("Legend").Build(param);

    /// <inheritdoc />
    public bool Match(object? data) => data is LegendEntryContext;
}
