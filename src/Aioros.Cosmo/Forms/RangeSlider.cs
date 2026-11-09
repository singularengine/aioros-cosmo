using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Aioros.Cosmo.Forms;

/// <summary>
/// shadcn/ui's Slider with two thumbs (Radix Slider with two values): <see cref="LowerValue"/> and <see cref="UpperValue"/>,
/// both two-way bindable, drawn by two Avalonia <see cref="Slider"/>s that share one track, so each thumb is a slider of its
/// own on both targets (a native range input on the web). As Radix does, the values stay ordered: a press on the track moves
/// the closest thumb and drags it, a thumb moved past the other becomes the upper one and keeps the focus, the arrows step by
/// one step, Shift with an arrow and Page Up and Page Down by ten, Home sets the lower value to the minimum and End the upper
/// to the maximum. A step is <see cref="TickFrequency"/> when <see cref="IsSnapToTickEnabled"/> is set, as on Avalonia's slider.
/// </summary>
[TemplatePart("PART_LowerSlider", typeof(Slider))]
[TemplatePart("PART_UpperSlider", typeof(Slider))]
public sealed class RangeSlider : TemplatedControl
{
    /// <summary>Defines <see cref="Minimum"/>.</summary>
    public static readonly StyledProperty<double> MinimumProperty = AvaloniaProperty.Register<RangeSlider, double>(nameof(Minimum));

    /// <summary>Defines <see cref="Maximum"/>.</summary>
    public static readonly StyledProperty<double> MaximumProperty = AvaloniaProperty.Register<RangeSlider, double>(nameof(Maximum), 100);

    /// <summary>Defines <see cref="LowerValue"/>.</summary>
    public static readonly StyledProperty<double> LowerValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(LowerValue), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="UpperValue"/>.</summary>
    public static readonly StyledProperty<double> UpperValueProperty =
        AvaloniaProperty.Register<RangeSlider, double>(nameof(UpperValue), 100, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="Orientation"/>.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty = AvaloniaProperty.Register<RangeSlider, Orientation>(nameof(Orientation));

    /// <summary>Defines <see cref="TickFrequency"/>.</summary>
    public static readonly StyledProperty<double> TickFrequencyProperty = AvaloniaProperty.Register<RangeSlider, double>(nameof(TickFrequency), 1);

    /// <summary>Defines <see cref="IsSnapToTickEnabled"/>.</summary>
    public static readonly StyledProperty<bool> IsSnapToTickEnabledProperty = AvaloniaProperty.Register<RangeSlider, bool>(nameof(IsSnapToTickEnabled));

    private readonly Slider?[] _thumbs = new Slider?[2];
    private int _active = -1;

    static RangeSlider()
    {
        FocusableProperty.OverrideDefaultValue<RangeSlider>(false);
    }

    /// <summary>Creates a range slider that takes the pointer and the keyboard of its two sliders.</summary>
    public RangeSlider()
    {
        AddHandler(PointerPressedEvent, Pressed, RoutingStrategies.Tunnel);
        AddHandler(PointerMovedEvent, Moved, RoutingStrategies.Tunnel);
        AddHandler(PointerReleasedEvent, Released, RoutingStrategies.Tunnel);
        AddHandler(PointerCaptureLostEvent, (_, _) => _active = -1, RoutingStrategies.Tunnel);
        AddHandler(KeyDownEvent, Keyed, RoutingStrategies.Tunnel);
    }

    /// <summary>The smallest value.</summary>
    public double Minimum
    {
        get => GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>The largest value.</summary>
    public double Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    /// <summary>The lower thumb's value, at most <see cref="UpperValue"/> once the visitor moves a thumb.</summary>
    public double LowerValue
    {
        get => GetValue(LowerValueProperty);
        set => SetValue(LowerValueProperty, value);
    }

    /// <summary>The upper thumb's value, at least <see cref="LowerValue"/> once the visitor moves a thumb.</summary>
    public double UpperValue
    {
        get => GetValue(UpperValueProperty);
        set => SetValue(UpperValueProperty, value);
    }

    /// <summary>Whether the track lies across (the default) or up and down, the lower value at the bottom.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>The step a thumb moves by, when <see cref="IsSnapToTickEnabled"/> is set.</summary>
    public double TickFrequency
    {
        get => GetValue(TickFrequencyProperty);
        set => SetValue(TickFrequencyProperty, value);
    }

    /// <summary>Whether the values snap to multiples of <see cref="TickFrequency"/> from <see cref="Minimum"/>.</summary>
    public bool IsSnapToTickEnabled
    {
        get => GetValue(IsSnapToTickEnabledProperty);
        set => SetValue(IsSnapToTickEnabledProperty, value);
    }

    private double Step => IsSnapToTickEnabled && TickFrequency > 0 ? TickFrequency : 1;

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        _thumbs[0] = e.NameScope.Find<Slider>("PART_LowerSlider");
        _thumbs[1] = e.NameScope.Find<Slider>("PART_UpperSlider");
    }

    private void Pressed(object? sender, PointerPressedEventArgs e)
    {
        if (!IsEffectivelyEnabled || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
        {
            return;
        }

        var value = ValueAt(e.GetPosition(this));
        var lower = Math.Abs(LowerValue - value);
        var upper = Math.Abs(UpperValue - value);
        _active = Update(value, upper < lower ? 1 : 0, NavigationMethod.Pointer);
        e.Pointer.Capture(this);
        e.Handled = true;
    }

    private void Moved(object? sender, PointerEventArgs e)
    {
        if (_active >= 0)
        {
            _active = Update(ValueAt(e.GetPosition(this)), _active, NavigationMethod.Pointer);
            e.Handled = true;
        }
    }

    private void Released(object? sender, PointerReleasedEventArgs e)
    {
        if (_active >= 0)
        {
            _active = -1;
            e.Pointer.Capture(null);
            e.Handled = true;
        }
    }

    private void Keyed(object? sender, KeyEventArgs e)
    {
        var index = Array.FindIndex(_thumbs, thumb => thumb is { IsKeyboardFocusWithin: true });
        if (index < 0 || !IsEffectivelyEnabled)
        {
            return;
        }

        var skip = e.Key is Key.PageUp or Key.PageDown || e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var step = Step * (skip ? 10 : 1);
        var current = index == 0 ? LowerValue : UpperValue;
        int? moved = e.Key switch
        {
            Key.Home => Update(Minimum, 0, NavigationMethod.Tab),
            Key.End => Update(Maximum, 1, NavigationMethod.Tab),
            Key.Right or Key.Up or Key.PageUp => Update(current + step, index, NavigationMethod.Tab),
            Key.Left or Key.Down or Key.PageDown => Update(current - step, index, NavigationMethod.Tab),
            _ => null,
        };
        e.Handled = moved is not null;
    }

    private double ValueAt(Point point)
    {
        var horizontal = Orientation == Orientation.Horizontal;
        var length = horizontal ? Bounds.Width : Bounds.Height;
        var along = horizontal ? point.X : length - point.Y;
        return Minimum + (length > 0 ? Math.Clamp(along / length, 0, 1) : 0) * (Maximum - Minimum);
    }

    private int Update(double value, int index, NavigationMethod method)
    {
        var snapped = Math.Clamp(IsSnapToTickEnabled && TickFrequency > 0 ? Math.Round((value - Minimum) / TickFrequency) * TickFrequency + Minimum : value, Minimum, Maximum);
        double[] values = [LowerValue, UpperValue];
        values[index] = snapped;
        Array.Sort(values);
        var moved = Array.IndexOf(values, snapped);
        SetCurrentValue(LowerValueProperty, values[0]);
        SetCurrentValue(UpperValueProperty, values[1]);
        _thumbs[moved]?.Focus(method);
        return moved;
    }
}
