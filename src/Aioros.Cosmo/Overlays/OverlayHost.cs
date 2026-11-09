using Avalonia;
using Avalonia.Controls;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Marks the panel that dialogs, alert dialogs, sheets and drawers open in: their mask and surface are added to the
/// nearest ancestor panel with <c>c:OverlayHost.IsHost="True"</c> (a <see cref="Panel"/> or a <see cref="Grid"/> whose
/// children overlap, usually the window's root), so they cover it as Radix's fixed overlay covers the viewport. Without a
/// host they open in the window's overlay layer. On the web, Aioros writes what a pending command added to such a panel
/// as a modal <c>&lt;dialog&gt;</c> placed on the panel's box.
/// </summary>
public static class OverlayHost
{
    /// <summary>Defines the <c>IsHost</c> attached property.</summary>
    public static readonly AttachedProperty<bool> IsHostProperty = AvaloniaProperty.RegisterAttached<Panel, bool>("IsHost", typeof(OverlayHost));

    /// <summary>Whether the panel hosts dialogs.</summary>
    public static bool GetIsHost(Panel panel) => panel.GetValue(IsHostProperty);

    /// <summary>Makes the panel host dialogs.</summary>
    public static void SetIsHost(Panel panel, bool value) => panel.SetValue(IsHostProperty, value);
}
