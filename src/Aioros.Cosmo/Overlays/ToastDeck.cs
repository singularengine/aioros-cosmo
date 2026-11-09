using Aioros.Markup;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media.Transformation;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Sonner's stack of toasts, the newest in front: collapsed, the two before it peek 14 px behind it, each 5% smaller,
/// their content hidden, the others hidden; expanded (under the pointer, or always with <see cref="IsExpanded"/>), the
/// toasts stand 14 px apart. Anchored at its bottom, or at its top when aligned to the top. A toast dragged toward its
/// corner's sides (down and right at the bottom right) follows the pointer, and the other way along the same axis
/// dampened as sonner's; let go past 45 px or faster than 0.11 px/ms it leaves the way it went over 200 ms and closes,
/// otherwise it moves back. Written for the web, the deck declares the same stack and swipes (<c>a:Web.Deck</c>,
/// <c>a:Web.Swipe</c>), which the browser keeps.
/// </summary>
public sealed class ToastDeck : Panel
{
    /// <summary>Defines <see cref="IsExpanded"/>.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<ToastDeck, bool>(nameof(IsExpanded));

    private const double Gap = 14;

    private const int Visible = 3;

    private const double Step = 0.05;

    private const double Threshold = 45;

    private const double Speed = 0.11;

    private readonly Dictionary<Control, Point> _swiped = [];

    private Swipe? _swipe;

    static ToastDeck()
    {
        AffectsArrange<ToastDeck>(IsExpandedProperty, IsPointerOverProperty);
        AffectsMeasure<ToastDeck>(IsExpandedProperty, IsPointerOverProperty);
        NotificationCard.IsClosingProperty.Changed.AddClassHandler<NotificationCard>((card, _) => card.FindAncestorOfType<ToastDeck>()?.InvalidateMeasure());
        IsExpandedProperty.Changed.AddClassHandler<ToastDeck>((deck, change) => Web.SetDeckExpanded(deck, change.GetNewValue<bool>()));
        HorizontalAlignmentProperty.Changed.AddClassHandler<ToastDeck>((deck, _) => deck.Directions());
        VerticalAlignmentProperty.Changed.AddClassHandler<ToastDeck>((deck, _) => deck.Directions());
    }

    /// <summary>Creates a deck, declaring its stack for the web.</summary>
    public ToastDeck()
    {
        Web.SetDeck(this, Gap);
        Web.SetDeckVisible(this, Visible);
        Web.SetDeckScale(this, Step);
        Web.SetDeckBehind(this, "behind");
    }

    /// <summary>Whether the toasts always stand apart (sonner's <c>expand</c>).</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    /// <summary>The clock a swipe's speed is read by (the system's by default, the <see cref="Toaster"/>'s when it shows the deck).</summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    private bool Open => IsExpanded || IsPointerOver;

    private List<Control> Newest() => [.. Children.Where(child => child is not NotificationCard { IsClosing: true }).Reverse()];

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        var toasts = Newest();
        foreach (var child in Children)
        {
            child.Measure(availableSize);
        }

        if (toasts.Count == 0)
        {
            return default;
        }

        var width = toasts.Max(toast => toast.DesiredSize.Width);
        var height = Open ? toasts.Take(Visible).Sum(toast => toast.DesiredSize.Height) + (Math.Min(Visible, toasts.Count) - 1) * Gap : toasts[0].DesiredSize.Height;
        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        var toasts = Newest();
        var top = VerticalAlignment == VerticalAlignment.Top;
        var front = toasts.Count > 0 ? toasts[0].DesiredSize.Height : 0;
        var offset = 0.0;
        for (var index = 0; index < toasts.Count; index++)
        {
            var toast = toasts[index];
            var height = Open ? toast.DesiredSize.Height : front;
            var y = top ? offset : finalSize.Height - offset - height;
            toast.Arrange(new Rect(0, y, finalSize.Width, height));
            Place(toast, index, top);
            offset += Open ? height + Gap : 0;
        }

        return finalSize;
    }

    /// <inheritdoc />
    protected override void ChildrenChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        foreach (var toast in e.NewItems?.OfType<Control>() ?? [])
        {
            toast.Classes.Set("top", VerticalAlignment == VerticalAlignment.Top);
            Web.SetSwipe(toast, Swipes());
            toast.AddHandler(PointerPressedEvent, OnPressed, RoutingStrategies.Tunnel);
        }

        foreach (var toast in e.OldItems?.OfType<Control>() ?? [])
        {
            toast.RemoveHandler(PointerPressedEvent, OnPressed);
            _swiped.Remove(toast);
        }

        base.ChildrenChanged(sender, e);
    }

    /// <summary>
    /// A press on a toast outside its buttons begins a swipe, and is handled there, so the notification manager does not
    /// close the toast on a press (sonner's toasts close by their buttons, a swipe or their time).
    /// </summary>
    private void OnPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not Control card || card is NotificationCard { IsClosing: true } || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed
            || (e.Source as Visual)?.GetSelfAndVisualAncestors().TakeWhile(visual => !ReferenceEquals(visual, card)).Any(visual => visual is Button) != false)
        {
            return;
        }

        e.Handled = true;
        _swipe = new Swipe(card, e.GetPosition(this), TimeProvider.GetTimestamp());
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (_swipe is not { } swipe)
        {
            return;
        }

        var delta = e.GetPosition(this) - swipe.Start;
        if (swipe.Axis is null && (Math.Abs(delta.X) > 1 || Math.Abs(delta.Y) > 1))
        {
            swipe.Axis = Math.Abs(delta.X) > Math.Abs(delta.Y) ? Orientation.Horizontal : Orientation.Vertical;
            e.Pointer.Capture(this);
            swipe.Card.SetValue(Animatable.TransitionsProperty, null);
        }

        if (swipe.Axis is not { } axis)
        {
            return;
        }

        var amount = Dampened(axis == Orientation.Horizontal ? delta.X : delta.Y, axis);
        _swiped[swipe.Card] = axis == Orientation.Horizontal ? new Point(amount, 0) : new Point(0, amount);
        InvalidateArrange();
    }

    /// <inheritdoc />
    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (_swipe is not { Axis: { } axis } swipe)
        {
            _swipe = null;
            return;
        }

        _swipe = null;
        e.Pointer.Capture(null);
        var offset = _swiped.GetValueOrDefault(swipe.Card);
        var amount = axis == Orientation.Horizontal ? offset.X : offset.Y;
        var elapsed = Math.Max(TimeProvider.GetElapsedTime(swipe.Started).TotalMilliseconds, 1);
        swipe.Card.ClearValue(Animatable.TransitionsProperty);
        if (amount != 0 && (Math.Abs(amount) >= Threshold || Math.Abs(amount) / elapsed > Speed))
        {
            Leave(swipe.Card, offset, axis == Orientation.Horizontal ? new Vector(Math.Sign(amount), 0) : new Vector(0, Math.Sign(amount)));
            return;
        }

        _swiped.Remove(swipe.Card);
        InvalidateArrange();
    }

    /// <inheritdoc />
    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (_swipe is { } swipe)
        {
            _swipe = null;
            swipe.Card.ClearValue(Animatable.TransitionsProperty);
            _swiped.Remove(swipe.Card);
            InvalidateArrange();
        }
    }

    /// <summary>Lets a swiped toast go the way it went: it moves its own size further and fades over its swiped transitions (200 ms, <c>ease-out</c>), then closes.</summary>
    private void Leave(Control card, Point offset, Vector direction)
    {
        _swiped.Remove(card);
        card.Classes.Add("swiped");
        var transform = TransformOperations.CreateBuilder(2);
        transform.AppendScale(1, 1);
        transform.AppendTranslate(offset.X + (direction.X * card.Bounds.Width), offset.Y + (direction.Y * card.Bounds.Height));
        card.RenderTransform = transform.Build();
        card.Opacity = 0;
        (card as NotificationCard)?.Close();
    }

    /// <summary>A swipe's amount along its axis: whole toward the deck's sides, the other way as sonner's <c>getDampening</c>, none on an axis without a side.</summary>
    private double Dampened(double delta, Orientation axis)
    {
        var toward = axis == Orientation.Vertical
            ? VerticalAlignment == VerticalAlignment.Top ? -1 : 1
            : HorizontalAlignment switch { HorizontalAlignment.Right => 1, HorizontalAlignment.Left => -1, _ => 0 };
        if (toward == 0 || Math.Sign(delta) == toward)
        {
            return toward == 0 ? 0 : delta;
        }

        var dampened = delta / (1.5 + (Math.Abs(delta) / 20));
        return Math.Abs(dampened) < Math.Abs(delta) ? dampened : delta;
    }

    /// <summary>The directions a toast is swiped away in, declared for the web: its corner's sides.</summary>
    private string Swipes()
    {
        var vertical = VerticalAlignment == VerticalAlignment.Top ? "Up" : "Down";
        return HorizontalAlignment switch
        {
            HorizontalAlignment.Right => vertical + ",Right",
            HorizontalAlignment.Left => vertical + ",Left",
            _ => vertical,
        };
    }

    private void Directions()
    {
        foreach (var toast in Children)
        {
            Web.SetSwipe(toast, Swipes());
        }
    }

    /// <summary>
    /// Lifts and shrinks a toast behind the front one while the deck is collapsed, and hides those past the third; the
    /// toast's transitions carry it there as sonner's 400 ms do. Written for the web, the toasts stay at rest, since the
    /// browser lays the stack out itself.
    /// </summary>
    private void Place(Control toast, int index, bool top)
    {
        if (Web.GetIsWritten(this))
        {
            toast.ClearValue(RenderTransformProperty);
            toast.ClearValue(OpacityProperty);
            return;
        }

        var behind = !Open && index > 0;
        var scale = behind ? 1 - (Step * index) : 1;
        var lift = behind ? (top ? 1 : -1) * Gap * index : 0;
        var swiped = _swiped.GetValueOrDefault(toast);
        var transform = TransformOperations.CreateBuilder(2);
        transform.AppendScale(scale, scale);
        transform.AppendTranslate(swiped.X, lift + swiped.Y);
        toast.RenderTransformOrigin = RelativePoint.Center;
        toast.RenderTransform = transform.Build();
        toast.Classes.Set("behind", behind);
        toast.Opacity = index < Visible ? 1 : 0;
    }

    /// <summary>A press on a toast: where and when it began, and the axis it locked to once it moved.</summary>
    private sealed class Swipe(Control card, Point start, long started)
    {
        public Control Card { get; } = card;

        public Point Start { get; } = start;

        public long Started { get; } = started;

        public Orientation? Axis { get; set; }
    }
}
