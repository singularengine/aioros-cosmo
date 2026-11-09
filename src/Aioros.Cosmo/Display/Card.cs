using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Aioros.Cosmo.Display;

/// <summary>shadcn/ui's Card: a surface stacking its parts (<see cref="CardHeader"/>, <see cref="CardContent"/>, <see cref="CardFooter"/>) 24 px apart.</summary>
public sealed class Card : ItemsControl
{
}

/// <summary>The header of a <see cref="Card"/>: its title, its description and an optional action in the top right corner; the <c>border</c> class rules it off from the content with a bottom border 24 px below it (shadcn/ui's <c>border-b</c>).</summary>
public sealed class CardHeader : TemplatedControl
{
    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<CardHeader, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<CardHeader, object?>(nameof(Description));

    /// <summary>Defines the <see cref="Action"/> property.</summary>
    public static readonly StyledProperty<object?> ActionProperty = AvaloniaProperty.Register<CardHeader, object?>(nameof(Action));

    /// <summary>The card's title, shown in semibold.</summary>
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

    /// <summary>Content placed in the header's top right corner, beside the title and the description.</summary>
    public object? Action
    {
        get => GetValue(ActionProperty);
        set => SetValue(ActionProperty, value);
    }
}

/// <summary>The main content of a <see cref="Card"/>, inset 24 px on both sides.</summary>
public sealed class CardContent : ContentControl
{
}

/// <summary>The footer of a <see cref="Card"/>, inset 24 px on both sides, its content centered vertically; the <c>border</c> class rules it off with a top border 24 px above it (shadcn/ui's <c>border-t</c>).</summary>
public sealed class CardFooter : ContentControl
{
}
