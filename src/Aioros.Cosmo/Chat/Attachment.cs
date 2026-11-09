using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Aioros.Cosmo.Chat;

/// <summary>Where an attachment is in its upload, shadcn/ui's <c>state</c>.</summary>
public enum AttachmentState
{
    /// <summary>Uploaded or ready: the attachment's usual look.</summary>
    Done,

    /// <summary>Chosen and not sent yet: a dashed outline.</summary>
    Idle,

    /// <summary>Uploading.</summary>
    Uploading,

    /// <summary>Being processed after its upload.</summary>
    Processing,

    /// <summary>Failed: the destructive border, media and description.</summary>
    Error,
}

/// <summary>
/// shadcn/ui's Attachment: a file in a chat, a rounded card of its <see cref="Media"/>, its <see cref="Title"/> and
/// <see cref="Description"/> and its <see cref="Actions"/>, in its <see cref="State"/> (the pseudo-classes <c>:idle</c>,
/// <c>:uploading</c>, <c>:processing</c>, <c>:error</c> and <c>:done</c>); the classes <c>sm</c> and <c>xs</c> make it
/// smaller, <c>vertical</c> puts the media above the texts and <c>image</c> lets an image fill the media.
/// </summary>
public sealed class Attachment : TemplatedControl
{
    /// <summary>Defines the <see cref="Media"/> property.</summary>
    public static readonly StyledProperty<object?> MediaProperty = AvaloniaProperty.Register<Attachment, object?>(nameof(Media));

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<object?> TitleProperty = AvaloniaProperty.Register<Attachment, object?>(nameof(Title));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<Attachment, object?>(nameof(Description));

    /// <summary>Defines the <see cref="Actions"/> property.</summary>
    public static readonly StyledProperty<object?> ActionsProperty = AvaloniaProperty.Register<Attachment, object?>(nameof(Actions));

    /// <summary>Defines the <see cref="Trigger"/> property.</summary>
    public static readonly StyledProperty<object?> TriggerProperty = AvaloniaProperty.Register<Attachment, object?>(nameof(Trigger));

    /// <summary>Defines the <see cref="State"/> property.</summary>
    public static readonly StyledProperty<AttachmentState> StateProperty = AvaloniaProperty.Register<Attachment, AttachmentState>(nameof(State));

    /// <summary>Starts in the <c>:done</c> state.</summary>
    public Attachment() => Mark(AttachmentState.Done);

    /// <summary>The file's icon or image, in a square on the muted color.</summary>
    public object? Media
    {
        get => GetValue(MediaProperty);
        set => SetValue(MediaProperty, value);
    }

    /// <summary>The file's name, in medium weight, trimmed to one line.</summary>
    public object? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>The file's details, 12 px and muted, trimmed to one line.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>The attachment's buttons, at its end (ghost <c>icon-xs</c> buttons in shadcn/ui's examples).</summary>
    public object? Actions
    {
        get => GetValue(ActionsProperty);
        set => SetValue(ActionsProperty, value);
    }

    /// <summary>
    /// What the whole card does (shadcn/ui's <c>AttachmentTrigger</c>): a <see cref="Button"/> or
    /// <see cref="HyperlinkButton"/> laid over the card under its <see cref="Actions"/>, which stay pressable above it, as the
    /// button opening a dialog's preview or the link opening the file; the card then takes the muted color at half its
    /// alpha under the pointer (the <c>:interactive</c> pseudo-class).
    /// </summary>
    public object? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>Where the attachment is in its upload.</summary>
    public AttachmentState State
    {
        get => GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StateProperty)
        {
            Mark(change.GetNewValue<AttachmentState>());
        }
        else if (change.Property == TriggerProperty)
        {
            PseudoClasses.Set(":interactive", change.NewValue is not null);
        }
    }

    private void Mark(AttachmentState state)
    {
        foreach (var each in Enum.GetValues<AttachmentState>())
        {
            PseudoClasses.Set(":" + each.ToString().ToLowerInvariant(), each == state);
        }
    }
}

/// <summary>shadcn/ui's AttachmentGroup: attachments in a row 12 px apart, scrolled sideways without a scroll bar.</summary>
public sealed class AttachmentGroup : ItemsControl
{
}
