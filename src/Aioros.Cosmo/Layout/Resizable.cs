using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Layout;

namespace Aioros.Cosmo.Layout;

/// <summary>
/// shadcn/ui's ResizablePanelGroup (react-resizable-panels' <c>Group</c>): a <see cref="Grid"/> laying its
/// <see cref="ResizablePanel"/>s side by side (or stacked, <see cref="Orientation"/>) with a 1 px
/// <see cref="ResizableHandle"/> between them. Each panel's share is a percentage of the group (its
/// <see cref="ResizablePanel.Size"/>), kept within its <see cref="ResizablePanel.MinSize"/> and
/// <see cref="ResizablePanel.MaxSize"/> as the group resizes.
/// </summary>
public sealed class ResizablePanelGroup : Grid
{
    /// <summary>Defines <see cref="Orientation"/>.</summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<ResizablePanelGroup, Orientation>(nameof(Orientation), Orientation.Horizontal);

    private bool _arranging;

    static ResizablePanelGroup()
    {
        OrientationProperty.Changed.AddClassHandler<ResizablePanelGroup>((group, _) => group.Rebuild());
        BoundsProperty.Changed.AddClassHandler<ResizablePanelGroup>((group, _) => group.Constrain());
    }

    /// <summary>Creates a horizontal group.</summary>
    public ResizablePanelGroup() => Children.CollectionChanged += (_, _) => Rebuild();

    /// <summary>Whether the panels lie side by side (<c>horizontal</c>, the default) or are stacked (<c>vertical</c>).</summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>The panels of the group, in order.</summary>
    public IReadOnlyList<ResizablePanel> Panels => [.. Children.OfType<ResizablePanel>()];

    /// <summary>The handles of the group, in order.</summary>
    public IReadOnlyList<ResizableHandle> Handles => [.. Children.OfType<ResizableHandle>()];

    private double Available => Math.Max(0, (Orientation == Orientation.Horizontal ? Bounds.Width : Bounds.Height) - Handles.Sum(Thickness));

    private static double Thickness(ResizableHandle handle) => handle.Orientation == Orientation.Horizontal ? handle.Bounds.Width : handle.Bounds.Height;

    /// <summary>Moves the boundary after <paramref name="handle"/> by <paramref name="delta"/> percent of the group, within both panels' limits.</summary>
    public void Resize(ResizableHandle handle, double delta)
    {
        if (Neighbors(handle) is not { } pair)
        {
            return;
        }

        var (before, after) = pair;
        var total = before.Size + after.Size;
        var low = Math.Max(before.MinSize, total - after.MaxSize);
        var high = Math.Min(before.MaxSize, total - after.MinSize);
        var size = Math.Clamp(before.Size + delta, low, Math.Max(low, high));
        Apply(before, size);
        Apply(after, total - size);
    }

    /// <summary>Collapses the panel before <paramref name="handle"/> to its collapsed size, or expands it back (Enter).</summary>
    public void Collapse(ResizableHandle handle)
    {
        if (Neighbors(handle) is not { } pair || !pair.Before.IsCollapsible)
        {
            return;
        }

        var (before, after) = pair;
        var collapsed = Math.Abs(before.Size - before.CollapsedSize) < 0.01;
        var target = collapsed ? Math.Max(before.LastExpandedSize, before.MinSize) : before.CollapsedSize;
        if (!collapsed)
        {
            before.LastExpandedSize = before.Size;
        }

        var delta = target - before.Size;
        Apply(before, target);
        Apply(after, after.Size - delta);
    }

    internal void Synchronize()
    {
        var available = Available;
        if (available <= 0)
        {
            return;
        }

        foreach (var panel in Panels)
        {
            var length = Orientation == Orientation.Horizontal ? panel.Bounds.Width : panel.Bounds.Height;
            Apply(panel, length / available * 100);
        }
    }

    internal void PanelChanged(ResizablePanel panel, AvaloniaProperty property)
    {
        if (property == ResizablePanel.DefaultSizeProperty)
        {
            Rebuild();
            return;
        }

        if (!_arranging && property == ResizablePanel.SizeProperty)
        {
            panel.Assigned = false;
            Balance(panel);
        }

        if (!_arranging && Definition(panel) is { } definition)
        {
            Star(definition, panel.Size);
            Constrain();
            Hint();
        }
    }

    /// <summary>
    /// States the group's rules on the web, where Aioros's split module moves the handles: shadcn's 5% for each arrow
    /// (<c>a:Web.ResizeStep</c>), what Enter collapses a collapsible panel to (<c>a:Web.CollapsedSize</c>) and each
    /// panel's limits on its definition (<c>a:Web.MinSize</c>, <c>a:Web.MaxSize</c>); a handle next to a panel whose
    /// <see cref="ResizablePanel.Size"/> is bound posts its position 150 ms after the visitor stops (<c>a:Web.Delay</c>).
    /// </summary>
    private void Hint()
    {
        for (var index = 0; index < Children.Count; index++)
        {
            switch (Children[index])
            {
                case ResizableHandle handle:
                    Web.SetResizeStep(handle, 5);
                    Web.SetCollapsedSize(handle, Neighbors(handle) is { Before.IsCollapsible: true } pair ? pair.Before.CollapsedSize : double.NaN);
                    Web.SetDelay(handle, Neighbors(handle) is { } sides && (Bound(sides.Before) || Bound(sides.After)) ? 150 : double.NaN);
                    break;
                case ResizablePanel panel when Definition(panel) is { } definition:
                    Web.SetMinSize(definition, panel.MinSize);
                    Web.SetMaxSize(definition, panel.MaxSize);
                    break;
            }
        }
    }

    private static bool Bound(ResizablePanel panel) => BindingOperations.GetBindingExpressionBase(panel, ResizablePanel.SizeProperty) is not null;

    private void Balance(ResizablePanel changed)
    {
        var panels = Panels;
        var index = panels.ToList().IndexOf(changed);
        if (index < 0 || panels.Count < 2 || !double.IsFinite(changed.Size))
        {
            return;
        }

        var neighbor = index + 1 < panels.Count ? panels[index + 1] : panels[index - 1];
        var rest = panels.Where(panel => panel != changed && panel != neighbor).Sum(panel => panel.Size);
        _arranging = true;
        neighbor.SetCurrentValue(ResizablePanel.SizeProperty, Math.Round(Math.Max(0, 100 - rest - changed.Size), 3));
        _arranging = false;
        if (Definition(neighbor) is { } definition)
        {
            Star(definition, neighbor.Size);
        }
    }

    private (ResizablePanel Before, ResizablePanel After)? Neighbors(ResizableHandle handle)
    {
        var index = Children.IndexOf(handle);
        var before = Children.Take(Math.Max(index, 0)).OfType<ResizablePanel>().LastOrDefault();
        var after = Children.Skip(index + 1).OfType<ResizablePanel>().FirstOrDefault();
        return index < 0 || before is null || after is null ? null : (before, after);
    }

    private void Apply(ResizablePanel panel, double size)
    {
        _arranging = true;
        panel.SetCurrentValue(ResizablePanel.SizeProperty, Math.Round(size, 3));
        panel.Assigned = false;
        _arranging = false;
        if (Definition(panel) is { } definition)
        {
            Star(definition, panel.Size);
        }
    }

    private DefinitionBase? Definition(ResizablePanel panel)
    {
        var index = Children.IndexOf(panel);
        if (index < 0)
        {
            return null;
        }

        return Orientation == Orientation.Horizontal
            ? index < ColumnDefinitions.Count ? ColumnDefinitions[index] : null
            : index < RowDefinitions.Count ? RowDefinitions[index] : null;
    }

    private static void Star(DefinitionBase definition, double size)
    {
        if (!double.IsFinite(size))
        {
            return;
        }

        var length = new GridLength(Math.Max(size, 0.0001), GridUnitType.Star);
        if (definition is ColumnDefinition column)
        {
            column.Width = length;
        }
        else if (definition is RowDefinition row)
        {
            row.Height = length;
        }
    }

    private void Rebuild()
    {
        var horizontal = Orientation == Orientation.Horizontal;
        var panels = Panels;
        var unset = panels.Count(panel => double.IsNaN(panel.DefaultSize));
        var given = panels.Where(panel => !double.IsNaN(panel.DefaultSize)).Sum(panel => panel.DefaultSize);
        var share = unset == 0 ? 0 : Math.Max(0, 100 - given) / unset;
        ColumnDefinitions.Clear();
        RowDefinitions.Clear();
        for (var index = 0; index < Children.Count; index++)
        {
            var child = Children[index];
            var length = Prepare(child, share);
            if (horizontal)
            {
                ColumnDefinitions.Add(new ColumnDefinition(length));
                SetColumn(child, index);
                SetRow(child, 0);
            }
            else
            {
                RowDefinitions.Add(new RowDefinition(length));
                SetRow(child, index);
                SetColumn(child, 0);
            }
        }

        Constrain();
        Hint();
    }

    private GridLength Prepare(Control child, double share)
    {
        if (child is ResizablePanel panel)
        {
            if (!panel.IsSet(ResizablePanel.SizeProperty) || panel.Assigned)
            {
                _arranging = true;
                panel.SetCurrentValue(ResizablePanel.SizeProperty, double.IsNaN(panel.DefaultSize) ? share : panel.DefaultSize);
                panel.Assigned = true;
                _arranging = false;
            }

            return new GridLength(double.IsFinite(panel.Size) ? Math.Max(panel.Size, 0.0001) : 1, GridUnitType.Star);
        }

        if (child is ResizableHandle handle)
        {
            handle.Orientation = Orientation;
            handle.ResizeDirection = Orientation == Orientation.Horizontal ? GridResizeDirection.Columns : GridResizeDirection.Rows;
            handle.ZIndex = 1;
        }

        return GridLength.Auto;
    }

    private void Constrain()
    {
        var available = Available;
        if (available <= 0)
        {
            return;
        }

        foreach (var panel in Panels)
        {
            if (Definition(panel) is ColumnDefinition column)
            {
                column.MinWidth = available * panel.MinSize / 100;
                column.MaxWidth = available * panel.MaxSize / 100;
            }
            else if (Definition(panel) is RowDefinition row)
            {
                row.MinHeight = available * panel.MinSize / 100;
                row.MaxHeight = available * panel.MaxSize / 100;
            }
        }
    }
}

/// <summary>
/// shadcn/ui's ResizablePanel: a panel of a <see cref="ResizablePanelGroup"/> whose <see cref="Size"/> (two way) is its
/// percentage of the group, starting at <see cref="DefaultSize"/> and kept within <see cref="MinSize"/> and
/// <see cref="MaxSize"/>; an <see cref="IsCollapsible"/> panel collapses to <see cref="CollapsedSize"/> with Enter on the
/// handle after it.
/// </summary>
public sealed class ResizablePanel : ContentControl
{
    /// <summary>Defines <see cref="DefaultSize"/>.</summary>
    public static readonly StyledProperty<double> DefaultSizeProperty = AvaloniaProperty.Register<ResizablePanel, double>(nameof(DefaultSize), double.NaN);

    /// <summary>Defines <see cref="Size"/>.</summary>
    public static readonly StyledProperty<double> SizeProperty = AvaloniaProperty.Register<ResizablePanel, double>(nameof(Size), double.NaN, defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="MinSize"/>.</summary>
    public static readonly StyledProperty<double> MinSizeProperty = AvaloniaProperty.Register<ResizablePanel, double>(nameof(MinSize));

    /// <summary>Defines <see cref="MaxSize"/>.</summary>
    public static readonly StyledProperty<double> MaxSizeProperty = AvaloniaProperty.Register<ResizablePanel, double>(nameof(MaxSize), 100);

    /// <summary>Defines <see cref="IsCollapsible"/>.</summary>
    public static readonly StyledProperty<bool> IsCollapsibleProperty = AvaloniaProperty.Register<ResizablePanel, bool>(nameof(IsCollapsible));

    /// <summary>Defines <see cref="CollapsedSize"/>.</summary>
    public static readonly StyledProperty<double> CollapsedSizeProperty = AvaloniaProperty.Register<ResizablePanel, double>(nameof(CollapsedSize));

    static ResizablePanel()
    {
        foreach (var property in new AvaloniaProperty[] { SizeProperty, MinSizeProperty, MaxSizeProperty, DefaultSizeProperty, IsCollapsibleProperty, CollapsedSizeProperty })
        {
            property.Changed.AddClassHandler<ResizablePanel>((panel, change) => (panel.Parent as ResizablePanelGroup)?.PanelChanged(panel, change.Property));
        }
    }

    /// <summary>The panel's starting percentage of the group (react-resizable-panels' <c>defaultSize</c>); unset panels share what is left.</summary>
    public double DefaultSize
    {
        get => GetValue(DefaultSizeProperty);
        set => SetValue(DefaultSizeProperty, value);
    }

    /// <summary>The panel's percentage of the group, two way: it follows a drag and the keys, and resizes the panel when set.</summary>
    public double Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }

    /// <summary>The smallest percentage of the group the panel keeps (<c>minSize</c>, 0).</summary>
    public double MinSize
    {
        get => GetValue(MinSizeProperty);
        set => SetValue(MinSizeProperty, value);
    }

    /// <summary>The largest percentage of the group the panel takes (<c>maxSize</c>, 100).</summary>
    public double MaxSize
    {
        get => GetValue(MaxSizeProperty);
        set => SetValue(MaxSizeProperty, value);
    }

    /// <summary>Whether Enter on the handle after the panel collapses it to <see cref="CollapsedSize"/> (<c>collapsible</c>).</summary>
    public bool IsCollapsible
    {
        get => GetValue(IsCollapsibleProperty);
        set => SetValue(IsCollapsibleProperty, value);
    }

    /// <summary>The percentage a collapsed panel keeps (<c>collapsedSize</c>, 0).</summary>
    public double CollapsedSize
    {
        get => GetValue(CollapsedSizeProperty);
        set => SetValue(CollapsedSizeProperty, value);
    }

    internal double LastExpandedSize { get; set; }

    internal bool Assigned { get; set; }
}
