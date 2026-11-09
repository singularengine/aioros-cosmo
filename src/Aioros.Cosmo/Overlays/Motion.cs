using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media.Transformation;
using Avalonia.Threading;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// The entrance of an overlay's surface, as tw-animate-css's <c>animate-in</c> plays shadcn/ui's <c>fade-in-0</c>,
/// <c>zoom-in-95</c> and <c>slide-in-from-*</c>: when the surface is shown it starts from <see cref="EnterOpacityProperty"/>,
/// <see cref="EnterScaleProperty"/> and <see cref="EnterOffsetProperty"/> (pixels, or a share of its own size as
/// <c>slide-in-from-right</c>'s 100%) and its own <c>Transitions</c> carry it to rest, with the length and easing the theme
/// gives them. Aioros writes the same transitions on the web, the starting look as the surface's <c>@starting-style</c>.
/// Nothing moves while <see cref="IsReducedProperty"/> is set on the surface or a parent (the window, for an app whose
/// visitor asks for less motion). Its exit (<c>animate-out</c>) is the look the <c>Exit</c> properties give it while it or
/// the control whose template holds it is <c>:closing</c>, which Aioros reads as the overlay's look while closed.
/// </summary>
public static partial class Motion
{
    /// <summary>The opacity the surface enters from (<c>fade-in-0</c>: 0); NaN, the default, for none.</summary>
    public static readonly AttachedProperty<double> EnterOpacityProperty = AvaloniaProperty.RegisterAttached<Control, double>("EnterOpacity", typeof(Motion), double.NaN);

    /// <summary>The scale the surface enters from (<c>zoom-in-95</c>: 0.95); NaN, the default, for none.</summary>
    public static readonly AttachedProperty<double> EnterScaleProperty = AvaloniaProperty.RegisterAttached<Control, double>("EnterScale", typeof(Motion), double.NaN);

    /// <summary>The offset the surface enters from (<c>slide-in-from-top-2</c>: <c>0,-8</c>; <c>slide-in-from-right</c>: <c>100%,0%</c>); none by default.</summary>
    public static readonly AttachedProperty<RelativePoint> EnterOffsetProperty = AvaloniaProperty.RegisterAttached<Control, RelativePoint>("EnterOffset", typeof(Motion), default);

    /// <summary>Whether surfaces inside this element enter with no motion; inherited, so a window sets it for all.</summary>
    public static readonly AttachedProperty<bool> IsReducedProperty = AvaloniaProperty.RegisterAttached<Control, bool>("IsReduced", typeof(Motion), inherits: true);

    private static readonly AttachedProperty<bool> WatchedProperty = AvaloniaProperty.RegisterAttached<Control, bool>("Watched", typeof(Motion));

    static Motion()
    {
        EnterOpacityProperty.Changed.AddClassHandler<Control>((control, _) => Watch(control));
        EnterScaleProperty.Changed.AddClassHandler<Control>((control, _) => Watch(control));
        EnterOffsetProperty.Changed.AddClassHandler<Control>((control, _) => Watch(control));
        ExitOpacityProperty.Changed.AddClassHandler<Control>((control, _) => WatchExit(control));
        ExitScaleProperty.Changed.AddClassHandler<Control>((control, _) => WatchExit(control));
        ExitOffsetProperty.Changed.AddClassHandler<Control>((control, _) => WatchExit(control));
    }

    /// <summary>Gets <see cref="EnterOpacityProperty"/>.</summary>
    public static double GetEnterOpacity(Control control) => control.GetValue(EnterOpacityProperty);

    /// <summary>Sets <see cref="EnterOpacityProperty"/>.</summary>
    public static void SetEnterOpacity(Control control, double value) => control.SetValue(EnterOpacityProperty, value);

    /// <summary>Gets <see cref="EnterScaleProperty"/>.</summary>
    public static double GetEnterScale(Control control) => control.GetValue(EnterScaleProperty);

    /// <summary>Sets <see cref="EnterScaleProperty"/>.</summary>
    public static void SetEnterScale(Control control, double value) => control.SetValue(EnterScaleProperty, value);

    /// <summary>Gets <see cref="EnterOffsetProperty"/>.</summary>
    public static RelativePoint GetEnterOffset(Control control) => control.GetValue(EnterOffsetProperty);

    /// <summary>Sets <see cref="EnterOffsetProperty"/>.</summary>
    public static void SetEnterOffset(Control control, RelativePoint value) => control.SetValue(EnterOffsetProperty, value);

    /// <summary>Gets <see cref="IsReducedProperty"/>.</summary>
    public static bool GetIsReduced(Control control) => control.GetValue(IsReducedProperty);

    /// <summary>Sets <see cref="IsReducedProperty"/>.</summary>
    public static void SetIsReduced(Control control, bool value) => control.SetValue(IsReducedProperty, value);

    private static void Watch(Control control)
    {
        if (control.GetValue(WatchedProperty))
        {
            return;
        }

        control.SetValue(WatchedProperty, true);
        control.AttachedToVisualTree += (_, _) => Enter(control);
    }

    internal static void Enter(Control control)
    {
        if (control.GetValue(IsReducedProperty) || !Moves(control))
        {
            return;
        }

        if (GetEnterOffset(control) is { Unit: RelativeUnit.Relative } offset && offset != default && control.Bounds.Size == default)
        {
            Place(control, offset: false);
            control.SizeChanged += Sized;
            return;
        }

        Place(control, offset: true);
        Dispatcher.UIThread.Post(() => Rest(control));
    }

    private static void Sized(object? sender, SizeChangedEventArgs e)
    {
        var control = (Control)sender!;
        var offset = GetEnterOffset(control).Point;
        if ((offset.X != 0 && control.Bounds.Width == 0) || (offset.Y != 0 && control.Bounds.Height == 0))
        {
            return;
        }

        control.SizeChanged -= Sized;
        Place(control, offset: true);
        Dispatcher.UIThread.Post(() => Rest(control));
    }

    private static bool Moves(Control control) =>
        !double.IsNaN(GetEnterOpacity(control)) || !double.IsNaN(GetEnterScale(control)) || GetEnterOffset(control) != default;

    /// <summary>Puts the surface at its starting look, its transitions held so it lands there at once.</summary>
    private static void Place(Control control, bool offset)
    {
        using var held = control.SetValue(Animatable.TransitionsProperty, null, BindingPriority.Animation);
        if (!double.IsNaN(GetEnterOpacity(control)))
        {
            control.Opacity = GetEnterOpacity(control);
        }

        control.RenderTransform = Transform(control, offset ? Offset(control) : default, double.IsNaN(GetEnterScale(control)) ? 1 : GetEnterScale(control));
    }

    /// <summary>Gives the surface its resting look, its theme's, which its transitions carry it to and its exit takes it from.</summary>
    private static void Rest(Control control)
    {
        if (!double.IsNaN(GetEnterOpacity(control)))
        {
            control.ClearValue(Visual.OpacityProperty);
        }

        control.ClearValue(Visual.RenderTransformProperty);
    }

    private static Point Offset(Control control) => Pixels(control, GetEnterOffset(control));

    private static Point Pixels(Control control, RelativePoint offset) =>
        offset.Unit == RelativeUnit.Relative ? new Point(offset.Point.X * control.Bounds.Width, offset.Point.Y * control.Bounds.Height) : offset.Point;

    private static TransformOperations Transform(Control control, Point offset, double scale) =>
        Transform(double.IsNaN(GetEnterScale(control)) ? double.NaN : scale, offset);

    /// <summary>A scale (none for NaN) and then a move, as tw-animate-css's <c>translate3d(...) scale3d(...)</c> draws them.</summary>
    private static TransformOperations Transform(double scale, Point offset)
    {
        var builder = TransformOperations.CreateBuilder(2);
        if (!double.IsNaN(scale))
        {
            builder.AppendScale(scale, scale);
        }

        builder.AppendTranslate(offset.X, offset.Y);
        return builder.Build();
    }
}
