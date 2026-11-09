using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Threading;

namespace Aioros.Cosmo.Forms;

/// <summary>
/// shadcn/ui's SelectScrollUpButton: shown above a select's list while the list can scroll up; while the pointer is on it
/// (or presses it) the list scrolls up by an item's height every 50 ms, as Radix Select's scroll buttons do.
/// </summary>
public sealed class SelectScrollUpButton : TemplatedControl
{
    /// <summary>Defines <see cref="Target"/>.</summary>
    public static readonly StyledProperty<ScrollViewer?> TargetProperty = AvaloniaProperty.Register<SelectScrollUpButton, ScrollViewer?>(nameof(Target));

    private readonly ListScroller _scroller;

    /// <summary>Creates a button hidden until its list can scroll up.</summary>
    public SelectScrollUpButton()
    {
        _scroller = new ListScroller(this, TargetProperty, -1);
    }

    /// <summary>The list's scroll viewer.</summary>
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>The clock the scrolling follows (the system's by default).</summary>
    public TimeProvider Time
    {
        get => _scroller.Time;
        set => _scroller.Time = value;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        _scroller?.Changed(change);
    }
}

/// <summary>
/// shadcn/ui's SelectScrollDownButton: shown below a select's list while the list can scroll down; while the pointer is on
/// it (or presses it) the list scrolls down by an item's height every 50 ms, as Radix Select's scroll buttons do.
/// </summary>
public sealed class SelectScrollDownButton : TemplatedControl
{
    /// <summary>Defines <see cref="Target"/>.</summary>
    public static readonly StyledProperty<ScrollViewer?> TargetProperty = AvaloniaProperty.Register<SelectScrollDownButton, ScrollViewer?>(nameof(Target));

    private readonly ListScroller _scroller;

    /// <summary>Creates a button hidden until its list can scroll down.</summary>
    public SelectScrollDownButton()
    {
        _scroller = new ListScroller(this, TargetProperty, 1);
    }

    /// <summary>The list's scroll viewer.</summary>
    public ScrollViewer? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>The clock the scrolling follows (the system's by default).</summary>
    public TimeProvider Time
    {
        get => _scroller.Time;
        set => _scroller.Time = value;
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        _scroller?.Changed(change);
    }
}

/// <summary>What a select's scroll button does: shows itself while its list can scroll its way (bringing the active item back into view as it appears, as Radix does) and scrolls it by the selected (or first enabled) item's height every 50 ms while pointed at or pressed.</summary>
internal sealed class ListScroller
{
    private static readonly TimeSpan Interval = TimeSpan.FromMilliseconds(50);

    private readonly TemplatedControl _button;
    private readonly AvaloniaProperty _targetProperty;
    private readonly int _direction;
    private ScrollViewer? _target;
    private ITimer? _timer;

    public ListScroller(TemplatedControl button, AvaloniaProperty targetProperty, int direction)
    {
        _button = button;
        _targetProperty = targetProperty;
        _direction = direction;
        _button.IsVisible = false;
        _button.PointerEntered += (_, _) => Start();
        _button.PointerMoved += (_, _) => Start();
        _button.PointerPressed += (_, _) => Start();
        _button.PointerExited += (_, _) => Stop();
        _button.DetachedFromVisualTree += (_, _) => Stop();
    }

    public TimeProvider Time { get; set; } = TimeProvider.System;

    public void Changed(AvaloniaPropertyChangedEventArgs change)
    {
        if (change.Property == _targetProperty)
        {
            if (_target is not null)
            {
                _target.ScrollChanged -= Scrolled;
            }

            _target = change.NewValue as ScrollViewer;
            if (_target is not null)
            {
                _target.ScrollChanged += Scrolled;
            }

            Show();
        }
        else if (change.Property == Visual.IsVisibleProperty && !_button.IsVisible)
        {
            Stop();
        }
    }

    private void Scrolled(object? sender, ScrollChangedEventArgs e) => Show();

    private bool CanScroll => _target is { } target && (_direction < 0
        ? target.Offset.Y > 0
        : Math.Ceiling(target.Offset.Y) < target.Extent.Height - target.Viewport.Height);

    private void Show()
    {
        var shown = CanScroll;
        if (shown && !_button.IsVisible && _button.TemplatedParent is ComboBox combo)
        {
            Dispatcher.UIThread.Post(() => Active(combo)?.BringIntoView(), DispatcherPriority.Loaded);
        }

        _button.IsVisible = shown;
    }

    private static Control? Active(ComboBox combo) =>
        TopLevel.GetTopLevel(combo)?.FocusManager?.GetFocusedElement() is ComboBoxItem { } focused && ItemsControl.ItemsControlFromItemContainer(focused) == combo
            ? focused
            : combo.ContainerFromIndex(combo.SelectedIndex);

    private void Start()
    {
        if (_timer is null && CanScroll)
        {
            _timer = Time.CreateTimer(_ => Dispatcher.UIThread.Post(Step), null, Interval, Interval);
        }
    }

    private void Stop()
    {
        _timer?.Dispose();
        _timer = null;
    }

    private void Step()
    {
        if (_target is not { } target || !CanScroll)
        {
            Stop();
            return;
        }

        var height = _button.TemplatedParent is ComboBox combo ? ItemHeight(combo) : 0;
        target.Offset = target.Offset.WithY(Math.Clamp(target.Offset.Y + _direction * height, 0, Math.Max(0, target.Extent.Height - target.Viewport.Height)));
    }

    private static double ItemHeight(ComboBox combo)
    {
        var item = combo.ContainerFromIndex(combo.SelectedIndex) ?? combo.GetRealizedContainers().FirstOrDefault(container => container.IsEnabled);
        return item?.Bounds.Height ?? 0;
    }
}
