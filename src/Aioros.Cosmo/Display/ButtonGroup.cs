using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's ButtonGroup: buttons, inputs, selects and texts joined in a row (or a column with <c>Orientation</c>), the
/// inner corners square and the shared borders single; nested groups sit 8 px apart.
/// </summary>
public sealed class ButtonGroup : StackPanel
{
    static ButtonGroup() => AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<ButtonGroup>(AutomationControlType.Group);
}

/// <summary>shadcn/ui's ButtonGroupText: a label joined to its group, on <c>--muted</c>.</summary>
public sealed class ButtonGroupText : ContentControl
{
}
