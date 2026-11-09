using System.Text.Json.Serialization;

namespace Gallery.Models;

/// <summary>One fixture in one theme compared with shadcn/ui's render: the share of pixels within 16/255 and the mean difference.</summary>
public sealed record FidelityMeasure(string Fixture, string Theme, double Matched, double MeanDelta);

/// <summary>The measures of the last comparison (<c>tools/shadcn/compare.mjs</c>).</summary>
public sealed record FidelityData(IReadOnlyList<FidelityMeasure> Measures);

/// <summary>A fixture's row on the Fidelity page.</summary>
public sealed record FidelityRow(string Fixture, string Href, string Light, string Dark, string Reference);

/// <summary>The Fidelity page's view model: one row per fixture with its light and dark verdicts.</summary>
public sealed class FidelityReport
{
    public FidelityReport(FidelityData data)
    {
        ArgumentNullException.ThrowIfNull(data);
        Rows = [.. data.Measures.GroupBy(measure => measure.Fixture).Select(group => new FidelityRow(
            group.Key,
            "/fidelity/" + group.Key,
            Verdict(group.FirstOrDefault(measure => measure.Theme == "light")),
            Verdict(group.FirstOrDefault(measure => measure.Theme == "dark")),
            "/reference/" + group.Key + ".light.png"))];
        Average = data.Measures.Count == 0 ? "no measures" : $"{data.Measures.Average(measure => measure.Matched):0.00}% of pixels on average";
    }

    public IReadOnlyList<FidelityRow> Rows { get; }

    public string Average { get; }

    private static string Verdict(FidelityMeasure? measure) => measure is null ? "—" : $"{measure.Matched:0.00}% (Δ {measure.MeanDelta:0.00})";
}

[JsonSerializable(typeof(List<FidelityMeasure>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal sealed partial class FidelityJson : JsonSerializerContext;
