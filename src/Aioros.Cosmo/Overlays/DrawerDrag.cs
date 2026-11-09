using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Vaul's drag on a drawer's surface: pressed outside a text box and a scrolled area, the surface follows the pointer
/// toward the edge it sits on (down for a bottom drawer, right for a right one) with no transition while the mask fades by
/// the share of the surface's size it moved, and away from it only by vaul's dampened distance
/// (<c>8 · (ln(d + 1) − 2)</c>); let go toward the edge past a quarter of its size or faster than 0.4 px/ms the drawer
/// closes from where the surface stands, otherwise the surface and the mask move back.
/// </summary>
internal sealed class DrawerDrag
{
    private const double Threshold = 0.25;

    private const double Speed = 0.4;

    private readonly DialogSurface _surface;
    private readonly Border _mask;
    private readonly TimeProvider _clock;
    private readonly Action _close;
    private readonly SheetSide _side;
    private Point? _start;
    private long _started;
    private bool _moved;
    private double _down;

    public DrawerDrag(DialogSurface surface, Border mask, TimeProvider clock, Action close, SheetSide side)
    {
        (_surface, _mask, _clock, _close, _side) = (surface, mask, clock, close, side);
        surface.PointerPressed += OnPressed;
        surface.PointerMoved += OnMoved;
        surface.PointerReleased += OnReleased;
        surface.PointerCaptureLost += OnCaptureLost;
    }

    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        var source = e.Source as Visual;
        if (!e.GetCurrentPoint(_surface).Properties.IsLeftButtonPressed
            || source?.GetSelfAndVisualAncestors().TakeWhile(visual => !ReferenceEquals(visual, _surface)).Any(visual => visual is TextBox or ScrollViewer { Offset.Y: > 0 }) == true)
        {
            return;
        }

        _start = e.GetPosition(_surface.GetVisualParent() as Visual ?? _surface);
        _started = _clock.GetTimestamp();
        (_moved, _down) = (false, 0);
    }

    private void OnMoved(object? sender, PointerEventArgs e)
    {
        if (_start is not { } start)
        {
            return;
        }

        var position = e.GetPosition(_surface.GetVisualParent() as Visual ?? _surface);
        var delta = _side switch
        {
            SheetSide.Right => position.X - start.X,
            SheetSide.Left => start.X - position.X,
            SheetSide.Top => start.Y - position.Y,
            _ => position.Y - start.Y,
        };
        if (!_moved && Math.Abs(delta) <= 1)
        {
            return;
        }

        if (!_moved)
        {
            _moved = true;
            e.Pointer.Capture(_surface);
            _surface.SetValue(Animatable.TransitionsProperty, null);
            _mask.SetValue(Animatable.TransitionsProperty, null);
        }

        _down = delta;
        var moved = delta > 0 ? delta : -Math.Max(8 * (Math.Log(-delta + 1) - 2), 0);
        var transform = TransformOperations.CreateBuilder(1);
        transform.AppendTranslate(_side switch { SheetSide.Right => moved, SheetSide.Left => -moved, _ => 0 }, _side switch { SheetSide.Bottom => moved, SheetSide.Top => -moved, _ => 0 });
        _surface.RenderTransform = transform.Build();
        _mask.Opacity = 1 - (Math.Max(moved, 0) / Math.Max(Size, 1));
    }

    private void OnReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (_start is null)
        {
            return;
        }

        _start = null;
        e.Pointer.Capture(null);
        if (!_moved)
        {
            return;
        }

        _moved = false;
        var elapsed = Math.Max(_clock.GetElapsedTime(_started).TotalMilliseconds, 1);
        Settle();
        if (_down > 0 && (_down / elapsed > Speed || _down >= Size * Threshold))
        {
            _close();
            return;
        }

        _surface.ClearValue(Visual.RenderTransformProperty);
        _mask.ClearValue(Visual.OpacityProperty);
    }

    private void OnCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (_start is null || !_moved)
        {
            return;
        }

        (_start, _moved) = (null, false);
        Settle();
        _surface.ClearValue(Visual.RenderTransformProperty);
        _mask.ClearValue(Visual.OpacityProperty);
    }

    private double Size => _side is SheetSide.Left or SheetSide.Right ? _surface.Bounds.Width : _surface.Bounds.Height;

    private void Settle()
    {
        _surface.ClearValue(Animatable.TransitionsProperty);
        _mask.ClearValue(Animatable.TransitionsProperty);
    }
}
