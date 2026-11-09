using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Navigation;

/// <summary>The side a <see cref="Sidebar"/> sits on.</summary>
public enum SidebarSide
{
    /// <summary>The start of the window (shadcn's default).</summary>
    Left,

    /// <summary>The end of the window.</summary>
    Right,
}

/// <summary>A <see cref="Sidebar"/>'s look (shadcn's <c>variant</c>).</summary>
public enum SidebarVariant
{
    /// <summary>Flush against the window's edge with a border (shadcn's default).</summary>
    Sidebar,

    /// <summary>A rounded, bordered, shadowed panel 8 px in.</summary>
    Floating,

    /// <summary>Flush, with the <see cref="SidebarInset"/> drawn as a rounded card on the sidebar's color.</summary>
    Inset,
}

/// <summary>How a <see cref="Sidebar"/> collapses (shadcn's <c>collapsible</c>).</summary>
public enum SidebarCollapsible
{
    /// <summary>It slides out of the window (shadcn's default).</summary>
    Offcanvas,

    /// <summary>It narrows to its icons, 48 px.</summary>
    Icon,

    /// <summary>It does not collapse.</summary>
    None,
}

/// <summary>
/// shadcn/ui's Sidebar: a 256 px column (<c>--sidebar</c>, a 1 px <c>--sidebar-border</c> edge) laying its
/// <see cref="SidebarHeader"/> at the top, its <see cref="SidebarContent"/> in the middle and its
/// <see cref="SidebarFooter"/> at the bottom, with a <see cref="SidebarRail"/> on its edge. Collapsed by its
/// <see cref="SidebarProvider"/>, it slides away (<see cref="SidebarCollapsible.Offcanvas"/>) or narrows to 48 px of icons
/// (<see cref="SidebarCollapsible.Icon"/>, its labels, sub-menus and actions hidden, its buttons' tooltips shown); on
/// narrow windows it is a 288 px sheet over a 50% mask, closed by Escape or a click on the mask.
/// </summary>
public sealed class Sidebar : ItemsControl
{
    /// <summary>Defines <see cref="Side"/>.</summary>
    public static readonly StyledProperty<SidebarSide> SideProperty = AvaloniaProperty.Register<Sidebar, SidebarSide>(nameof(Side));

    /// <summary>Defines <see cref="Variant"/>.</summary>
    public static readonly StyledProperty<SidebarVariant> VariantProperty = AvaloniaProperty.Register<Sidebar, SidebarVariant>(nameof(Variant));

    /// <summary>Defines <see cref="Collapsible"/>.</summary>
    public static readonly StyledProperty<SidebarCollapsible> CollapsibleProperty = AvaloniaProperty.Register<Sidebar, SidebarCollapsible>(nameof(Collapsible));

    /// <summary>Defines <see cref="ExpandedWidth"/>.</summary>
    public static readonly StyledProperty<double> ExpandedWidthProperty = AvaloniaProperty.Register<Sidebar, double>(nameof(ExpandedWidth), 256);

    static Sidebar()
    {
        AutomationProperties.LandmarkTypeProperty.OverrideDefaultValue<Sidebar>(AutomationLandmarkType.Navigation);
        foreach (var property in new AvaloniaProperty[] { SideProperty, VariantProperty, CollapsibleProperty })
        {
            property.Changed.AddClassHandler<Sidebar>((sidebar, _) => sidebar.Provider?.Mark());
        }
    }

    /// <summary>The side the sidebar sits on.</summary>
    public SidebarSide Side
    {
        get => GetValue(SideProperty);
        set => SetValue(SideProperty, value);
    }

    /// <summary>The sidebar's look.</summary>
    public SidebarVariant Variant
    {
        get => GetValue(VariantProperty);
        set => SetValue(VariantProperty, value);
    }

    /// <summary>How the sidebar collapses.</summary>
    public SidebarCollapsible Collapsible
    {
        get => GetValue(CollapsibleProperty);
        set => SetValue(CollapsibleProperty, value);
    }

    /// <summary>The sidebar's width while it is open, 256 px by default (shadcn/ui's <c>--sidebar-width</c> on its provider).</summary>
    public double ExpandedWidth
    {
        get => GetValue(ExpandedWidthProperty);
        set => SetValue(ExpandedWidthProperty, value);
    }

    /// <summary>Whether the sidebar shows only its icons (collapsed with <see cref="SidebarCollapsible.Icon"/> on a wide window).</summary>
    public bool IsIconOnly { get; private set; }

    internal event EventHandler? StateChanged;

    private SidebarProvider? Provider => Parent as SidebarProvider;

    internal void Follow(bool open, bool mobile, bool mobileOpen)
    {
        mobile = mobile && Collapsible != SidebarCollapsible.None;
        var collapsed = !open && Collapsible != SidebarCollapsible.None && !mobile;
        IsIconOnly = collapsed && Collapsible == SidebarCollapsible.Icon;
        PseudoClasses.Set(":collapsed", collapsed);
        PseudoClasses.Set(":icon", IsIconOnly);
        PseudoClasses.Set(":offcanvas", collapsed && Collapsible == SidebarCollapsible.Offcanvas);
        PseudoClasses.Set(":mobile", mobile);
        PseudoClasses.Set(":mobile-open", mobile && mobileOpen);
        PseudoClasses.Set(":floating", Variant == SidebarVariant.Floating && !mobile);
        PseudoClasses.Set(":inset", Variant == SidebarVariant.Inset && !mobile);
        PseudoClasses.Set(":right", Side == SidebarSide.Right);
        PseudoClasses.Set(":fixed", Collapsible == SidebarCollapsible.None);
        IsHitTestVisible = !mobile || mobileOpen;
        ZIndex = mobile ? 1 : 0;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (e.NameScope.Find<Border>("PART_Mask") is { } mask)
        {
            mask.PointerPressed += (_, args) =>
            {
                args.Handled = true;
                Provider?.SetCurrentValue(SidebarProvider.IsMobileOpenProperty, false);
            };
        }

        Provider?.Mark();
    }
}

/// <summary>
/// shadcn/ui's SidebarInset: the main area beside the <see cref="Sidebar"/>, on <c>--background</c>; with the
/// <see cref="SidebarVariant.Inset"/> sidebar it is a card 8 px in from the window (and from the collapsed sidebar), rounded
/// <c>xl</c> with <c>shadow-sm</c>. It is the page's main landmark.
/// </summary>
public sealed class SidebarInset : ContentControl
{
    static SidebarInset() => AutomationProperties.LandmarkTypeProperty.OverrideDefaultValue<SidebarInset>(AutomationLandmarkType.Main);

    internal void Follow(bool inset, bool collapsed)
    {
        PseudoClasses.Set(":inset", inset);
        PseudoClasses.Set(":collapsed", collapsed);
    }
}
