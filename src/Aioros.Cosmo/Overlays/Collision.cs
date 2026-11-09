using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// The distance a popup's border keeps from its window's left and right edges, as Radix Popper's <c>collisionPadding</c>
/// (Radix Select's 10 px). Avalonia's positioner keeps none, so once the popup is open its border is shifted along its
/// anchor, through its horizontal offset, as little as that takes, as each layout places it while it is open, and the
/// shift is undone when it closes; on the web
/// <c>a:Web.CollisionPadding</c> carries the same distance.
/// </summary>
public static class Collision
{
    /// <summary>The distance kept from the window's edges; 0, the default, for none.</summary>
    public static readonly AttachedProperty<double> PaddingProperty = AvaloniaProperty.RegisterAttached<Popup, double>("Padding", typeof(Collision));

    private static readonly AttachedProperty<bool> WatchedProperty = AvaloniaProperty.RegisterAttached<Popup, bool>("Watched", typeof(Collision));

    private static readonly AttachedProperty<double> ShiftProperty = AvaloniaProperty.RegisterAttached<Popup, double>("Shift", typeof(Collision));

    static Collision() => PaddingProperty.Changed.AddClassHandler<Popup>(Watch);

    /// <summary>Gets <see cref="PaddingProperty"/>.</summary>
    public static double GetPadding(Popup popup) => popup.GetValue(PaddingProperty);

    /// <summary>Sets <see cref="PaddingProperty"/>.</summary>
    public static void SetPadding(Popup popup, double value) => popup.SetValue(PaddingProperty, value);

    private static void Watch(Popup popup, AvaloniaPropertyChangedEventArgs change)
    {
        Web.SetCollisionPadding(popup, change.GetNewValue<double>());
        if (popup.GetValue(WatchedProperty))
        {
            return;
        }

        popup.SetValue(WatchedProperty, true);

        void Placed(object? sender, EffectiveViewportChangedEventArgs e) => Keep(popup);
        popup.Opened += (_, _) => popup.Child?.EffectiveViewportChanged += Placed;
        popup.Closed += (_, _) =>
        {
            popup.Child?.EffectiveViewportChanged -= Placed;
            Release(popup);
        };
    }

    private static void Keep(Popup popup)
    {
        var padding = GetPadding(popup);
        if (padding <= 0 || popup.Child is not { } child || TopLevel.GetTopLevel(popup) is not { } top)
        {
            return;
        }

        var left = Left(child, top);
        var right = left + child.Bounds.Width;
        var shift = left < padding ? padding - left : Math.Max(Math.Min(top.ClientSize.Width - padding - right, 0), padding - left);
        if (Math.Abs(shift) > 0.01)
        {
            popup.SetValue(ShiftProperty, popup.GetValue(ShiftProperty) + shift);
            popup.SetCurrentValue(Popup.HorizontalOffsetProperty, popup.HorizontalOffset + shift);
        }
    }

    private static double Left(Visual child, TopLevel top)
    {
        var left = 0d;
        var own = TopLevel.GetTopLevel(child);
        for (var visual = (Visual?)child; visual is not null && !ReferenceEquals(visual, own); visual = visual.GetVisualParent())
        {
            left += visual.Bounds.X;
        }

        return own is null || ReferenceEquals(own, top) ? left : left + ((own.PointToScreen(default).X - top.PointToScreen(default).X) / top.RenderScaling);
    }

    private static void Release(Popup popup)
    {
        var shift = popup.GetValue(ShiftProperty);
        if (shift != 0)
        {
            popup.ClearValue(ShiftProperty);
            popup.SetCurrentValue(Popup.HorizontalOffsetProperty, popup.HorizontalOffset - shift);
        }
    }
}
