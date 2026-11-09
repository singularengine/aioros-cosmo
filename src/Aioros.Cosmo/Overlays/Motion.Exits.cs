using Aioros.Markup;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// The exit of an overlay's surface: the look it leaves for while it, or the control whose template holds it, is
/// <c>:closing</c> (a closing <see cref="NotificationCard"/> counts as <c>:closing</c>). On the desktop a popup closes at
/// once, so the surface of a closed popup leaves as a picture of it, in its window's overlay layer where it stood, which
/// passes every click through and is gone once its exit has played or the popup opens again, as Radix's <c>Presence</c>
/// keeps a closed element only for its exit; the overlays Cosmo adds itself (dialogs, sheets, drawers) and its toasts stay,
/// passing clicks through, while their exit plays.
/// </summary>
public static partial class Motion
{
    /// <summary>The opacity the surface leaves for (<c>fade-out-0</c>: 0); NaN, the default, for none.</summary>
    public static readonly AttachedProperty<double> ExitOpacityProperty = AvaloniaProperty.RegisterAttached<Control, double>("ExitOpacity", typeof(Motion), double.NaN);

    /// <summary>The scale the surface leaves for (<c>zoom-out-95</c>: 0.95); NaN, the default, for none.</summary>
    public static readonly AttachedProperty<double> ExitScaleProperty = AvaloniaProperty.RegisterAttached<Control, double>("ExitScale", typeof(Motion), double.NaN);

    /// <summary>The offset the surface leaves for (sonner's fall: <c>0%,100%</c>); none by default.</summary>
    public static readonly AttachedProperty<RelativePoint> ExitOffsetProperty = AvaloniaProperty.RegisterAttached<Control, RelativePoint>("ExitOffset", typeof(Motion), default);

    private static readonly AttachedProperty<bool> LeavesProperty = AvaloniaProperty.RegisterAttached<Control, bool>("Leaves", typeof(Motion));

    private static readonly AttachedProperty<bool> LeavingProperty = AvaloniaProperty.RegisterAttached<Control, bool>("Leaving", typeof(Motion));

    private static readonly AttachedProperty<bool> OwnerWatchedProperty = AvaloniaProperty.RegisterAttached<Control, bool>("OwnerWatched", typeof(Motion));

    private static readonly AttachedProperty<Spot?> SpotProperty = AvaloniaProperty.RegisterAttached<Control, Spot?>("Spot", typeof(Motion));

    private static readonly AttachedProperty<Image?> GhostProperty = AvaloniaProperty.RegisterAttached<Control, Image?>("Ghost", typeof(Motion));

    private static readonly AttachedProperty<double> ProgressProperty = AvaloniaProperty.RegisterAttached<Control, double>("Progress", typeof(Motion));

    /// <summary>Gets <see cref="ExitOpacityProperty"/>.</summary>
    public static double GetExitOpacity(Control control) => control.GetValue(ExitOpacityProperty);

    /// <summary>Sets <see cref="ExitOpacityProperty"/>.</summary>
    public static void SetExitOpacity(Control control, double value) => control.SetValue(ExitOpacityProperty, value);

    /// <summary>Gets <see cref="ExitScaleProperty"/>.</summary>
    public static double GetExitScale(Control control) => control.GetValue(ExitScaleProperty);

    /// <summary>Sets <see cref="ExitScaleProperty"/>.</summary>
    public static void SetExitScale(Control control, double value) => control.SetValue(ExitScaleProperty, value);

    /// <summary>Gets <see cref="ExitOffsetProperty"/>.</summary>
    public static RelativePoint GetExitOffset(Control control) => control.GetValue(ExitOffsetProperty);

    /// <summary>Sets <see cref="ExitOffsetProperty"/>.</summary>
    public static void SetExitOffset(Control control, RelativePoint value) => control.SetValue(ExitOffsetProperty, value);

    /// <summary>
    /// Plays the exits of parts Cosmo keeps in the tree while they leave (a dialog's surface and mask): they turn
    /// <c>:closing</c>, so they take their exit looks by their transitions, and the task completes once the longest of
    /// those transitions has run on the parts' animation clock; at once when nothing leaves, when motion is reduced, or
    /// when Aioros writes the tree for the web (<see cref="Web.IsWrittenProperty"/>), where the exit is the browser's.
    /// </summary>
    internal static Task ExitAsync(params Control[] parts)
    {
        var moving = parts.Where(Leaves).ToList();
        if (moving.Count == 0)
        {
            return Task.CompletedTask;
        }

        foreach (var part in moving)
        {
            ((IPseudoClasses)part.Classes).Add(":closing");
        }

        return PlayedAsync(moving[0], moving.Select(Longest).Max());
    }

    /// <summary>Whether a part would play an exit: it has one, it is shown on a screen, and motion is not reduced.</summary>
    internal static bool Leaves(Control part) =>
        Exits(part) && part.IsAttachedToVisualTree() && !part.GetValue(IsReducedProperty) && !Web.GetIsWritten(part) && Longest(part) > TimeSpan.Zero;

    private static bool Exits(Control control) =>
        !double.IsNaN(GetExitOpacity(control)) || !double.IsNaN(GetExitScale(control)) || GetExitOffset(control) != default;

    private static TimeSpan Longest(Control control) =>
        (control.Transitions ?? []).OfType<TransitionBase>()
            .Where(transition => transition.Property == Visual.OpacityProperty || transition.Property == Visual.RenderTransformProperty)
            .Select(transition => transition.Delay + transition.Duration).DefaultIfEmpty().Max();

    /// <summary>Completes once <paramref name="length"/> has run on the control's animation clock (the window's).</summary>
    private static async Task PlayedAsync(Animatable control, TimeSpan length)
    {
        var timer = new Avalonia.Animation.Animation
        {
            Duration = length,
            Children =
            {
                new KeyFrame { Cue = new Cue(0), Setters = { new Setter(ProgressProperty, 0d) } },
                new KeyFrame { Cue = new Cue(1), Setters = { new Setter(ProgressProperty, 1d) } },
            },
        };
        await timer.RunAsync(control);
    }

    private static void WatchExit(Control control)
    {
        if (control.GetValue(LeavesProperty))
        {
            return;
        }

        control.SetValue(LeavesProperty, true);
        control.Classes.CollectionChanged += (_, _) => Leave(control);
        control.AttachedToVisualTree += (_, _) =>
        {
            if (control.TemplatedParent is StyledElement owner && !control.GetValue(OwnerWatchedProperty))
            {
                control.SetValue(OwnerWatchedProperty, true);
                owner.Classes.CollectionChanged += (_, _) => Leave(control);
                owner.PropertyChanged += (_, change) =>
                {
                    if (change.Property == NotificationCard.IsClosingProperty)
                    {
                        Leave(control);
                    }
                };
            }

            Shown(control);
        };
        control.DetachedFromVisualTree += (_, _) => Hidden(control);
    }

    /// <summary>Gives the surface its exit look while it or its template's owner is <c>:closing</c>, and its resting look back after.</summary>
    private static void Leave(Control control)
    {
        var owner = control.TemplatedParent as StyledElement;
        var closing = control.Classes.Contains(":closing") || owner?.Classes.Contains(":closing") == true || owner is NotificationCard { IsClosing: true };
        if (!closing && !control.GetValue(LeavingProperty))
        {
            return;
        }

        control.SetValue(LeavingProperty, closing);
        if (!closing)
        {
            control.ClearValue(Visual.OpacityProperty);
            control.ClearValue(Visual.RenderTransformProperty);
            return;
        }

        if (!double.IsNaN(GetExitOpacity(control)))
        {
            control.Opacity = GetExitOpacity(control);
        }

        control.RenderTransform = Transform(GetExitScale(control), Pixels(control, GetExitOffset(control)));
    }

    /// <summary>A popup's surface shown: a picture of it still leaving goes, and where it stands in its window is followed.</summary>
    private static void Shown(Control control)
    {
        if (control.GetValue(GhostProperty) is { } ghost)
        {
            Bury(control, ghost);
        }

        if (Exits(control) && control.GetVisualAncestors().Any(ancestor => ancestor is PopupRoot or OverlayPopupHost))
        {
            control.LayoutUpdated += Track;
            Track(control, EventArgs.Empty);
        }
    }

    private static void Track(object? sender, EventArgs e)
    {
        var control = (Control)sender!;
        var top = TopLevel.GetTopLevel(control);
        var window = (top as PopupRoot)?.ParentTopLevel ?? top;
        if (window is null || control.Bounds.Size == default)
        {
            return;
        }

        var parent = control.GetVisualParent() ?? control;
        var origin = ReferenceEquals(parent, control) ? default : control.Bounds.Position;
        var at = ReferenceEquals(window, top) ? parent.TranslatePoint(origin, window) : window.PointToClient(parent.PointToScreen(origin));
        if (at is { } point)
        {
            control.SetValue(SpotProperty, new Spot(window, point, control.Bounds.Size, window.RenderScaling, control.GetValue(IsReducedProperty) || window.GetValue(IsReducedProperty) || Web.GetIsWritten(window)));
        }
    }

    /// <summary>A popup's surface closed: it leaves as a picture of it, unless motion is reduced or the tree is written for the web.</summary>
    private static void Hidden(Control control)
    {
        control.LayoutUpdated -= Track;
        var spot = control.GetValue(SpotProperty);
        control.ClearValue(SpotProperty);
        if (spot is not { Reduced: false } || !spot.Window.IsVisible || OverlayLayer.GetOverlayLayer(spot.Window) is not { } layer || Longest(control) is not { Ticks: > 0 } length)
        {
            return;
        }

        var pixels = new PixelSize(Math.Max(1, (int)Math.Ceiling(spot.Size.Width * spot.Scaling)), Math.Max(1, (int)Math.Ceiling(spot.Size.Height * spot.Scaling)));
        var picture = new RenderTargetBitmap(pixels, new Vector(96 * spot.Scaling, 96 * spot.Scaling));
        picture.Render(control);
        var ghost = new Image
        {
            Source = picture,
            Width = spot.Size.Width,
            Height = spot.Size.Height,
            Stretch = Stretch.Fill,
            IsHitTestVisible = false,
            RenderTransformOrigin = control.RenderTransformOrigin,
            Transitions = control.Transitions,
        };
        var at = spot.Window.TranslatePoint(spot.At, layer) ?? spot.At;
        Canvas.SetLeft(ghost, at.X);
        Canvas.SetTop(ghost, at.Y);
        layer.Children.Add(ghost);
        control.SetValue(GhostProperty, ghost);
        if (!double.IsNaN(GetExitOpacity(control)))
        {
            ghost.Opacity = GetExitOpacity(control);
        }

        ghost.RenderTransform = Transform(GetExitScale(control), Pixels(control, GetExitOffset(control)));
        _ = PlayedAsync(ghost, length).ContinueWith(_ => Bury(control, ghost), TaskScheduler.FromCurrentSynchronizationContext());
    }

    private static void Bury(Control control, Image ghost)
    {
        if (ReferenceEquals(control.GetValue(GhostProperty), ghost))
        {
            control.ClearValue(GhostProperty);
        }

        (ghost.Parent as Panel)?.Children.Remove(ghost);
        (ghost.Source as IDisposable)?.Dispose();
        ghost.Source = null;
    }

    /// <summary>Where a popup's surface stood in its window, at what size and scaling, and whether its exit should not move.</summary>
    private sealed record Spot(TopLevel Window, Point At, Size Size, double Scaling, bool Reduced);
}
