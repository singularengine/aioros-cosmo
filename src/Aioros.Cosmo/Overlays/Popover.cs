using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// The open state of a control's flyout (its <c>Button.Flyout</c> or <c>FlyoutBase.AttachedFlyout</c>, a popover or a
/// menu), two-way bindable as Radix's <c>open</c> and <c>onOpenChange</c>: setting it opens or closes the flyout, and the
/// flyout opening or closing (a click, Escape, a click outside) sets it.
/// </summary>
public static class Popover
{
    /// <summary>Defines the <c>IsOpen</c> attached property.</summary>
    public static readonly AttachedProperty<bool> IsOpenProperty =
        AvaloniaProperty.RegisterAttached<Control, bool>("IsOpen", typeof(Popover), defaultBindingMode: BindingMode.TwoWay);

    private static readonly AttachedProperty<FlyoutBase?> WatchedProperty =
        AvaloniaProperty.RegisterAttached<Control, FlyoutBase?>("Watched", typeof(Popover));

    /// <summary>Whether the control's flyout is open.</summary>
    public static bool GetIsOpen(Control control) => control.GetValue(IsOpenProperty);

    /// <summary>Opens or closes the control's flyout.</summary>
    public static void SetIsOpen(Control control, bool value) => control.SetValue(IsOpenProperty, value);

    static Popover()
    {
        IsOpenProperty.Changed.AddClassHandler<Control>((control, change) => Apply(control, change.GetNewValue<bool>()));
        Button.FlyoutProperty.Changed.AddClassHandler<Button>((button, _) => Watch(button));
        FlyoutBase.AttachedFlyoutProperty.Changed.AddClassHandler<Control>((control, _) => Watch(control));
        Control.LoadedEvent.AddClassHandler<Control>((control, _) =>
        {
            if (control.IsSet(IsOpenProperty))
            {
                Watch(control);
                if (GetIsOpen(control))
                {
                    Apply(control, true);
                }
            }
        });
    }

    internal static void Register() => _ = IsOpenProperty;

    private static FlyoutBase? FlyoutOf(Control control) => (control as Button)?.Flyout ?? FlyoutBase.GetAttachedFlyout(control);

    private static void Watch(Control control)
    {
        var flyout = FlyoutOf(control);
        var watched = control.GetValue(WatchedProperty);
        if (ReferenceEquals(flyout, watched))
        {
            return;
        }

        if (watched is not null)
        {
            watched.Opened -= OnOpened;
            watched.Closed -= OnClosed;
        }

        control.SetValue(WatchedProperty, flyout);
        if (flyout is not null)
        {
            flyout.Opened += OnOpened;
            flyout.Closed += OnClosed;
        }
    }

    private static void OnOpened(object? sender, EventArgs e) => Report(sender, true);

    private static void OnClosed(object? sender, EventArgs e) => Report(sender, false);

    private static void Report(object? sender, bool open)
    {
        if (sender is FlyoutBase { Target: Control target } && GetIsOpen(target) != open)
        {
            target.SetCurrentValue(IsOpenProperty, open);
        }
    }

    private static void Apply(Control control, bool open)
    {
        Watch(control);
        if (FlyoutOf(control) is not { } flyout || flyout.IsOpen == open)
        {
            return;
        }

        if (open && TopLevel.GetTopLevel(control) is null)
        {
            control.AttachedToVisualTree += Opened;
            void Opened(object? sender, VisualTreeAttachmentEventArgs e)
            {
                control.AttachedToVisualTree -= Opened;
                Apply(control, GetIsOpen(control));
            }
        }
        else if (open)
        {
            flyout.ShowAt(control);
        }
        else
        {
            flyout.Hide();
        }
    }
}
