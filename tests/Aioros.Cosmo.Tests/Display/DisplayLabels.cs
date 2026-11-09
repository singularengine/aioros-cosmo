using Avalonia.Data.Converters;

namespace Aioros.Cosmo.Tests.Display;

/// <summary>Labels the display view computes from its model's states.</summary>
public static class DisplayLabels
{
    /// <summary>The save button's label while it saves and after.</summary>
    public static readonly IValueConverter Save = new FuncValueConverter<bool, string>(saving => saving ? "Saving..." : "Save");
}
