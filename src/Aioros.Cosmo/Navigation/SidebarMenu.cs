using Aioros.Markup;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Navigation;

/// <summary>shadcn/ui's SidebarHeader: the sidebar's top, its children 8 px apart and 8 px in.</summary>
public sealed class SidebarHeader : StackPanel
{
    /// <summary>Stacks the children 8 px apart.</summary>
    public SidebarHeader() => Spacing = 8;
}

/// <summary>shadcn/ui's SidebarFooter: the sidebar's bottom, its children 8 px apart and 8 px in.</summary>
public sealed class SidebarFooter : StackPanel
{
    /// <summary>Stacks the children 8 px apart.</summary>
    public SidebarFooter() => Spacing = 8;
}

/// <summary>shadcn/ui's SidebarContent: the sidebar's scrolling middle, its <see cref="SidebarGroup"/>s 8 px apart.</summary>
public sealed class SidebarContent : ItemsControl
{
}

/// <summary>shadcn/ui's SidebarGroup: a section of the sidebar, 8 px in, a <see cref="SidebarGroupLabel"/> over its menu.</summary>
public sealed class SidebarGroup : StackPanel
{
}

/// <summary>shadcn/ui's SidebarGroupLabel: a 32 px row of 12 px medium text at 70% of <c>--sidebar-foreground</c>, which moves up and fades out when the sidebar shows only its icons.</summary>
public sealed class SidebarGroupLabel : ContentControl
{
}

/// <summary>shadcn/ui's SidebarMenu: a list of <see cref="SidebarMenuItem"/>s 4 px apart, its items declared or from <c>ItemsSource</c>.</summary>
public sealed class SidebarMenu : ItemsControl
{
    static SidebarMenu() => AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<SidebarMenu>(AutomationControlType.List);
}

/// <summary>
/// shadcn/ui's SidebarMenuItem: a <see cref="SidebarMenuButton"/> with, optionally, a <see cref="SidebarMenuAction"/> or a
/// <see cref="SidebarMenuBadge"/> at its end and a <see cref="SidebarMenuSub"/> under it. An <see cref="IsCollapsible"/>
/// item (shadcn's <c>Collapsible</c> around it) shows its sub-menu while <see cref="IsExpanded"/> (two way), which its
/// button toggles, and turns its button's chevron (or its action, when the action <see cref="SidebarMenuAction.Toggles"/>);
/// on the web the browser toggles it with no request (its flag, posted only when <see cref="IsExpanded"/> is bound two way).
/// </summary>
public sealed class SidebarMenuItem : Grid
{
    /// <summary>Defines <see cref="IsCollapsible"/>.</summary>
    public static readonly StyledProperty<bool> IsCollapsibleProperty = AvaloniaProperty.Register<SidebarMenuItem, bool>(nameof(IsCollapsible));

    /// <summary>Defines <see cref="IsExpanded"/>.</summary>
    public static readonly StyledProperty<bool> IsExpandedProperty = AvaloniaProperty.Register<SidebarMenuItem, bool>(nameof(IsExpanded), defaultBindingMode: BindingMode.TwoWay);

    static SidebarMenuItem()
    {
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<SidebarMenuItem>(AutomationControlType.ListItem);
        IsCollapsibleProperty.Changed.AddClassHandler<SidebarMenuItem>((item, _) => item.Mark());
        IsExpandedProperty.Changed.AddClassHandler<SidebarMenuItem>((item, _) => item.Mark());
    }

    /// <summary>Lays the button over the sub-menu and hooks the button's toggle.</summary>
    public SidebarMenuItem()
    {
        RowDefinitions = [new RowDefinition(GridLength.Auto), new RowDefinition(GridLength.Auto)];
        AddHandler(Button.ClickEvent, OnClick);
        Children.CollectionChanged += (_, _) => Place();
    }

    /// <summary>Whether the item's button toggles its sub-menu.</summary>
    public bool IsCollapsible
    {
        get => GetValue(IsCollapsibleProperty);
        set => SetValue(IsCollapsibleProperty, value);
    }

    /// <summary>Whether the item's sub-menu is shown, two way.</summary>
    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    private void Place()
    {
        foreach (var child in Children)
        {
            SetRow(child, child is SidebarMenuSub ? 1 : 0);
        }

        Flag();
    }

    private void Mark()
    {
        PseudoClasses.Set(":collapsible", IsCollapsible);
        PseudoClasses.Set(":expanded", IsExpanded);
        Flag();
    }

    internal void Retoggle() => Flag();

    private Button? Toggler => Children.OfType<SidebarMenuAction>().FirstOrDefault(static action => action.Toggles) as Button ?? Children.OfType<SidebarMenuButton>().FirstOrDefault();

    private void Flag()
    {
        var flag = IsCollapsible ? nameof(IsExpanded) : null;
        var toggler = Toggler;
        PseudoClasses.Set(":action-toggles", toggler is SidebarMenuAction);
        PseudoClasses.Set(":action", Children.OfType<SidebarMenuAction>().Any());
        Web.SetFlag(this, flag);
        foreach (var button in Children.OfType<Button>().Where(static child => child is SidebarMenuButton or SidebarMenuAction))
        {
            Web.SetFlips(button, button == toggler ? flag : null);
        }
    }

    private void OnClick(object? sender, RoutedEventArgs e)
    {
        if (IsCollapsible && e.Source is Button button && button.Parent == this && button == Toggler)
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
        }
    }
}

/// <summary>
/// shadcn/ui's SidebarMenuButton: a 32 px row (8 px of padding, a 6 px radius, 14 px text, its icon 16 px and 8 px from
/// the text), <c>--sidebar-accent</c> under the pointer and while <see cref="IsActive"/> (medium text then), <c>sm</c>
/// (28 px, 12 px text), <c>lg</c> (48 px) and <c>outline</c> as classes. When the sidebar shows only its icons it is a
/// 32 px square showing <see cref="Tooltip"/> at its right.
/// </summary>
public sealed class SidebarMenuButton : Button
{
    /// <summary>Defines <see cref="IsActive"/>.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty = AvaloniaProperty.Register<SidebarMenuButton, bool>(nameof(IsActive));

    /// <summary>Defines <see cref="Tooltip"/>.</summary>
    public static readonly StyledProperty<string?> TooltipProperty = AvaloniaProperty.Register<SidebarMenuButton, string?>(nameof(Tooltip));

    private Sidebar? _sidebar;

    static SidebarMenuButton()
    {
        IsActiveProperty.Changed.AddClassHandler<SidebarMenuButton>((button, change) => button.PseudoClasses.Set(":active", change.GetNewValue<bool>()));
        TooltipProperty.Changed.AddClassHandler<SidebarMenuButton>((button, _) => button.Tip());
    }

    /// <summary>Whether the button is the current page's (shadcn's <c>isActive</c>).</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <summary>The tip shown at the button's right while the sidebar shows only its icons (shadcn's <c>tooltip</c>).</summary>
    public string? Tooltip
    {
        get => GetValue(TooltipProperty);
        set => SetValue(TooltipProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarMenuButton);

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        _sidebar = this.FindLogicalAncestorOfType<Sidebar>();
        if (_sidebar is not null)
        {
            _sidebar.StateChanged += OnSidebarChanged;
        }

        Tip();
    }

    /// <inheritdoc />
    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        if (_sidebar is not null)
        {
            _sidebar.StateChanged -= OnSidebarChanged;
            _sidebar = null;
        }

        base.OnDetachedFromLogicalTree(e);
    }

    private void OnSidebarChanged(object? sender, EventArgs e) => Tip();

    private void Tip()
    {
        ToolTip.SetTip(this, _sidebar?.IsIconOnly == true ? Tooltip : null);
        ToolTip.SetPlacement(this, PlacementMode.Right);
        ToolTip.SetHorizontalOffset(this, 4);
    }
}

/// <summary>shadcn/ui's SidebarMenuSub: an item's sub-menu, 14 px in behind a 1 px <c>--sidebar-border</c> line, its <see cref="SidebarMenuSubButton"/>s 4 px apart; hidden while the sidebar shows only its icons.</summary>
public sealed class SidebarMenuSub : ItemsControl
{
}

/// <summary>shadcn/ui's SidebarMenuSubButton: a 28 px row of 14 px text (12 px with <c>sm</c>), <c>--sidebar-accent</c> under the pointer and while <see cref="IsActive"/>.</summary>
public sealed class SidebarMenuSubButton : Button
{
    /// <summary>Defines <see cref="IsActive"/>.</summary>
    public static readonly StyledProperty<bool> IsActiveProperty = SidebarMenuButton.IsActiveProperty.AddOwner<SidebarMenuSubButton>();

    static SidebarMenuSubButton() => IsActiveProperty.Changed.AddClassHandler<SidebarMenuSubButton>((button, change) => button.PseudoClasses.Set(":active", change.GetNewValue<bool>()));

    /// <summary>Whether the button is the current page's.</summary>
    public bool IsActive
    {
        get => GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarMenuSubButton);
}

/// <summary>
/// shadcn/ui's SidebarMenuAction: a 20 px button at the end of a menu item (a menu's trigger), shown only under the pointer
/// with the <c>show-on-hover</c> class; hidden while the sidebar shows only its icons. With <see cref="Toggles"/> it is
/// its collapsible item's trigger in place of the item's button, as shadcn's <c>CollapsibleTrigger</c> around the action.
/// </summary>
public sealed class SidebarMenuAction : Button
{
    /// <summary>Defines <see cref="Toggles"/>.</summary>
    public static readonly StyledProperty<bool> TogglesProperty = AvaloniaProperty.Register<SidebarMenuAction, bool>(nameof(Toggles));

    static SidebarMenuAction() => TogglesProperty.Changed.AddClassHandler<SidebarMenuAction>((action, _) => (action.Parent as SidebarMenuItem)?.Retoggle());

    /// <summary>Whether the action toggles its collapsible item's sub-menu, and turns a quarter while it is shown, instead of the item's button.</summary>
    public bool Toggles
    {
        get => GetValue(TogglesProperty);
        set => SetValue(TogglesProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarMenuAction);
}

/// <summary>shadcn/ui's SidebarMenuBadge: a 20 px count at the end of a menu item, 12 px medium tabular text.</summary>
public sealed class SidebarMenuBadge : ContentControl
{
}

/// <summary>shadcn/ui's SidebarTrigger: a 28 px ghost button with Lucide's <c>panel-left</c>, named "Toggle Sidebar", that toggles its provider's sidebar (and runs its <c>Command</c>, if any).</summary>
public sealed class SidebarTrigger : Button
{
    static SidebarTrigger() => AutomationProperties.NameProperty.OverrideDefaultValue<SidebarTrigger>("Toggle Sidebar");

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarTrigger);

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        if (HotKey is null && this.FindLogicalAncestorOfType<SidebarProvider>() is { ToggleGesture: { } gesture })
        {
            SetCurrentValue(HotKeyProperty, gesture);
        }
    }

    /// <inheritdoc />
    protected override void OnClick()
    {
        (this.FindAncestorOfType<SidebarProvider>() ?? this.FindLogicalAncestorOfType<SidebarProvider>())?.Toggle();
        base.OnClick();
    }
}

/// <summary>shadcn/ui's SidebarRail: a 16 px strip straddling the sidebar's edge that shows a 2 px <c>--sidebar-border</c> line under the pointer and toggles the sidebar when clicked; it takes no keyboard focus.</summary>
public sealed class SidebarRail : Button
{
    static SidebarRail()
    {
        FocusableProperty.OverrideDefaultValue<SidebarRail>(false);
        AutomationProperties.NameProperty.OverrideDefaultValue<SidebarRail>("Toggle Sidebar");
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarRail);

    /// <inheritdoc />
    protected override void OnClick()
    {
        this.FindLogicalAncestorOfType<SidebarProvider>()?.Toggle();
        base.OnClick();
    }
}

/// <summary>shadcn/ui's SidebarInput: a text box in the sidebar, 32 px high on <c>--background</c> with no shadow at rest.</summary>
public sealed class SidebarInput : TextBox
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarInput);
}

/// <summary>shadcn/ui's SidebarSeparator: a 1 px <c>--sidebar-border</c> line 8 px in from the sidebar's sides.</summary>
public sealed class SidebarSeparator : Separator
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(SidebarSeparator);
}
