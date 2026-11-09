using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;

namespace Aioros.Cosmo.Overlays;

/// <summary>
/// Radix's defaults for Avalonia's overlays, as shadcn/ui sets them: flyouts and menus 4 px from their target (Radix's
/// <c>sideOffset</c>), tooltips above their target with no delay (shadcn's <c>TooltipProvider</c>, <c>delayDuration</c> 0)
/// and Radix's 300 ms skip delay between tooltips. Avalonia registers the tooltip's attached properties on <see cref="Control"/>,
/// whose defaults cannot be overridden, so they are overridden on the controls that hold tooltips (templated controls,
/// decorators, texts, panels, images, shapes and view boxes).
/// </summary>
public static class OverlayDefaults
{
    private static bool _registered;

    /// <summary>Overrides the defaults once; <c>CosmoTheme</c> calls it. A value set on a flyout or a control still wins.</summary>
    public static void Register()
    {
        if (_registered)
        {
            return;
        }

        _registered = true;
        PopupFlyoutBase.VerticalOffsetProperty.OverrideDefaultValue<Flyout>(4);
        PopupFlyoutBase.VerticalOffsetProperty.OverrideDefaultValue<MenuFlyout>(4);
        Tips<TemplatedControl>();
        Tips<Decorator>();
        Tips<TextBlock>();
        Tips<Panel>();
        Tips<Image>();
        Tips<Shape>();
        Tips<Viewbox>();
        Popover.Register();
        TooltipKeyboard.Register();
        MenuKeyboard.Register();
    }

    private static void Tips<T>()
        where T : Control
    {
        ToolTip.PlacementProperty.OverrideDefaultValue<T>(PlacementMode.Top);
        ToolTip.VerticalOffsetProperty.OverrideDefaultValue<T>(0);
        ToolTip.ShowDelayProperty.OverrideDefaultValue<T>(0);
        ToolTip.BetweenShowDelayProperty.OverrideDefaultValue<T>(300);
    }
}
