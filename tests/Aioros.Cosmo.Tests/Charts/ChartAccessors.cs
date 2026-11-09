using Aioros.Runtime.Rendering;
using Gallery.Models;

namespace Aioros.Cosmo.Tests.Charts;

/// <summary>
/// The accessors Aioros's generator emits for an app's chart rows, written here for the test assembly, which has no
/// generator: Pictor reads a row's members through them.
/// </summary>
internal static class ChartAccessors
{
    private static readonly Lazy<bool> Registered = new(() =>
    {
        MemberAccessors.Register(
        [
            new(typeof(ChartRow), nameof(ChartRow.Month), row => ((ChartRow)row).Month),
            new(typeof(ChartRow), nameof(ChartRow.Date), row => ((ChartRow)row).Date),
            new(typeof(ChartRow), nameof(ChartRow.Browser), row => ((ChartRow)row).Browser),
            new(typeof(ChartRow), nameof(ChartRow.Label), row => ((ChartRow)row).Label),
            new(typeof(ChartRow), nameof(ChartRow.Desktop), row => ((ChartRow)row).Desktop),
            new(typeof(ChartRow), nameof(ChartRow.Mobile), row => ((ChartRow)row).Mobile),
            new(typeof(ChartRow), nameof(ChartRow.Visitors), row => ((ChartRow)row).Visitors),
            new(typeof(ChartRow), nameof(ChartRow.Running), row => ((ChartRow)row).Running),
            new(typeof(ChartRow), nameof(ChartRow.Swimming), row => ((ChartRow)row).Swimming),
            new(typeof(ChartRow), nameof(ChartRow.Chrome), row => ((ChartRow)row).Chrome),
            new(typeof(ChartRow), nameof(ChartRow.Safari), row => ((ChartRow)row).Safari),
            new(typeof(ChartRow), nameof(ChartRow.Firefox), row => ((ChartRow)row).Firefox),
            new(typeof(ChartRow), nameof(ChartRow.Edge), row => ((ChartRow)row).Edge),
            new(typeof(ChartRow), nameof(ChartRow.Other), row => ((ChartRow)row).Other),
        ]);
        return true;
    });

    /// <summary>Registers the accessors once.</summary>
    public static void Register() => _ = Registered.Value;
}
