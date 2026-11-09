using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Aioros.Cosmo.Layout;

/// <summary>
/// shadcn/ui's ResizableHandle (react-resizable-panels' <c>Separator</c>): Avalonia's <see cref="GridSplitter"/> drawn as
/// a 1 px <c>--border</c> line between two <see cref="ResizablePanel"/>s, its grip (<see cref="WithHandle"/>) a 12 by 16 px
/// bordered box with Lucide's <c>grip-vertical</c>. It resizes its neighbors when dragged, and with the keyboard as the
/// library does: the arrows along the group by 5%, Home and End to the limits, Enter collapses a collapsible panel and F6
/// moves to the next handle (Shift+F6 to the previous).
/// </summary>
public sealed class ResizableHandle : GridSplitter
{
    /// <summary>Defines <see cref="WithHandle"/>.</summary>
    public static readonly StyledProperty<bool> WithHandleProperty = AvaloniaProperty.Register<ResizableHandle, bool>(nameof(WithHandle));

    /// <summary>Defines <see cref="Orientation"/>.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ResizableHandle, Orientation>(nameof(Orientation), Orientation.Horizontal);

    static ResizableHandle()
    {
        FocusableProperty.OverrideDefaultValue<ResizableHandle>(true);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<ResizableHandle>(AutomationControlType.Separator);
    }

    /// <summary>Whether the grip box is drawn on the line (shadcn's <c>withHandle</c>).</summary>
    public bool WithHandle
    {
        get => GetValue(WithHandleProperty);
        set => SetValue(WithHandleProperty, value);
    }

    /// <summary>The orientation of the group the handle divides, set by the group.</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(ResizableHandle);

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == OrientationProperty)
        {
            PseudoClasses.Set(":vertical", Orientation == Orientation.Vertical);
        }
    }

    /// <inheritdoc />
    protected override void OnDragCompleted(Avalonia.Input.VectorEventArgs e)
    {
        base.OnDragCompleted(e);
        (Parent as ResizablePanelGroup)?.Synchronize();
    }

    /// <inheritdoc />
    protected override void OnKeyDown(KeyEventArgs e)
    {
        if (Parent is not ResizablePanelGroup group)
        {
            base.OnKeyDown(e);
            return;
        }

        var horizontal = Orientation == Orientation.Horizontal;
        double? delta = e.Key switch
        {
            Key.Left when horizontal => -5,
            Key.Right when horizontal => 5,
            Key.Up when !horizontal => -5,
            Key.Down when !horizontal => 5,
            Key.Home => -100,
            Key.End => 100,
            _ => null,
        };
        if (delta is { } step)
        {
            group.Resize(this, step);
            e.Handled = true;
            return;
        }

        if (e.Key == Key.Enter)
        {
            group.Collapse(this);
            e.Handled = true;
        }
        else if (e.Key == Key.F6)
        {
            var handles = group.Handles;
            var index = handles.ToList().IndexOf(this);
            var next = (e.KeyModifiers & KeyModifiers.Shift) != 0 ? (index - 1 + handles.Count) % handles.Count : (index + 1) % handles.Count;
            handles[next].Focus(NavigationMethod.Tab);
            e.Handled = true;
        }
        else if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
        {
            e.Handled = true;
        }
    }
}
