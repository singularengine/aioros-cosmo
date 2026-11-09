using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Threading;

namespace Aioros.Cosmo.Navigation;

/// <summary>
/// shadcn/ui's NavigationMenu (Radix NavigationMenu, <c>viewport={false}</c>): a row of
/// <see cref="NavigationMenuTrigger"/>s and links 4 px apart. A trigger opens its content when pressed or after the pointer
/// rests on it for <see cref="DelayDuration"/>; while one is open, moving onto another trigger opens that one at once, and
/// leaving both the trigger and its content closes it after <see cref="CloseDelay"/>. Left and Right move the focus between
/// the items.
/// </summary>
public sealed class NavigationMenu : StackPanel
{
    private ITimer? _timer;

    /// <summary>Lays the items out as Radix's list and hooks the pointer and the keys.</summary>
    public NavigationMenu()
    {
        Orientation = Orientation.Horizontal;
        Spacing = 4;
        AutomationProperties.SetLandmarkType(this, AutomationLandmarkType.Navigation);
        AutomationProperties.SetName(this, "Main");
        XYFocus.SetNavigationModes(this, XYFocusNavigationModes.Keyboard);
        AddHandler(KeyDownEvent, Move, RoutingStrategies.Bubble);
    }

    /// <summary>How long the pointer rests on a trigger before its content opens (Radix's <c>delayDuration</c>, 200 ms).</summary>
    public TimeSpan DelayDuration { get; set; } = TimeSpan.FromMilliseconds(200);

    /// <summary>How long after the pointer leaves an open item it closes (Radix's close timer, 150 ms).</summary>
    public TimeSpan CloseDelay { get; set; } = TimeSpan.FromMilliseconds(150);

    /// <summary>The clock of the delays; the system's unless a test sets its own.</summary>
    public TimeProvider Time { get; set; } = TimeProvider.System;

    /// <summary>The trigger whose content is open, if any.</summary>
    public NavigationMenuTrigger? OpenTrigger => Children.OfType<NavigationMenuTrigger>().FirstOrDefault(trigger => trigger.Flyout?.IsOpen == true);

    internal void Leave()
    {
        if (OpenTrigger is { } open)
        {
            Schedule(CloseDelay, () =>
            {
                if (!open.IsPointerOver && !open.IsContentPointerOver)
                {
                    open.Flyout?.Hide();
                }
            });
        }
    }

    internal void Stay() => Cancel();

    internal void Enter(NavigationMenuTrigger trigger)
    {
        if (trigger.Flyout is not { } flyout || flyout.IsOpen)
        {
            Cancel();
            return;
        }

        if (OpenTrigger is { } open)
        {
            Cancel();
            Switch(open, trigger);
            return;
        }

        Schedule(DelayDuration, () =>
        {
            if (trigger.IsPointerOver && !flyout.IsOpen)
            {
                flyout.ShowAt(trigger);
            }
        });
    }

    /// <summary>
    /// Shows a trigger's content in place of another's, sliding as Radix's <c>data-motion</c>: the new content enters
    /// from the side of the one it replaces (from the end when it is after it) and the old one leaves toward the other side.
    /// </summary>
    internal void Switch(NavigationMenuTrigger from, NavigationMenuTrigger to)
    {
        var later = Children.IndexOf(to) > Children.IndexOf(from);
        from.Leaving(later ? "to-start" : "to-end");
        to.Entering(later ? "from-end" : "from-start");
        from.Flyout?.Hide();
        to.Flyout?.ShowAt(to);
    }

    private void Move(object? sender, KeyEventArgs e)
    {
        if (e.Key is not (Key.Left or Key.Right) || e.Source is not Control source)
        {
            return;
        }

        var items = Children.Where(child => child.IsVisible && child.Focusable && child.IsEffectivelyEnabled).ToList();
        var index = items.IndexOf(source);
        if (index < 0)
        {
            return;
        }

        var next = index + (e.Key == Key.Right ? 1 : -1);
        if (next >= 0 && next < items.Count)
        {
            items[next].Focus(NavigationMethod.Directional);
        }

        e.Handled = true;
    }

    private void Schedule(TimeSpan delay, Action action)
    {
        Cancel();
        _timer = Time.CreateTimer(_ => Dispatcher.UIThread.Post(action), null, delay, Timeout.InfiniteTimeSpan);
    }

    private void Cancel()
    {
        _timer?.Dispose();
        _timer = null;
    }
}

/// <summary>
/// shadcn/ui's NavigationMenuTrigger: a 36 px medium button with Lucide's <c>chevron-down</c> that turns while its
/// <see cref="Button.Flyout"/>, the content, is open. The content opens under the trigger's start, 6 px below, in shadcn's
/// <c>navigation-menu</c> look, and closes when one of its links is chosen (Radix's <c>onSelect</c>).
/// </summary>
public sealed class NavigationMenuTrigger : Button
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(NavigationMenuTrigger);

    private static readonly string[] Motions = ["from-start", "from-end", "to-start", "to-end"];

    private string? _entering;

    internal bool IsContentPointerOver { get; private set; }

    /// <summary>The content's next opening replaces another's and enters from that side (<c>from-end</c>, <c>from-start</c>).</summary>
    internal void Entering(string motion) => _entering = motion;

    /// <summary>The open content leaves toward a side as another replaces it (<c>to-start</c>, <c>to-end</c>).</summary>
    internal void Leaving(string motion)
    {
        if ((Flyout as Flyout)?.Content is Control { Parent: FlyoutPresenter presenter })
        {
            presenter.Classes.RemoveAll(Motions);
            presenter.Classes.Add(motion);
        }
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == FlyoutProperty && change.NewValue is Flyout flyout)
        {
            flyout.Placement = PlacementMode.BottomEdgeAlignedLeft;
            flyout.VerticalOffset = 6;
            flyout.FlyoutPresenterClasses.Add("navigation-menu");
            flyout.Opening += (_, _) => Move(flyout);
            flyout.Opened += (_, _) => Watch(flyout);
            flyout.Closed += (_, _) => IsContentPointerOver = false;
        }
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Flyout is Flyout flyout)
        {
            flyout.OverlayInputPassThroughElement = Parent as NavigationMenu ?? (IInputElement)this;
        }
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        if (Flyout?.IsOpen == true)
        {
            Flyout.Hide();
            return;
        }

        if (Parent is NavigationMenu { OpenTrigger: { } open } menu && !ReferenceEquals(open, this))
        {
            menu.Switch(open, this);
            return;
        }

        base.OnClick();
    }

    /// <summary>Gives the opening content the side it enters from when it replaces another's, and none otherwise.</summary>
    private void Move(Flyout flyout)
    {
        var motion = _entering;
        _entering = null;
        if (flyout.Content is Control { Parent: FlyoutPresenter presenter })
        {
            presenter.Classes.RemoveAll(Motions);
            if (motion is not null)
            {
                presenter.Classes.Add(motion);
            }
        }
    }

    /// <inheritdoc />
    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        (Parent as NavigationMenu)?.Enter(this);
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        (Parent as NavigationMenu)?.Leave();
    }

    private void Watch(Flyout flyout)
    {
        if (flyout.Content is not Control content || content.Parent is not Control presenter)
        {
            return;
        }

        presenter.PointerEntered -= ContentEntered;
        presenter.PointerExited -= ContentExited;
        presenter.RemoveHandler(ClickEvent, Chosen);
        presenter.PointerEntered += ContentEntered;
        presenter.PointerExited += ContentExited;
        presenter.AddHandler(ClickEvent, Chosen);
    }

    private void Chosen(object? sender, RoutedEventArgs e) => Dispatcher.UIThread.Post(() => Flyout?.Hide());

    private void ContentEntered(object? sender, PointerEventArgs e)
    {
        IsContentPointerOver = true;
        (Parent as NavigationMenu)?.Stay();
    }

    private void ContentExited(object? sender, PointerEventArgs e)
    {
        IsContentPointerOver = false;
        (Parent as NavigationMenu)?.Leave();
    }
}
