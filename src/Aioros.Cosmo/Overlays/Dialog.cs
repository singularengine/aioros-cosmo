using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// shadcn/ui's Dialog (Radix Dialog): a modal surface centered over its <see cref="OverlayHost"/> (512 px wide at most,
/// <c>MaxWidth</c> to change it) on a 50% black mask, open while <see cref="IsOpen"/> (two-way) is true. The dialog itself
/// takes no room where it is declared; its <see cref="Content"/> is shown in a <see cref="DialogSurface"/>. Escape, the
/// close button and a click on the mask close it; a button with <c>c:Dialog.Closes="True"</c> closes it too.
/// </summary>
public sealed class Dialog : Control
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Dialog, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="Content"/>.</summary>
    public static readonly StyledProperty<object?> ContentProperty = AvaloniaProperty.Register<Dialog, object?>(nameof(Content));

    /// <summary>Defines <see cref="ContentTemplate"/>.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = AvaloniaProperty.Register<Dialog, IDataTemplate?>(nameof(ContentTemplate));

    /// <summary>Defines <see cref="ShowCloseButton"/>.</summary>
    public static readonly StyledProperty<bool> ShowCloseButtonProperty = AvaloniaProperty.Register<Dialog, bool>(nameof(ShowCloseButton), true);

    /// <summary>Defines <see cref="IsLightDismissEnabled"/>.</summary>
    public static readonly StyledProperty<bool> IsLightDismissEnabledProperty = AvaloniaProperty.Register<Dialog, bool>(nameof(IsLightDismissEnabled), true);

    /// <summary>Defines the <c>Closes</c> attached property.</summary>
    public static readonly AttachedProperty<bool> ClosesProperty = AvaloniaProperty.RegisterAttached<Dialog, Button, bool>("Closes");

    /// <summary>Defines <see cref="Trigger"/>.</summary>
    public static readonly StyledProperty<Button?> TriggerProperty = AvaloniaProperty.Register<Dialog, Button?>(nameof(Trigger));

    private readonly Modal _modal;

    static Dialog()
    {
        IsOpenProperty.Changed.AddClassHandler<Dialog>((dialog, change) => dialog._modal.Toggle(change.GetNewValue<bool>()));
        Web.ShowsProperty.Changed.AddClassHandler<Dialog>((dialog, _) => dialog._modal.Toggle(dialog.IsOpen));
        ContentProperty.Changed.AddClassHandler<Dialog>((dialog, _) => dialog._modal.Refresh());
        Button.ClickEvent.AddClassHandler<Button>(Closing, RoutingStrategies.Bubble);
        ClosesProperty.Changed.AddClassHandler<Button>((button, change) => Web.SetCloses(button, change.GetNewValue<bool>()));
        TriggerProperty.Changed.AddClassHandler<Dialog>((dialog, change) => dialog._modal.Trigger(change.GetOldValue<Button?>(), change.GetNewValue<Button?>()));
    }

    /// <summary>Creates a closed dialog, which takes no room where it is declared.</summary>
    public Dialog()
    {
        IsVisible = false;
        _modal = new Modal(this, "dialog", IsOpenProperty);
        Web.SetModal(this, nameof(IsOpen));
    }

    /// <summary>The button that opens the dialog (Radix's <c>DialogTrigger</c>): in the browser with no request on the web.</summary>
    public Button? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>Whether the dialog is open (Radix's <c>open</c> and <c>onOpenChange</c>), two way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>What the dialog shows: its header, fields and footer, or a model drawn by <see cref="ContentTemplate"/>.</summary>
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

    /// <summary>Whether the close button is shown in the top right corner (shadcn's <c>showCloseButton</c>, true).</summary>
    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    /// <summary>Whether a click on the mask closes the dialog (true).</summary>
    public bool IsLightDismissEnabled
    {
        get => GetValue(IsLightDismissEnabledProperty);
        set => SetValue(IsLightDismissEnabledProperty, value);
    }

    /// <summary>The open surface, while the dialog is open.</summary>
    public DialogSurface? Surface => _modal.Surface;

    /// <summary>Whether clicking the button closes the dialog, sheet or drawer it is in (shadcn's <c>DialogClose</c>).</summary>
    public static bool GetCloses(Button button) => button.GetValue(ClosesProperty);

    /// <summary>Makes clicking the button close the dialog, sheet or drawer it is in.</summary>
    public static void SetCloses(Button button, bool value) => button.SetValue(ClosesProperty, value);

    private static void Closing(Button button, RoutedEventArgs e)
    {
        if (GetCloses(button) && button.FindAncestorOfType<DialogSurface>() is { } surface)
        {
            surface.RaiseEvent(new RoutedEventArgs(DialogSurface.CloseRequestedEvent, surface));
        }
    }
}

/// <summary>
/// shadcn/ui's Alert Dialog (Radix AlertDialog): a <see cref="Dialog"/> that interrupts with important content and expects
/// an answer: no close button, a click on the mask does not close it, the focus starts on its cancel button
/// (<c>IsCancel="True"</c>) and Escape closes it. <c>Classes="sm"</c> is shadcn's small size.
/// </summary>
public sealed class AlertDialog : Control
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = Dialog.IsOpenProperty.AddOwner<AlertDialog>();

    /// <summary>Defines <see cref="Content"/>.</summary>
    public static readonly StyledProperty<object?> ContentProperty = Dialog.ContentProperty.AddOwner<AlertDialog>();

    /// <summary>Defines <see cref="ContentTemplate"/>.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = Dialog.ContentTemplateProperty.AddOwner<AlertDialog>();

    /// <summary>Defines <see cref="Trigger"/>.</summary>
    public static readonly StyledProperty<Button?> TriggerProperty = Dialog.TriggerProperty.AddOwner<AlertDialog>();

    private readonly Modal _modal;

    static AlertDialog()
    {
        Dialog.ShowCloseButtonProperty.OverrideDefaultValue<AlertDialog>(false);
        Dialog.IsLightDismissEnabledProperty.OverrideDefaultValue<AlertDialog>(false);
        IsOpenProperty.Changed.AddClassHandler<AlertDialog>((dialog, change) => dialog._modal.Toggle(change.GetNewValue<bool>()));
        Web.ShowsProperty.Changed.AddClassHandler<AlertDialog>((dialog, _) => dialog._modal.Toggle(dialog.IsOpen));
        ContentProperty.Changed.AddClassHandler<AlertDialog>((dialog, _) => dialog._modal.Refresh());
        TriggerProperty.Changed.AddClassHandler<AlertDialog>((dialog, change) => dialog._modal.Trigger(change.GetOldValue<Button?>(), change.GetNewValue<Button?>()));
    }

    /// <summary>Creates a closed alert dialog.</summary>
    public AlertDialog()
    {
        IsVisible = false;
        _modal = new Modal(this, "alert", IsOpenProperty);
        Web.SetModal(this, nameof(IsOpen));
    }

    /// <summary>The button that opens the alert dialog (Radix's <c>AlertDialogTrigger</c>): in the browser with no request on the web.</summary>
    public Button? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>Whether the alert dialog is open, two way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>What the alert dialog shows: its header and its footer's answers.</summary>
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

    /// <summary>The open surface, while the alert dialog is open.</summary>
    public DialogSurface? Surface => _modal.Surface;
}
