using Aioros.Pages;
using Gallery.Models;

namespace Gallery.Pages.Themes;

/// <summary>Loads the theme customizer for the color theme and the radius in the query.</summary>
public static class ThemesLoader
{
    public static Task<LoaderResult<ThemeSelection>> LoadAsync(string? color, string? radius) =>
        Task.FromResult(LoaderResult.With(new ThemeSelection(color, radius), "Themes", new PageMetadata("Pick one of shadcn/ui's color themes and radii: Cosmo swaps resource dictionaries, on the web and the desktop.")));
}
