using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Display;

/// <summary>When a part inside a <see cref="Collapsible"/> is shown.</summary>
public enum CollapsibleShows
{
    /// <summary>Always, the part's own visibility.</summary>
    Always,

    /// <summary>Only while the collapsible is open (Radix's <c>CollapsibleContent</c>, a "Show less" label).</summary>
    Open,

    /// <summary>Only while the collapsible is closed (a preview, a "Show more" label).</summary>
    Closed,
}

/// <summary>
/// Radix's Collapsible as a panel: its parts stacked, the ones marked with <see cref="ShowsProperty"/> shown only while
/// it is open or only while it is closed, switched by a <see cref="CollapsibleTrigger"/> inside it (shadcn/ui's bubble
/// that shows more of a long message, its preview swapped for the whole text). <see cref="IsOpen"/> binds two way; the
/// <c>:open</c> pseudo-class turns the trigger's chevron. On the web the browser switches it with no request
/// (Aioros's <c>a:Web.Flag</c>), both states written.
/// </summary>
public sealed class Collapsible : StackPanel
{
    /// <summary>Defines <see cref="IsOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsOpenProperty = AvaloniaProperty.Register<Collapsible, bool>(nameof(IsOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>When a part inside a collapsible is shown.</summary>
    public static readonly AttachedProperty<CollapsibleShows> ShowsProperty = AvaloniaProperty.RegisterAttached<Collapsible, Control, CollapsibleShows>("Shows");

    static Collapsible()
    {
        IsOpenProperty.Changed.AddClassHandler<Collapsible>((collapsible, _) => collapsible.Apply());
        ShowsProperty.Changed.AddClassHandler<Control>((part, _) =>
        {
            part.AttachedToLogicalTree -= Attached;
            part.AttachedToLogicalTree += Attached;
            part.AttachedToVisualTree -= Shown;
            part.AttachedToVisualTree += Shown;
            part.FindLogicalAncestorOfType<Collapsible>()?.Apply();
        });
    }

    /// <summary>Creates a closed collapsible.</summary>
    public Collapsible() => PseudoClasses.Set(":open", false);

    /// <summary>Whether the collapsible shows its <see cref="CollapsibleShows.Open"/> parts.</summary>
    public bool IsOpen
    {
        get => GetValue(IsOpenProperty);
        set => SetValue(IsOpenProperty, value);
    }

    /// <summary>Gets <see cref="ShowsProperty"/>.</summary>
    public static CollapsibleShows GetShows(Control part) => part.GetValue(ShowsProperty);

    /// <summary>Sets <see cref="ShowsProperty"/>.</summary>
    public static void SetShows(Control part, CollapsibleShows value) => part.SetValue(ShowsProperty, value);

    /// <inheritdoc />
    protected override void OnAttachedToLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Apply();
    }

    private static void Attached(object? sender, LogicalTreeAttachmentEventArgs e) => ((Control)sender!).FindLogicalAncestorOfType<Collapsible>()?.Apply();

    private static void Shown(object? sender, VisualTreeAttachmentEventArgs e) => ((Control)sender!).FindLogicalAncestorOfType<Collapsible>()?.Apply();

    internal void Toggle() => SetCurrentValue(IsOpenProperty, !IsOpen);

    private void Apply()
    {
        PseudoClasses.Set(":open", IsOpen);
        foreach (var part in this.GetLogicalDescendants().Concat(this.GetVisualDescendants()).OfType<Control>().Distinct())
        {
            switch (GetShows(part))
            {
                case CollapsibleShows.Open:
                    part.SetValue(IsVisibleProperty, IsOpen, BindingPriority.Style);
                    break;
                case CollapsibleShows.Closed:
                    part.SetValue(IsVisibleProperty, !IsOpen, BindingPriority.Style);
                    break;
            }
        }
    }
}

/// <summary>Radix's CollapsibleTrigger: a button that opens and closes the <see cref="Collapsible"/> it is in, <c>aria-expanded</c> as it is.</summary>
public sealed class CollapsibleTrigger : Button
{
    /// <inheritdoc />
    protected override void OnClick()
    {
        base.OnClick();
        this.FindLogicalAncestorOfType<Collapsible>()?.Toggle();
    }
}
