using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Metadata;

namespace Aioros.Cosmo.Overlays;

/// <summary>The edge a <see cref="Sheet"/> slides from.</summary>
public enum SheetSide
{
    /// <summary>From the right edge, as tall as the host, 384 px wide at most (the default).</summary>
    Right,

    /// <summary>From the left edge.</summary>
    Left,

    /// <summary>From the top edge, as wide as the host.</summary>
    Top,

    /// <summary>From the bottom edge.</summary>
    Bottom,
}

/// <summary>
/// shadcn/ui's Sheet (Radix Dialog at an edge): a <see cref="Dialog"/> whose surface covers one <see cref="Side"/> of its
/// host, with a close button; Escape and a click on the mask close it.
/// </summary>
public sealed class Sheet : Control
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = Dialog.IsOpenProperty.AddOwner<Sheet>();

    /// <summary>Defines <see cref="Content"/>.</summary>
    public static readonly StyledProperty<object?> ContentProperty = Dialog.ContentProperty.AddOwner<Sheet>();

    /// <summary>Defines <see cref="ContentTemplate"/>.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = Dialog.ContentTemplateProperty.AddOwner<Sheet>();

    /// <summary>Defines <see cref="Trigger"/>.</summary>
    public static readonly StyledProperty<Button?> TriggerProperty = Dialog.TriggerProperty.AddOwner<Sheet>();

    /// <summary>Defines <see cref="Side"/>.</summary>
    public static readonly StyledProperty<SheetSide> SideProperty = AvaloniaProperty.Register<Sheet, SheetSide>(nameof(Side));

    private readonly Modal _modal;

    static Sheet()
    {
        IsOpenProperty.Changed.AddClassHandler<Sheet>((sheet, change) => sheet._modal.Toggle(change.GetNewValue<bool>()));
        Web.ShowsProperty.Changed.AddClassHandler<Sheet>((sheet, _) => sheet._modal.Toggle(sheet.IsOpen));
        ContentProperty.Changed.AddClassHandler<Sheet>((sheet, _) => sheet._modal.Refresh());
        SideProperty.Changed.AddClassHandler<Sheet>((sheet, _) => sheet._modal.Refresh());
        TriggerProperty.Changed.AddClassHandler<Sheet>((sheet, change) => sheet._modal.Trigger(change.GetOldValue<Button?>(), change.GetNewValue<Button?>()));
    }

    /// <summary>Creates a closed sheet.</summary>
    public Sheet()
    {
        IsVisible = false;
        _modal = new Modal(this, "sheet", IsOpenProperty);
        Web.SetModal(this, nameof(IsOpen));
    }

    /// <summary>The button that opens the sheet (Radix's <c>DialogTrigger</c>): in the browser with no request on the web.</summary>
    public Button? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>Whether the sheet is open, two way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The edge the sheet covers (<see cref="SheetSide.Right"/> by default, shadcn's <c>side</c>).</summary>
    public SheetSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    /// <summary>What the sheet shows.</summary>
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

    /// <summary>The open surface, while the sheet is open.</summary>
    public DialogSurface? Surface => _modal.Surface;
}

/// <summary>
/// shadcn/ui's Drawer (vaul): a panel at the bottom of its host with rounded top corners and a handle, 80% of the host's
/// height at most; Escape, a click on the mask and a drag down past a quarter of its height (or a flick) close it, as
/// vaul's do, and the browser drags it the same way on the web. With another <see cref="Side"/> (vaul's <c>direction</c>)
/// it is a full-height panel on that edge, three quarters of the host's width and 384 px at most, with no handle, and it is
/// dragged toward that edge to close.
/// </summary>
public sealed class Drawer : Control
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = Dialog.IsOpenProperty.AddOwner<Drawer>();

    /// <summary>Defines <see cref="Content"/>.</summary>
    public static readonly StyledProperty<object?> ContentProperty = Dialog.ContentProperty.AddOwner<Drawer>();

    /// <summary>Defines <see cref="ContentTemplate"/>.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = Dialog.ContentTemplateProperty.AddOwner<Drawer>();

    /// <summary>Defines <see cref="Trigger"/>.</summary>
    public static readonly StyledProperty<Button?> TriggerProperty = Dialog.TriggerProperty.AddOwner<Drawer>();

    /// <summary>Defines <see cref="Side"/>.</summary>
    public static readonly StyledProperty<SheetSide> SideProperty = AvaloniaProperty.Register<Drawer, SheetSide>(nameof(Side), SheetSide.Bottom);

    private readonly Modal _modal;

    static Drawer()
    {
        Dialog.ShowCloseButtonProperty.OverrideDefaultValue<Drawer>(false);
        IsOpenProperty.Changed.AddClassHandler<Drawer>((drawer, change) => drawer._modal.Toggle(change.GetNewValue<bool>()));
        Web.ShowsProperty.Changed.AddClassHandler<Drawer>((drawer, _) => drawer._modal.Toggle(drawer.IsOpen));
        ContentProperty.Changed.AddClassHandler<Drawer>((drawer, _) => drawer._modal.Refresh());
        TriggerProperty.Changed.AddClassHandler<Drawer>((drawer, change) => drawer._modal.Trigger(change.GetOldValue<Button?>(), change.GetNewValue<Button?>()));
    }

    /// <summary>Creates a closed drawer.</summary>
    public Drawer()
    {
        IsVisible = false;
        _modal = new Modal(this, "drawer", IsOpenProperty);
        Web.SetModal(this, nameof(IsOpen));
    }

    /// <summary>The button that opens the drawer (Vaul's <c>DrawerTrigger</c>): in the browser with no request on the web.</summary>
    public Button? Trigger
    {
        get => GetValue(TriggerProperty);
        set => SetValue(TriggerProperty, value);
    }

    /// <summary>Whether the drawer is open, two way.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>The edge the drawer opens from (vaul's <c>direction</c>), the bottom by default.</summary>
    public SheetSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    /// <summary>What the drawer shows.</summary>
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

    /// <summary>The clock a drag's speed is read by (the system's by default; a test's fake one in tests).</summary>
    public TimeProvider TimeProvider { get; set; } = TimeProvider.System;

    /// <summary>The open surface, while the drawer is open.</summary>
    public DialogSurface? Surface => _modal.Surface;
}
