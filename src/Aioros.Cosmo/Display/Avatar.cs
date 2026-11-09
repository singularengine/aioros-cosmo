using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's Avatar (Radix Avatar): an image in a 32 px circle (<c>sm</c> 24, <c>lg</c> 40), or its content, the
/// fallback (initials on <c>--muted</c>), while it has no image; a <see cref="AvatarBadge"/> in <see cref="Badge"/> sits
/// in its bottom right corner, inside the circle.
/// </summary>
public sealed class Avatar : ContentControl
{
    /// <summary>Defines the <see cref="Source"/> property.</summary>
    public static readonly StyledProperty<IImage?> SourceProperty = AvaloniaProperty.Register<Avatar, IImage?>(nameof(Source));

    /// <summary>Defines the <see cref="Badge"/> property.</summary>
    public static readonly StyledProperty<object?> BadgeProperty = AvaloniaProperty.Register<Avatar, object?>(nameof(Badge));

    /// <summary>Creates an avatar that tells its group its size.</summary>
    public Avatar()
    {
        Classes.CollectionChanged += (_, _) => this.FindAncestorOfType<AvatarGroup>()?.Size();
    }

    /// <summary>The avatar's image; the fallback shows while it is null.</summary>
    public IImage? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>The badge in the avatar's bottom right corner, usually an <see cref="AvatarBadge"/>.</summary>
    public object? Badge
    {
        get => GetValue(BadgeProperty);
        set => SetValue(BadgeProperty, value);
    }

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        this.FindAncestorOfType<AvatarGroup>()?.Size();
    }
}

/// <summary>shadcn/ui's AvatarBadge: a dot in <c>--primary</c> ringed with <c>--background</c>, 10 px (8 in a <c>sm</c> avatar, which hides its icon, 12 in a <c>lg</c> one); its content is an icon.</summary>
public sealed class AvatarBadge : ContentControl
{
}

/// <summary>
/// shadcn/ui's avatar group: its avatars (items, or avatars an <c>ItemTemplate</c> builds) in a row overlapping by 8 px, each
/// ringed with <c>--background</c>, and an optional <see cref="AvatarGroupCount"/> last, as large as the group's avatars.
/// </summary>
public sealed class AvatarGroup : ItemsControl
{
    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        container.Margin = new Thickness(-8, 0, 0, 0);
    }

    internal void Size()
    {
        var avatars = this.GetVisualDescendants().OfType<Avatar>().ToList();
        PseudoClasses.Set(":sm", avatars.Exists(static avatar => avatar.Classes.Contains("sm")));
        PseudoClasses.Set(":lg", avatars.Exists(static avatar => avatar.Classes.Contains("lg")));
    }
}

/// <summary>shadcn/ui's AvatarGroupCount: the number of avatars a group leaves out (or an icon), in a <c>--muted</c> circle as large as the group's avatars.</summary>
public sealed class AvatarGroupCount : ContentControl
{
}
