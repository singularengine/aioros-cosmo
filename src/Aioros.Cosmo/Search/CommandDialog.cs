using Avalonia;
using Avalonia.Automation;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Aioros.Cosmo.Overlays;

namespace Aioros.Cosmo.Search;

/// <summary>
/// shadcn/ui's CommandDialog: a <see cref="Command"/> in a dialog (Radix Dialog, 512 px wide, no padding; the command's
/// input 48 px high, its items 44 px, its groups 8 px in), opened and closed by <see cref="Gesture"/> anywhere in its
/// window (Ctrl+K, or Meta+K on a Mac keyboard, by default; shadcn's example uses J) and while <see cref="IsOpen"/> (two
/// way) is true. Escape, the close button and a click outside close it. <see cref="Title"/> names the dialog for
/// assistive technologies (shadcn's screen-reader-only header). A <see cref="Trigger"/>, the button that opens the
/// dialog, takes the <see cref="Gesture"/> as its hot key, and the dialog leaves the gesture to it, so the same click opens
/// the dialog on the desktop and in the browser (where Aioros writes a hot key as a page shortcut).
/// </summary>
public sealed class CommandDialog : Control
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = Dialog.IsOpenProperty.AddOwner<CommandDialog>();

    /// <summary>Defines <see cref="Content"/>.</summary>
    public static readonly StyledProperty<object?> ContentProperty = Dialog.ContentProperty.AddOwner<CommandDialog>();

    /// <summary>Defines <see cref="ContentTemplate"/>.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = Dialog.ContentTemplateProperty.AddOwner<CommandDialog>();

    /// <summary>Defines <see cref="Gesture"/>.</summary>
    public static readonly StyledProperty<KeyGesture?> GestureProperty =
        AvaloniaProperty.Register<CommandDialog, KeyGesture?>(nameof(Gesture), new KeyGesture(Key.K, KeyModifiers.Control));

    /// <summary>Defines <see cref="Title"/>.</summary>
    public static readonly StyledProperty<string?> TitleProperty = AvaloniaProperty.Register<CommandDialog, string?>(nameof(Title), "Command Palette");

    /// <summary>Defines <see cref="Trigger"/>.</summary>
    public static readonly StyledProperty<Button?> TriggerProperty = AvaloniaProperty.Register<CommandDialog, Button?>(nameof(Trigger));

    private readonly Modal _modal;
    private TopLevel? _top;
    private KeyGesture? _assigned;

    static CommandDialog()
    {
        IsOpenProperty.Changed.AddClassHandler<CommandDialog>((dialog, change) => dialog.Toggle(change.GetNewValue<bool>()));
        Aioros.Markup.Web.ShowsProperty.Changed.AddClassHandler<CommandDialog>((dialog, _) => dialog.Toggle(dialog.IsOpen));
        ContentProperty.Changed.AddClassHandler<CommandDialog>((dialog, _) => dialog._modal.Refresh());
        TriggerProperty.Changed.AddClassHandler<CommandDialog>((dialog, change) =>
        {
            dialog.Assign(change.GetOldValue<Button?>());
            dialog._modal.Trigger(change.GetOldValue<Button?>(), change.GetNewValue<Button?>(), clicks: false);
        });
        GestureProperty.Changed.AddClassHandler<CommandDialog>((dialog, _) => dialog.Assign(dialog.Trigger));
    }

    /// <summary>Creates a closed command dialog, which takes no room where it is declared.</summary>
    public CommandDialog()
    {
        IsVisible = false;
        Classes.Add("command");
        _modal = new Modal(this, "dialog", IsOpenProperty);
        Aioros.Markup.Web.SetModal(this, nameof(IsOpen));
        Aioros.Markup.Web.SetModalKey(this, Gesture);
    }

    /// <summary>Whether the dialog is open, two way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>What the dialog shows: its <see cref="Command"/>.</summary>
    [Content]
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    /// <summary>The template of a model <see cref="Content"/>.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <summary>The keys that open and close the dialog anywhere in its window; Meta stands for Ctrl too, as cmdk's example reads <c>metaKey || ctrlKey</c>.</summary>
    public KeyGesture? Gesture
    {
        get => GetValue(GestureProperty);
        set => SetValue(GestureProperty, value);
    }

    /// <summary>The dialog's accessible name (shadcn's <c>title</c>, "Command Palette").</summary>
    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>
    /// The button that opens the dialog (its command opens it, as a model's bound <see cref="IsOpen"/>): it takes the
    /// <see cref="Gesture"/> as its <c>HotKey</c> unless it has one, and the dialog then leaves the gesture to it.
    /// </summary>
    public Button? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>The open surface, while the dialog is open.</summary>
    public DialogSurface? Surface => _modal.Surface;

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _top = TopLevel.GetTopLevel(this);
        _top?.AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel, handledEventsToo: true);
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
    {
        _top?.RemoveHandler(KeyDownEvent, OnKeyDown);
        _top = null;
        base.OnDetachedFromVisualTree(e);
    }

    /// <summary>Gives the trigger the gesture as its hot key, taking it back from a trigger it was given to before.</summary>
    private void Assign(Button? previous)
    {
        if (previous is not null && _assigned is { } given && Equals(previous.HotKey, given))
        {
            previous.ClearValue(Button.HotKeyProperty);
        }

        _assigned = null;
        if (Trigger is { } trigger && trigger.HotKey is null && Gesture is { } gesture)
        {
            trigger.SetCurrentValue(Button.HotKeyProperty, gesture);
            _assigned = gesture;
        }

        Aioros.Markup.Web.SetModalKey(this, Trigger is null ? Gesture : null);
    }

    private void Toggle(bool open)
    {
        _modal.Toggle(open);
        if (open && _modal.Surface is { } surface)
        {
            AutomationProperties.SetName(surface, Title);
        }
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (Gesture is not { } gesture || e.Key != gesture.Key)
        {
            return;
        }

        var modifiers = e.KeyModifiers;
        var expected = gesture.KeyModifiers;
        var macStyle = (expected & KeyModifiers.Control) != 0 && modifiers == ((expected & ~KeyModifiers.Control) | KeyModifiers.Meta);
        if (modifiers == expected && Trigger is { IsEffectivelyEnabled: true } trigger && gesture.Equals(trigger.HotKey))
        {
            return;
        }

        if (modifiers == expected || macStyle)
        {
            e.Handled = true;
            SetCurrentValue(IsOpenProperty, !IsOpen);
        }
    }
}
