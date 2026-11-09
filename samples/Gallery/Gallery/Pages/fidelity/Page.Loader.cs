using System.Text.Json;
using Aioros.Pages;
using Gallery.Models;

namespace Gallery.Pages.Fidelity;

/// <summary>Loads the last comparison of Cosmo's fixtures with shadcn/ui's renders.</summary>
public static class FidelityLoader
{
    public static async Task<LoaderResult<FidelityData>> LoadAsync(CancellationToken cancellationToken)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Reference", "summary.json");
        var measures = File.Exists(path)
            ? JsonSerializer.Deserialize(await File.ReadAllTextAsync(path, cancellationToken), FidelityJson.Default.ListFidelityMeasure) ?? []
            : [];
        return LoaderResult.With(new FidelityData(measures), "Fidelity", new PageMetadata("Cosmo's components compared pixel by pixel with shadcn/ui's own render in Chromium."));
    }
}
