using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Navigation;

/// <summary>
/// Radix Tabs' keyboard for Avalonia's <see cref="TabItem"/>: Right and Left (Down and Up when the strip stands at the
/// side, Radix's vertical orientation) move to the next and previous enabled tab, looping, Home and End to the first and
/// last, and the tab they reach is selected (Radix's automatic activation) and focused.
/// </summary>
public static class TabsKeyboard
{
    private static bool _registered;

    /// <summary>Registers the keys for every <see cref="TabItem"/> once; <c>CosmoTheme</c> calls it.</summary>
    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        InputElement.KeyDownEvent.AddClassHandler<TabItem>(Move, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
    }

    private static void Move(TabItem tab, KeyEventArgs e)
    {
        if (e.Handled || !ReferenceEquals(e.Source, tab) || tab.FindLogicalAncestorOfType<TabControl>() is not { } tabs)
        {
            return;
        }

        var enabled = tabs.GetRealizedContainers().OfType<TabItem>().Where(item => item.IsEffectivelyEnabled).ToList();
        var index = enabled.IndexOf(tab);
        if (index < 0 || enabled.Count == 0)
        {
            return;
        }

        var vertical = tabs.TabStripPlacement is Dock.Left or Dock.Right;
        var next = (e.Key, vertical) switch
        {
            (Key.Right, false) or (Key.Down, true) => enabled[(index + 1) % enabled.Count],
            (Key.Left, false) or (Key.Up, true) => enabled[(index - 1 + enabled.Count) % enabled.Count],
            (Key.Home, _) => enabled[0],
            (Key.End, _) => enabled[^1],
            _ => null,
        };
        if (next is null)
        {
            return;
        }

        tabs.SelectedIndex = tabs.IndexFromContainer(next);
        next.Focus(NavigationMethod.Directional);
        e.Handled = true;
    }
}
