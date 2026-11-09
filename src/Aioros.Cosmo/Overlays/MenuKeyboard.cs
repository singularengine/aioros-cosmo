using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Radix Menu's keyboard for Avalonia's menus: the arrows stop at the first and the last item (Radix's <c>loop</c> is off),
/// a typed letter moves to the next item whose text starts with it (typeahead), and Down on a button whose flyout is a
/// menu opens it, as Return and Space do; Shift+F10 opens a control's context menu, as a browser's <c>contextmenu</c> event
/// does (Avalonia maps only the Apps key).
/// </summary>
internal static class MenuKeyboard
{
    public static void Register()
    {
        InputElement.KeyDownEvent.AddClassHandler<MenuItem>(Stop, RoutingStrategies.Tunnel);
        InputElement.TextInputEvent.AddClassHandler<MenuItem>(TypeAhead, RoutingStrategies.Tunnel);
        InputElement.KeyDownEvent.AddClassHandler<Button>(Open, RoutingStrategies.Bubble);
        InputElement.KeyUpEvent.AddClassHandler<Control>(Context, RoutingStrategies.Bubble);
    }

    private static void Context(Control control, KeyEventArgs e)
    {
        if (e.Handled || e.Key != Key.F10 || e.KeyModifiers != KeyModifiers.Shift || !ReferenceEquals(e.Source, control) || (control.ContextMenu is null && control.ContextFlyout is null))
        {
            return;
        }

        var request = new ContextRequestedEventArgs();
        control.RaiseEvent(request);
        e.Handled = request.Handled;
    }

    private static List<MenuItem> Siblings(MenuItem item) =>
        item.GetLogicalParent() is ItemsControl parent
            ? [.. parent.GetLogicalChildren().OfType<MenuItem>().Where(sibling => sibling.IsEffectivelyEnabled && sibling.Focusable && sibling.IsVisible)]
            : [];

    private static void Stop(MenuItem item, KeyEventArgs e)
    {
        if (e.Handled || !ReferenceEquals(e.Source, item) || e.Key is not (Key.Up or Key.Down))
        {
            return;
        }

        var siblings = Siblings(item);
        if (siblings.Count > 0 && ReferenceEquals(siblings[e.Key == Key.Up ? 0 : ^1], item))
        {
            e.Handled = true;
        }
    }

    private static void TypeAhead(MenuItem item, TextInputEventArgs e)
    {
        if (e.Handled || string.IsNullOrWhiteSpace(e.Text) || !ReferenceEquals(e.Source, item))
        {
            return;
        }

        var siblings = Siblings(item);
        var start = siblings.IndexOf(item);
        for (var step = 1; step <= siblings.Count; step++)
        {
            var next = siblings[(start + step + siblings.Count) % siblings.Count];
            if ((next.Header as string ?? next.Header?.ToString())?.StartsWith(e.Text, StringComparison.CurrentCultureIgnoreCase) == true)
            {
                Move(item, next);
                e.Handled = true;
                return;
            }
        }
    }

    private static void Move(MenuItem from, MenuItem to)
    {
        from.IsSelected = false;
        to.IsSelected = true;
        to.Focus(NavigationMethod.Directional);
    }

    private static void Open(Button button, KeyEventArgs e)
    {
        if (e.Handled || e.Key != Key.Down || button.Flyout is not MenuFlyout menu || menu.IsOpen)
        {
            return;
        }

        menu.ShowAt(button);
        e.Handled = true;
        if (menu.Items.OfType<MenuItem>().FirstOrDefault(item => item.IsEffectivelyEnabled && item.Focusable) is { } first)
        {
            first.IsSelected = true;
            first.Focus(NavigationMethod.Directional);
        }
    }
}
