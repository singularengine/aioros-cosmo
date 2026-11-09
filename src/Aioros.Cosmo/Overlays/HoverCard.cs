using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Radix's HoverCard for any control: <c>c:HoverCard.Card</c> is shown in a popover below the control (the presenter's
/// <c>hover-card</c> class, shadcn's <c>HoverCardContent</c>) once the pointer has rested on the control for
/// <see cref="GetOpenDelay"/> milliseconds (700) or the control has the keyboard focus, and stays open while the pointer
/// is over the control or the card, closing <see cref="GetCloseDelay"/> milliseconds (300) after it left both.
/// </summary>
public static class HoverCard
{
    /// <summary>Defines the <c>Card</c> attached property.</summary>
    public static readonly AttachedProperty<object?> CardProperty = AvaloniaProperty.RegisterAttached<Control, object?>("Card", typeof(HoverCard));

    /// <summary>Defines the <c>OpenDelay</c> attached property.</summary>
    public static readonly AttachedProperty<int> OpenDelayProperty = AvaloniaProperty.RegisterAttached<Control, int>("OpenDelay", typeof(HoverCard), 700);

    /// <summary>Defines the <c>CloseDelay</c> attached property.</summary>
    public static readonly AttachedProperty<int> CloseDelayProperty = AvaloniaProperty.RegisterAttached<Control, int>("CloseDelay", typeof(HoverCard), 300);

    /// <summary>Defines the <c>Placement</c> attached property.</summary>
    public static readonly AttachedProperty<PlacementMode> PlacementProperty =
        AvaloniaProperty.RegisterAttached<Control, PlacementMode>("Placement", typeof(HoverCard), PlacementMode.Bottom);

    private static readonly AttachedProperty<HoverCardFlyout?> FlyoutProperty = AvaloniaProperty.RegisterAttached<Control, HoverCardFlyout?>("Flyout", typeof(HoverCard));

    static HoverCard()
    {
        CardProperty.Changed.AddClassHandler<Control>((control, change) => Attach(control, change.GetNewValue<object?>()));
        PlacementProperty.Changed.AddClassHandler<Control>((control, change) =>
        {
            if (control.GetValue(FlyoutProperty) is { } flyout)
            {
                flyout.Placement = change.GetNewValue<PlacementMode>();
            }
        });
    }

    /// <summary>The card's content.</summary>
    public static object? GetCard(Control control) => control.GetValue(CardProperty);

    /// <summary>Sets the card's content.</summary>
    public static void SetCard(Control control, object? value) => control.SetValue(CardProperty, value);

    /// <summary>How long the pointer rests on the control before the card opens, in milliseconds (Radix's <c>openDelay</c>, 700).</summary>
    public static int GetOpenDelay(Control control) => control.GetValue(OpenDelayProperty);

    /// <summary>Sets the open delay.</summary>
    public static void SetOpenDelay(Control control, int value) => control.SetValue(OpenDelayProperty, value);

    /// <summary>How long the card stays once the pointer left the control and the card, in milliseconds (Radix's <c>closeDelay</c>, 300).</summary>
    public static int GetCloseDelay(Control control) => control.GetValue(CloseDelayProperty);

    /// <summary>Sets the close delay.</summary>
    public static void SetCloseDelay(Control control, int value) => control.SetValue(CloseDelayProperty, value);

    /// <summary>Where the card opens (below the control, centered, by default).</summary>
    public static PlacementMode GetPlacement(Control control) => control.GetValue(PlacementProperty);

    /// <summary>Sets where the card opens.</summary>
    public static void SetPlacement(Control control, PlacementMode value) => control.SetValue(PlacementProperty, value);

    /// <summary>Whether the control's card is open.</summary>
    public static bool IsOpen(Control control) => control.GetValue(FlyoutProperty)?.IsOpen == true;

    private static void Attach(Control control, object? card)
    {
        if (control.GetValue(FlyoutProperty) is { } existing)
        {
            existing.Content = card;
            return;
        }

        var flyout = new HoverCardFlyout(control) { Content = card, Placement = GetPlacement(control), ShowMode = FlyoutShowMode.Transient };
        flyout.FlyoutPresenterClasses.Add("hover-card");
        control.SetValue(FlyoutProperty, flyout);
        FlyoutBase.SetAttachedFlyout(control, flyout);
        control.PointerEntered += (_, _) => flyout.Enter();
        control.PointerExited += (_, pointer) => flyout.Moved(pointer);
        control.GotFocus += (_, focus) =>
        {
            if (focus.NavigationMethod == NavigationMethod.Tab)
            {
                flyout.Enter();
            }
        };
        control.LostFocus += (_, _) => flyout.Leave();
    }

    private sealed class HoverCardFlyout : Flyout
    {
        private readonly Control _owner;
        private IDisposable? _pending;
        private Control? _presenter;
        private TopLevel? _top;

        public HoverCardFlyout(Control owner)
        {
            _owner = owner;
            Opened += (_, _) =>
            {
                _top = TopLevel.GetTopLevel(owner);
                _top?.AddHandler(InputElement.PointerMovedEvent, OnPointerMoved, Avalonia.Interactivity.RoutingStrategies.Tunnel, handledEventsToo: true);
            };
            Closed += (_, _) =>
            {
                _top?.RemoveHandler(InputElement.PointerMovedEvent, OnPointerMoved);
                _top = null;
            };
        }

        private void OnPointerMoved(object? sender, PointerEventArgs e) => Moved(e);

        public void Moved(PointerEventArgs e)
        {
            if (Inside(_owner, e) || (_presenter is not null && Inside(_presenter, e)))
            {
                Enter();
            }
            else
            {
                Leave();
            }
        }

        private static bool Inside(Control control, PointerEventArgs e) =>
            control.IsEffectivelyVisible && TopLevel.GetTopLevel(control) is not null && new Rect(control.Bounds.Size).Contains(e.GetPosition(control));

        public void Enter()
        {
            _pending?.Dispose();
            _pending = null;
            if (IsOpen)
            {
                return;
            }

            Later(GetOpenDelay(_owner), () => ShowAt(_owner));
        }

        public void Leave()
        {
            if (!IsOpen)
            {
                _pending?.Dispose();
                _pending = null;
                return;
            }

            if (_pending is null)
            {
                Later(GetCloseDelay(_owner), Hide);
            }
        }

        private void Later(int delay, Action action)
        {
            if (delay <= 0)
            {
                action();
                return;
            }

            _pending = DispatcherTimer.RunOnce(() =>
            {
                _pending = null;
                action();
            }, TimeSpan.FromMilliseconds(delay));
        }

        protected override Control CreatePresenter()
        {
            _presenter = base.CreatePresenter();
            return _presenter;
        }
    }
}
