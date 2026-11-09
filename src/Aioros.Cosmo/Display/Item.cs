using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's Item: a row with an optional <see cref="Media"/>, a <see cref="Title"/> and a <see cref="Description"/>,
/// and <see cref="Actions"/> at its end, with an optional full row above it (<see cref="Header"/>) and below it
/// (<see cref="Footer"/>); classes <c>outline</c>, <c>muted</c>, <c>sm</c> and <c>link</c> (a hover look for
/// items that navigate). Its content, when set, takes the title's and description's place.
/// </summary>
public sealed class Item : ContentControl
{
    /// <summary>Defines the <see cref="Media"/> property.</summary>
    public static readonly StyledProperty<object?> MediaProperty = AvaloniaProperty.Register<Item, object?>(nameof(Media));

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<Item, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<Item, object?>(nameof(Description));

    /// <summary>Defines the <see cref="Actions"/> property.</summary>
    public static readonly StyledProperty<object?> ActionsProperty = AvaloniaProperty.Register<Item, object?>(nameof(Actions));

    /// <summary>Defines the <see cref="Header"/> property.</summary>
    public static readonly StyledProperty<object?> HeaderProperty = AvaloniaProperty.Register<Item, object?>(nameof(Header));

    /// <summary>Defines the <see cref="Footer"/> property.</summary>
    public static readonly StyledProperty<object?> FooterProperty = AvaloniaProperty.Register<Item, object?>(nameof(Footer));

    /// <summary>The full row above the item's media, texts and actions (an <see cref="ItemHeader"/>).</summary>
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    /// <summary>The full row below the item's media, texts and actions (an <see cref="ItemFooter"/>).</summary>
    public object? Footer
    {
        get => GetValue(FooterProperty);
        set => SetValue(FooterProperty, value);
    }

    /// <summary>The icon, avatar or image at the item's start (an <see cref="ItemMedia"/>).</summary>
    public object? Media
    {
        get => GetValue(MediaProperty);
        set => SetValue(MediaProperty, value);
    }

    /// <summary>The item's title, in medium weight.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The muted text under the title, two lines at most.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>Buttons or an icon at the item's end.</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }
}

/// <summary>shadcn/ui's ItemMedia: the classes <c>icon</c> (a 32 px bordered <c>--muted</c> tile) and <c>image</c> (a 40 px rounded clip).</summary>
public sealed class ItemMedia : ContentControl
{
}

/// <summary>shadcn/ui's ItemGroup: items in a column.</summary>
public sealed class ItemGroup : StackPanel
{
}

/// <summary>shadcn/ui's ItemHeader: a row whose first and last children sit at its ends, vertically centered, 8 px apart at least.</summary>
public sealed class ItemHeader : DockPanel
{
    /// <summary>Creates the row.</summary>
    public ItemHeader()
    {
        LastChildFill = false;
        HorizontalSpacing = 8;
    }
}

/// <summary>shadcn/ui's ItemFooter: a row whose first and last children sit at its ends, vertically centered, 8 px apart at least.</summary>
public sealed class ItemFooter : DockPanel
{
    /// <summary>Creates the row.</summary>
    public ItemFooter()
    {
        LastChildFill = false;
        HorizontalSpacing = 8;
    }
}

/// <summary>shadcn/ui's ItemSeparator: a separator between the items of an <see cref="ItemGroup"/>, with no margin.</summary>
public sealed class ItemSeparator : Separator
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(Separator);
}
