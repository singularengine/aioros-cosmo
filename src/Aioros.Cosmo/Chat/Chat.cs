using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Chat;

/// <summary>
/// shadcn/ui's Marker: a row of muted 14 px text with an optional 16 px <see cref="Icon"/>; the <c>separator</c> class draws
/// a line on each side of its content, <c>border</c> a bottom border, <c>vertical</c> puts the icon above the content.
/// </summary>
public sealed class Marker : ContentControl
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<Marker, object?>(nameof(Icon));

    /// <summary>The 16 px icon before the content.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }
}

/// <summary>
/// shadcn/ui's Marker as a link (<c>&lt;Marker asChild&gt;&lt;a href&gt;</c>): the marker's row of icon and content, its
/// text underlined 3 px below and the foreground color under the pointer, an <c>&lt;a href&gt;</c> on the web.
/// </summary>
public sealed class MarkerLink : HyperlinkButton
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty = Marker.IconProperty.AddOwner<MarkerLink>();

    /// <summary>The 16 px icon before the content.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MarkerLink);
}

/// <summary>
/// shadcn/ui's Marker as a button (<c>&lt;Marker asChild&gt;&lt;button&gt;</c>): the marker's row of icon and content,
/// the foreground color under the pointer, a <c>&lt;button&gt;</c> on the web.
/// </summary>
public sealed class MarkerButton : Button
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty = Marker.IconProperty.AddOwner<MarkerButton>();

    /// <summary>The 16 px icon before the content.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(MarkerButton);
}

/// <summary>
/// shadcn/ui's Bubble: its content in a rounded box at most 80% of its row wide, in the variant its class names
/// (<c>secondary</c>, <c>muted</c>, <c>tinted</c>, <c>outline</c>, <c>ghost</c>, <c>destructive</c>; the primary color
/// otherwise), at the row's end with <c>end</c>, its <see cref="Reactions"/> overlapping its bottom edge. A
/// <see cref="Button"/> or <see cref="HyperlinkButton"/> as its content is the bubble's box itself (shadcn/ui's
/// <c>&lt;BubbleContent asChild&gt;</c>, the <c>:action</c> pseudo-class): it takes the variant's color, its hover color
/// and the focus ring.
/// </summary>
public sealed class Bubble : ContentControl
{
    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ContentProperty)
        {
            PseudoClasses.Set(":action", change.NewValue is Button);
        }
    }

    /// <summary>Defines the <see cref="Reactions"/> property.</summary>
    public static readonly StyledProperty<object?> ReactionsProperty = AvaloniaProperty.Register<Bubble, object?>(nameof(Reactions));

    /// <summary>The reactions shown on the bubble's bottom edge, 12 px from its end (its start with the <c>reactions-start</c> class).</summary>
    public object? Reactions
    {
        get => GetValue(ReactionsProperty);
        set => SetValue(ReactionsProperty, value);
    }

    /// <summary>
    /// Measures the bubble across its row, its content in the row's first or last 80% (star columns, written as a CSS
    /// grid); content too wide for them wraps across all of them (<c>:wraps</c>), as a <c>w-fit max-w-[80%]</c> box does.
    /// </summary>
    /// <param name="availableSize">The space the row gives.</param>
    /// <returns>The bubble's size.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (!double.IsInfinity(availableSize.Width))
        {
            var natural = base.MeasureOverride(availableSize.WithWidth(double.PositiveInfinity));
            PseudoClasses.Set(":wraps", natural.Width > availableSize.Width * 0.8);
        }

        return base.MeasureOverride(availableSize);
    }
}

/// <summary>shadcn/ui's BubbleGroup: bubbles stacked 8 px apart.</summary>
public sealed class BubbleGroup : StackPanel
{
}

/// <summary>
/// shadcn/ui's Message: a chat row of its <see cref="Avatar"/>, at the row's bottom edge, and its content 8 px apart; the
/// <c>end</c> class reverses the row and aligns the content's parts to its end. Over a <see cref="MessageFooter"/> the
/// avatar is raised 32 px, as shadcn/ui's <c>-translate-y-8</c>.
/// </summary>
public sealed class Message : ContentControl
{
    /// <summary>Defines the <see cref="Avatar"/> property.</summary>
    public static readonly StyledProperty<object?> AvatarProperty = AvaloniaProperty.Register<Message, object?>(nameof(Avatar));

    /// <summary>The avatar beside the content, at the row's bottom edge.</summary>
    public object? Avatar
    {
        get => GetValue(AvatarProperty);
        set => SetValue(AvatarProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        PseudoClasses.Set(":footer", Content is Panel panel && panel.Children.OfType<MessageFooter>().Any());
    }
}

/// <summary>shadcn/ui's MessageContent: a message's header, bubbles and footer stacked 10 px apart.</summary>
public sealed class MessageContent : StackPanel
{
}

/// <summary>shadcn/ui's MessageHeader: a 12 px medium muted line above a message's bubbles, 12 px from its side.</summary>
public sealed class MessageHeader : TextBlock
{
}

/// <summary>shadcn/ui's MessageFooter: a 12 px medium muted line below a message's bubbles, 12 px from its side.</summary>
public sealed class MessageFooter : TextBlock
{
}

/// <summary>shadcn/ui's MessageGroup: messages stacked 8 px apart.</summary>
public sealed class MessageGroup : StackPanel
{
}
