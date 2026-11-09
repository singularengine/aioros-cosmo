using Avalonia;
using Avalonia.Controls;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's Alert: a bordered callout with an optional <see cref="Icon"/>, a <see cref="Title"/> and its content as the
/// description; the <c>destructive</c> class colors it with <c>--destructive</c>.
/// </summary>
public sealed class Alert : ContentControl
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<Alert, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<Alert, object?>(nameof(Title));

    /// <summary>The 16 px icon left of the title, in the alert's text color.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The alert's title, in medium weight.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }
}
