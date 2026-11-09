using Avalonia;
using Avalonia.Controls;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's Empty: an empty state centered in its box, its <see cref="Media"/>, <see cref="Title"/> and
/// <see cref="Description"/> above its content (the actions); the <c>outline</c> class draws a dashed border.
/// </summary>
public sealed class Empty : ContentControl
{
    /// <summary>Defines the <see cref="Media"/> property.</summary>
    public static readonly StyledProperty<object?> MediaProperty = AvaloniaProperty.Register<Empty, object?>(nameof(Media));

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<Empty, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<Empty, object?>(nameof(Description));

    /// <summary>The picture above the title: an <see cref="EmptyMedia"/>, an avatar, an image.</summary>
    public object? Media
    {
        get => GetValue(MediaProperty);
        set => SetValue(MediaProperty, value);
    }

    /// <summary>The empty state's title.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The muted text under the title.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}

/// <summary>shadcn/ui's EmptyMedia: the picture of an empty state; the <c>icon</c> class puts an icon on a 40 px <c>--muted</c> tile.</summary>
public sealed class EmptyMedia : ContentControl
{
}
