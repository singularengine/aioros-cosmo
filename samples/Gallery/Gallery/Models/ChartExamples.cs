namespace Gallery.Models;

/// <summary>The Chart page's model: the data of shadcn/ui's chart examples and the interactive examples' choices.</summary>
public sealed class ChartExamples
{
    /// <summary>The examples' data.</summary>
    public ChartSamples Samples { get; } = new();

    /// <summary>The interactive examples' time range and series.</summary>
    public ChartInteractive Interactive { get; } = new();
}
