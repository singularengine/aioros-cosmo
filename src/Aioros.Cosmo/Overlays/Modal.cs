using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// What Radix's Dialog does for <see cref="Dialog"/>, <see cref="AlertDialog"/>, <see cref="Sheet"/> and
/// <see cref="Drawer"/>: while its owner is open, a mask and a <see cref="DialogSurface"/> showing the owner's content
/// cover the nearest <see cref="OverlayHost"/> panel (or the window's overlay layer); the focus moves into the surface and
/// cycles there, Escape and (when light dismiss is on) a click on the mask close it, and the focus goes back where it
/// was. Closing sets the owner's <c>IsOpen</c> back, so a two-way binding tells the model.
/// </summary>
internal sealed class Modal(Control owner, string kind, StyledProperty<bool> isOpen)
{
    private Border? _mask;
    private DialogSurface? _surface;
    private Panel? _host;
    private Panel? _layer;
    private IInputElement? _previous;
    private readonly List<Leaving> _leaving = [];

    public DialogSurface? Surface => _surface;

    public void Toggle(bool open)
    {
        open = open || Web.GetShows(owner);
        if (open && _surface is null)
        {
            Show();
        }
        else if (!open && _surface is not null)
        {
            Hide();
        }
    }

    /// <summary>
    /// Makes a button the owner's trigger (Radix's <c>DialogTrigger</c>): a click opens it on the desktop, unless
    /// <paramref name="clicks"/> is false (a trigger whose own command opens it), and the browser opens it on the web with no
    /// request (<c>a:Web.Opens</c>); the previous trigger is let go.
    /// </summary>
    public void Trigger(Button? previous, Button? trigger, bool clicks = true)
    {
        if (previous is not null)
        {
            previous.Click -= OnTrigger;
            Web.SetOpens(previous, null);
        }

        if (trigger is not null)
        {
            if (clicks)
            {
                trigger.Click += OnTrigger;
            }

            Web.SetOpens(trigger, owner);
        }
    }

    public void Refresh()
    {
        if (_surface is not null)
        {
            Fill(_surface);
        }
    }

    public void Close()
    {
        if (owner.GetValue(isOpen))
        {
            owner.SetCurrentValue(isOpen, false);
        }
        else
        {
            Hide();
        }
    }

    private void Show()
    {
        Finish();
        var host = owner.GetVisualAncestors().OfType<Panel>().FirstOrDefault(panel => OverlayHost.GetIsHost(panel));
        if (host is null && OverlayLayer.GetOverlayLayer(owner) is null)
        {
            return;
        }

        _previous = TopLevel.GetTopLevel(owner)?.FocusManager?.GetFocusedElement();
        _mask = new Border { Classes = { "cosmo-overlay" } };
        _mask[!Border.BackgroundProperty] = _mask.GetResourceObservable("CosmoOverlayBrush").ToBinding();
        _mask.PointerPressed += OnMaskPressed;
        _surface = new DialogSurface();
        Mark(_mask, _surface);
        _surface.Classes.Add(kind);
        _surface.Classes.AddRange(owner.Classes);
        _surface[!StyledElement.DataContextProperty] = owner[!StyledElement.DataContextProperty];
        KeyboardNavigation.SetTabNavigation(_surface, KeyboardNavigationMode.Cycle);
        _surface.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Bubble);
        _surface.CloseRequested += OnCloseRequested;
        _surface.Loaded += OnLoaded;
        if (owner is Drawer drawer)
        {
            Drag(drawer, _mask, _surface);
        }

        Fill(_surface);
        if (host is not null)
        {
            _host = host;
            Size(_surface, host.Bounds.Size);
            host.Children.Add(_mask);
            host.Children.Add(_surface);
            return;
        }

        var overlay = OverlayLayer.GetOverlayLayer(owner)!;
        Size(_surface, overlay.Bounds.Size);
        _layer = new Panel { Width = overlay.Bounds.Width, Height = overlay.Bounds.Height, Children = { _mask, _surface } };
        overlay.PropertyChanged += OnLayerChanged;
        overlay.Children.Add(_layer);
    }

    private void Fill(DialogSurface surface)
    {
        surface.Content = owner.GetValue(Dialog.ContentProperty);
        surface.ContentTemplate = owner.GetValue(Dialog.ContentTemplateProperty);
        surface.ShowCloseButton = owner.GetValue(Dialog.ShowCloseButtonProperty);
        Forward(surface, Layoutable.WidthProperty);
        Forward(surface, Layoutable.MaxWidthProperty);
        Forward(surface, Layoutable.MaxHeightProperty);
        if (((owner as Sheet)?.Side ?? (owner as Drawer)?.Side) is { } side)
        {
            surface.Classes.Set("top", side == SheetSide.Top);
            surface.Classes.Set("right", side == SheetSide.Right);
            surface.Classes.Set("bottom", side == SheetSide.Bottom);
            surface.Classes.Set("left", side == SheetSide.Left);
        }
    }

    private void Forward(DialogSurface surface, StyledProperty<double> property)
    {
        if (owner.IsSet(property))
        {
            surface.SetValue(property, owner.GetValue(property));
        }
    }

    /// <summary>
    /// Closes the overlay: the focus goes back at once, and the mask and the surface, passing clicks through, stay while
    /// their exit plays (<see cref="Motion.ExitAsync"/>), as Radix's <c>Presence</c> keeps closed content for its exit.
    /// </summary>
    private void Hide()
    {
        if (_surface is null || _mask is null)
        {
            return;
        }

        _surface.CloseRequested -= OnCloseRequested;
        _surface.Loaded -= OnLoaded;
        _surface.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        _mask.PointerPressed -= OnMaskPressed;
        _mask.IsHitTestVisible = false;
        _surface.IsHitTestVisible = false;
        var leaving = new Leaving(_mask, _surface, _host, _layer);
        (_mask, _surface, _host, _layer) = (null, null, null, null);
        var previous = _previous;
        _previous = null;
        previous?.Focus(NavigationMethod.Tab);
        _leaving.Add(leaving);
        var exit = Motion.ExitAsync(leaving.Mask, leaving.Surface);
        if (exit.IsCompleted)
        {
            Remove(leaving);
            return;
        }

        _ = exit.ContinueWith(_ => Remove(leaving), TaskScheduler.FromCurrentSynchronizationContext());
    }

    /// <summary>Ends the exits still playing at once, so an overlay opened again takes back its content.</summary>
    private void Finish()
    {
        foreach (var leaving in _leaving.ToList())
        {
            Remove(leaving);
        }
    }

    private void Remove(Leaving leaving)
    {
        if (!_leaving.Remove(leaving))
        {
            return;
        }

        leaving.Surface.Content = null;
        if (leaving.Host is not null)
        {
            leaving.Host.Children.Remove(leaving.Mask);
            leaving.Host.Children.Remove(leaving.Surface);
        }
        else if (leaving.Layer is not null && OverlayLayer.GetOverlayLayer(owner) is { } overlay)
        {
            overlay.PropertyChanged -= OnLayerChanged;
            overlay.Children.Remove(leaving.Layer);
        }
    }

    /// <summary>A side sheet's or drawer's width: three quarters of its host, 384 px at most (shadcn's <c>w-3/4 sm:max-w-sm</c>), unless set.</summary>
    private void Size(DialogSurface surface, Size host)
    {
        if (owner is Sheet { Side: SheetSide.Left or SheetSide.Right } or Drawer { Side: SheetSide.Left or SheetSide.Right } && !owner.IsSet(Layoutable.WidthProperty) && host.Width > 0)
        {
            surface.Width = Math.Min(host.Width * 0.75, 384);
        }

        if (owner is Drawer { Side: SheetSide.Top or SheetSide.Bottom } && !owner.IsSet(Layoutable.MaxHeightProperty) && host.Height > 0)
        {
            surface.MaxHeight = host.Height * 0.8;
        }
    }

    private void OnLayerChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.BoundsProperty && sender is Visual overlay && _layer is not null)
        {
            (_layer.Width, _layer.Height) = (overlay.Bounds.Width, overlay.Bounds.Height);
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (_surface is null)
        {
            return;
        }


        var focusable = _surface.GetVisualDescendants().OfType<InputElement>()
            .Where(element => element.Focusable && element.IsEffectivelyEnabled && element.IsEffectivelyVisible && KeyboardNavigation.GetIsTabStop(element))
            .ToList();
        var first = focusable.OfType<Button>().FirstOrDefault(button => button.IsCancel && owner is AlertDialog) ?? focusable.FirstOrDefault();
        first?.Focus(NavigationMethod.Tab);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && !e.Handled)
        {
            e.Handled = true;
            Close();
        }
    }

    private void OnMaskPressed(object? sender, PointerPressedEventArgs e)
    {
        if (owner.GetValue(Dialog.IsLightDismissEnabledProperty))
        {
            e.Handled = true;
            Close();
        }
    }

    private void OnCloseRequested(object? sender, RoutedEventArgs e) => Close();

    private void OnTrigger(object? sender, RoutedEventArgs e) => owner.SetCurrentValue(isOpen, true);

    /// <summary>A closed overlay's mask and surface while their exit plays, and where they were added.</summary>
    private sealed record Leaving(Border Mask, DialogSurface Surface, Panel? Host, Panel? Layer);

    /// <summary>A drawer's surface is dragged toward its edge to close as vaul's (<see cref="DrawerDrag"/>), and declares the same drag for the web: toward its edge, past a quarter of its size or faster than 0.4 px/ms.</summary>
    private void Drag(Drawer drawer, Border mask, DialogSurface surface)
    {
        _ = new DrawerDrag(surface, mask, drawer.TimeProvider, Close, drawer.Side);
        Web.SetSwipe(surface, drawer.Side switch { SheetSide.Right => "Right", SheetSide.Left => "Left", SheetSide.Top => "Up", _ => "Down" });
        Web.SetSwipeDistance(surface, 0.25);
        Web.SetSwipeVelocity(surface, 0.4);
    }

    /// <summary>Marks the mask and the surface as the owner's overlay for the web (<c>a:Web.ModalOf</c>), and whether a click on the mask closes it.</summary>
    private void Mark(Control mask, Control surface)
    {
        Web.SetModalOf(mask, owner);
        Web.SetModalOf(surface, owner);
        Web.SetLightDismiss(owner, owner.GetValue(Dialog.IsLightDismissEnabledProperty));
    }
}
