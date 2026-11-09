using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Forms;

/// <summary>
/// Radix Radio Group's arrow keys for Avalonia's <see cref="RadioButton"/>: Down and Right check and focus the next
/// enabled radio of its group, Up and Left the previous one, looping at the ends, as a browser's radios do.
/// </summary>
public static class RadioKeyboard
{
    private static bool _registered;

    /// <summary>Registers the arrow keys for every <see cref="RadioButton"/> once; <c>CosmoTheme</c> calls it.</summary>
    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        InputElement.KeyDownEvent.AddClassHandler<RadioButton>(Move, RoutingStrategies.Bubble);
    }

    private static void Move(RadioButton radio, KeyEventArgs e)
    {
        var step = e.Key switch
        {
            Key.Down or Key.Right => 1,
            Key.Up or Key.Left => -1,
            _ => 0,
        };
        if (step == 0 || e.Handled || radio.Parent is not Panel panel)
        {
            return;
        }

        var group = panel.Children.OfType<RadioButton>().Where(member => member.GroupName == radio.GroupName && (member == radio || member.IsEffectivelyEnabled)).ToList();
        var index = group.IndexOf(radio);
        if (group.Count < 2 || index < 0)
        {
            return;
        }

        var next = group[(index + step + group.Count) % group.Count];
        next.IsChecked = true;
        next.Focus(NavigationMethod.Directional);
        e.Handled = true;
    }
}
