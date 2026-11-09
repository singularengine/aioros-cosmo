using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;

namespace Aioros.Cosmo.Chat;

/// <summary>Where a <see cref="TraceLine"/>'s step is, shadcn/ui's <c>status</c>.</summary>
public enum TraceStatus
{
    /// <summary>Done: the usual muted line.</summary>
    Done,

    /// <summary>Running: the title shimmers, as shadcn/ui's.</summary>
    Running,

    /// <summary>Failed: the line in <c>--destructive</c>.</summary>
    Error,

    /// <summary>Not started: the line at 60% opacity.</summary>
    Pending,
}

/// <summary>shadcn/ui's Trace (the radix base in the radix-rhea style): an agent's steps, lines and collapsible steps 8 px apart.</summary>
public sealed class Trace : StackPanel
{
}

/// <summary>
/// shadcn/ui's TraceLine: a step as a 14 px muted line of its 16 px <see cref="Icon"/>, its title (the content, cut short
/// with an ellipsis), its <see cref="Description"/> and a <see cref="Trailing"/> icon, 8 px apart, as wide as they are; its
/// <see cref="Status"/> is a pseudo-class (<c>:running</c>, <c>:error</c>, <c>:pending</c>, <c>:done</c>).
/// </summary>
[PseudoClasses(":done", ":running", ":error", ":pending")]
public sealed class TraceLine : ContentControl
{
    /// <summary>Defines the <see cref="Icon"/> property.</summary>
    public static readonly StyledProperty<object?> IconProperty = AvaloniaProperty.Register<TraceLine, object?>(nameof(Icon));

    /// <summary>Defines the <see cref="Description"/> property.</summary>
    public static readonly StyledProperty<object?> DescriptionProperty = AvaloniaProperty.Register<TraceLine, object?>(nameof(Description));

    /// <summary>Defines the <see cref="Trailing"/> property.</summary>
    public static readonly StyledProperty<object?> TrailingProperty = AvaloniaProperty.Register<TraceLine, object?>(nameof(Trailing));

    /// <summary>Defines the <see cref="Status"/> property.</summary>
    public static readonly StyledProperty<TraceStatus> StatusProperty = AvaloniaProperty.Register<TraceLine, TraceStatus>(nameof(Status));

    /// <summary>Starts in the <c>:done</c> state.</summary>
    public TraceLine() => Mark(TraceStatus.Done);

    /// <summary>The 16 px icon before the title.</summary>
    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    /// <summary>The detail after the title (a count, a duration), never cut short.</summary>
    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    /// <summary>A 16 px icon at the line's end, 4 px after the title.</summary>
    public object? Trailing
    {
        get => GetValue(TrailingProperty);
        set => SetValue(TrailingProperty, value);
    }

    /// <summary>Where the step is.</summary>
    public TraceStatus Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    /// <summary>
    /// Measures the line as wide as its parts, or, when they do not fit, as wide as its row with the title cut short, as a
    /// <c>w-fit max-w-full</c> row of a shrinking title does.
    /// </summary>
    /// <param name="availableSize">The space the trace gives.</param>
    /// <returns>The line's size.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        if (double.IsInfinity(availableSize.Width))
        {
            return base.MeasureOverride(availableSize);
        }

        var natural = base.MeasureOverride(availableSize.WithWidth(double.PositiveInfinity));
        var size = base.MeasureOverride(availableSize);
        return natural.Width > availableSize.Width ? size.WithWidth(availableSize.Width) : size;
    }

    /// <summary>Marks the status as a pseudo-class.</summary>
    /// <param name="change">The changed property.</param>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == StatusProperty)
        {
            Mark(change.GetNewValue<TraceStatus>());
        }
    }

    /// <summary>Marks the step whose trigger the line is, as shadcn/ui's trigger turns <c>--destructive</c> around an error line.</summary>
    /// <param name="e">The event.</param>
    protected override void OnAttachedToLogicalTree(Avalonia.LogicalTree.LogicalTreeAttachmentEventArgs e)
    {
        base.OnAttachedToLogicalTree(e);
        Mark(Status);
    }

    private void Mark(TraceStatus status)
    {
        foreach (var each in Enum.GetValues<TraceStatus>())
        {
            PseudoClasses.Set(":" + each.ToString().ToLowerInvariant(), each == status);
        }

        if (Avalonia.LogicalTree.LogicalExtensions.FindLogicalAncestorOfType<Expander>(this) is { } step && ReferenceEquals(step.Header, this))
        {
            ((IPseudoClasses)step.Classes).Set(":error", status == TraceStatus.Error);
        }
    }
}
