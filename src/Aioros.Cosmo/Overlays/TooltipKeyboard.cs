using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Radix Tooltip's keyboard for Avalonia's <see cref="ToolTip"/>: a control that has a tip shows it while it holds the
/// keyboard focus (Tab or the arrows, not a click), and Escape hides it.
/// </summary>
internal static class TooltipKeyboard
{
    public static void Register()
    {
        InputElement.GotFocusEvent.AddClassHandler<Control>(Focused, RoutingStrategies.Bubble);
        InputElement.LostFocusEvent.AddClassHandler<Control>(Blurred, RoutingStrategies.Bubble);
        InputElement.KeyDownEvent.AddClassHandler<Control>(Escaped, RoutingStrategies.Bubble);
    }

    private static void Focused(Control control, FocusChangedEventArgs e)
    {
        if (ReferenceEquals(e.Source, control) && e.NavigationMethod is NavigationMethod.Tab or NavigationMethod.Directional && ToolTip.GetTip(control) is not null)
        {
            ToolTip.SetIsOpen(control, true);
        }
    }

    private static void Blurred(Control control, RoutedEventArgs e)
    {
        if (ReferenceEquals(e.Source, control) && ToolTip.GetIsOpen(control))
        {
            ToolTip.SetIsOpen(control, false);
        }
    }

    private static void Escaped(Control control, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && !e.Handled && ReferenceEquals(e.Source, control) && ToolTip.GetIsOpen(control))
        {
            ToolTip.SetIsOpen(control, false);
            e.Handled = true;
        }
    }
}
