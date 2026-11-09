using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Search;

/// <summary>
/// shadcn/ui's Command (cmdk 1.1.1): a search input over a list of <see cref="CommandGroup"/>s, <see cref="CommandItem"/>s
/// and <see cref="CommandSeparator"/>s. Typing filters and ranks the items with <see cref="Filter"/> (cmdk's
/// <c>command-score</c> by default), hides the groups left empty and the separators, shows <see cref="Empty"/> when nothing
/// matches and selects the first item; Up and Down (Ctrl+N, Ctrl+J, Ctrl+P, Ctrl+K), Home and End, Alt with the arrows
/// between groups and Ctrl or Meta with the arrows to the ends move the selection, the pointer selects what it passes, and
/// Enter chooses the selected item. <see cref="Text"/> and <see cref="SelectedValue"/> bind two way.
/// </summary>
[TemplatePart("PART_Input", typeof(TextBox))]
public sealed class Command : ItemsControl
{
    /// <summary>Defines <see cref="Text"/>.</summary>
    public static readonly StyledProperty<string?> TextProperty = AvaloniaProperty.Register<Command, string?>(nameof(Text), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="SelectedValue"/>.</summary>
    public static readonly StyledProperty<string?> SelectedValueProperty = AvaloniaProperty.Register<Command, string?>(nameof(SelectedValue), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="Placeholder"/>.</summary>
    public static readonly StyledProperty<string?> PlaceholderProperty = AvaloniaProperty.Register<Command, string?>(nameof(Placeholder), "Type a command or search...");

    /// <summary>Defines <see cref="Empty"/>.</summary>
    public static readonly StyledProperty<object?> EmptyProperty = AvaloniaProperty.Register<Command, object?>(nameof(Empty), "No results found.");

    /// <summary>Defines <see cref="ShouldFilter"/>.</summary>
    public static readonly StyledProperty<bool> ShouldFilterProperty = AvaloniaProperty.Register<Command, bool>(nameof(ShouldFilter), true);

    /// <summary>Defines <see cref="IsLoop"/>.</summary>
    public static readonly StyledProperty<bool> IsLoopProperty = AvaloniaProperty.Register<Command, bool>(nameof(IsLoop));

    /// <summary>Defines <see cref="IsEmpty"/>.</summary>
    public static readonly DirectProperty<Command, bool> IsEmptyProperty = AvaloniaProperty.RegisterDirect<Command, bool>(nameof(IsEmpty), command => command.IsEmpty);

    internal static readonly AttachedProperty<double> RankProperty = AvaloniaProperty.RegisterAttached<Command, Control, double>("Rank");

    private bool _empty;
    private bool _refiltering;

    static Command()
    {
        TextProperty.Changed.AddClassHandler<Command>((command, _) => command.Refilter());
        ShouldFilterProperty.Changed.AddClassHandler<Command>((command, _) => command.Refilter());
        SelectedValueProperty.Changed.AddClassHandler<Command>((command, change) => command.Follow(change.GetNewValue<string?>()));
    }

    /// <summary>Hooks the list's keys on the input, before the text box sees them.</summary>
    public Command() => AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);

    /// <summary>The typed search (cmdk's <c>search</c>), two way.</summary>
    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>The selected item's value (cmdk's <c>value</c> and <c>onValueChange</c>), two way.</summary>
    public string? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    /// <summary>The input's placeholder.</summary>
    public string? Placeholder
    {
        get => GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    /// <summary>What the list shows when no item matches (shadcn's <c>CommandEmpty</c>).</summary>
    public object? Empty
    {
        get => GetValue(EmptyProperty);
        set => SetValue(EmptyProperty, value);
    }

    /// <summary>Whether typing filters the items (cmdk's <c>shouldFilter</c>); off, the model filters them itself.</summary>
    public bool ShouldFilter
    {
        get => GetValue(ShouldFilterProperty);
        set => SetValue(ShouldFilterProperty, value);
    }

    /// <summary>Whether the arrows wrap around the ends of the list (cmdk's <c>loop</c>).</summary>
    public bool IsLoop
    {
        get => GetValue(IsLoopProperty);
        set => SetValue(IsLoopProperty, value);
    }

    /// <summary>Whether no item matches the search.</summary>
    public bool IsEmpty
    {
        get => _empty;
        private set => SetAndRaise(IsEmptyProperty, ref _empty, value);
    }

    /// <summary>How an item's value and keywords score against the search (cmdk's <c>filter</c>): 0 hides the item.</summary>
    public Func<string, string, IReadOnlyList<string>?, double> Filter { get; set; } = CommandScore.Score;

    /// <summary>The selected item, if any.</summary>
    public CommandItem? SelectedItem => Visible().FirstOrDefault(item => item.IsSelected);

    /// <summary>The items the list shows, in the order it shows them.</summary>
    public IReadOnlyList<CommandItem> Visible() => [.. Ordered(this).SelectMany(entry => entry switch
    {
        CommandGroup group when group.IsVisible => Ordered(group).OfType<CommandItem>().Where(item => item.IsVisible),
        CommandItem item when item.IsVisible => [item],
        _ => [],
    })];

    /// <summary>Selects an item, as the pointer passing over it does.</summary>
    public void Select(CommandItem? item)
    {
        foreach (var each in All())
        {
            each.Select(each == item);
        }

        SetCurrentValue(SelectedValueProperty, item?.EffectiveValue);
    }

    internal void ItemAdded()
    {
        if (!_refiltering)
        {
            Refilter();
        }
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new CommandItem();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) =>
        item is CommandGroup or CommandSeparator ? NeedsContainer<Control>(item, out recycleKey) : NeedsContainer<CommandItem>(item, out recycleKey);

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        Refilter();
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        Refilter();
    }

    private static IEnumerable<Control> Ordered(ItemsControl owner) =>
        owner.GetRealizedContainers().Select((control, index) => (control, index)).OrderByDescending(pair => pair.control.GetValue(RankProperty)).ThenBy(pair => pair.index).Select(pair => pair.control);

    private IEnumerable<CommandItem> All() => GetRealizedContainers().SelectMany(entry => entry switch
    {
        CommandGroup group => group.GetRealizedContainers().OfType<CommandItem>(),
        CommandItem item => [item],
        _ => [],
    });

    private void Refilter()
    {
        _refiltering = true;
        var search = Text ?? string.Empty;
        var filtering = ShouldFilter && search.Length > 0;
        foreach (var item in All())
        {
            item.Score = filtering ? Filter(item.EffectiveValue, search, item.Keywords) : 1;
            item.IsVisible = item.Score > 0;
            item.SetValue(RankProperty, filtering ? item.Score : 0);
        }

        foreach (var entry in GetRealizedContainers())
        {
            Hide(entry, filtering);
        }

        var first = true;
        foreach (var group in Ordered(this).OfType<CommandGroup>().Where(group => group.IsVisible))
        {
            group.Follow(!first);
            first = false;
        }

        IsEmpty = !All().Any(item => item.IsVisible);
        Select(Visible().FirstOrDefault(item => item.IsEffectivelyEnabled));
        _refiltering = false;
    }

    private static void Hide(Control entry, bool filtering)
    {
        if (entry is CommandGroup group)
        {
            var scores = group.GetRealizedContainers().OfType<CommandItem>().Where(item => item.IsVisible).Select(item => item.Score).ToList();
            group.IsVisible = !filtering || scores.Count > 0;
            group.SetValue(RankProperty, filtering && scores.Count > 0 ? scores.Max() : 0);
        }
        else if (entry is CommandSeparator separator)
        {
            separator.IsVisible = !filtering;
        }
    }

    private void Follow(string? value)
    {
        if (_refiltering || SelectedItem?.EffectiveValue == value)
        {
            return;
        }

        var item = Visible().FirstOrDefault(each => each.EffectiveValue == value);
        foreach (var each in All())
        {
            each.Select(each == item);
        }
    }

    private void Move(int step)
    {
        var items = Visible().Where(item => item.IsEffectivelyEnabled).ToList();
        if (items.Count == 0)
        {
            return;
        }

        var index = items.FindIndex(item => item.IsSelected) + step;
        if (IsLoop)
        {
            index = (index + items.Count) % items.Count;
        }

        if (index >= 0 && index < items.Count)
        {
            Select(items[index]);
        }
    }

    private void Edge(bool last)
    {
        var items = Visible().Where(item => item.IsEffectivelyEnabled).ToList();
        Select(last ? items.LastOrDefault() : items.FirstOrDefault());
    }

    private void Jump(int step)
    {
        var groups = Ordered(this).OfType<CommandGroup>().Where(group => group.IsVisible).ToList();
        var current = groups.FindIndex(group => group.GetRealizedContainers().OfType<CommandItem>().Any(item => item.IsSelected));
        for (var index = current + step; index >= 0 && index < groups.Count; index += step)
        {
            if (Ordered(groups[index]).OfType<CommandItem>().FirstOrDefault(item => item.IsVisible && item.IsEffectivelyEnabled) is { } first)
            {
                Select(first);
                return;
            }
        }

        Move(step);
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        var control = (e.KeyModifiers & KeyModifiers.Control) != 0;
        var meta = (e.KeyModifiers & KeyModifiers.Meta) != 0;
        var alt = (e.KeyModifiers & KeyModifiers.Alt) != 0;
        switch (e.Key)
        {
            case Key.Down or Key.N or Key.J when e.Key == Key.Down || control:
                Step(1, meta, alt);
                break;
            case Key.Up or Key.P or Key.K when e.Key == Key.Up || control:
                Step(-1, meta, alt);
                break;
            case Key.Home:
                Edge(false);
                break;
            case Key.End:
                Edge(true);
                break;
            case Key.Enter:
                SelectedItem?.Choose();
                break;
            default:
                return;
        }

        e.Handled = true;
    }

    private void Step(int step, bool meta, bool alt)
    {
        if (meta)
        {
            Edge(step > 0);
        }
        else if (alt)
        {
            Jump(step);
        }
        else
        {
            Move(step);
        }
    }
}

/// <summary>The items panel of a <see cref="Command"/> and its groups: a stack that shows its children by search rank while a search ranks them (cmdk's sorting), in their order otherwise.</summary>
public sealed class CommandPanel : StackPanel
{
    static CommandPanel() => AffectsParentMeasure<CommandPanel>(Command.RankProperty);

    /// <inheritdoc />
    protected override Size MeasureOverride(Size availableSize)
    {
        if (!Ranked())
        {
            return base.MeasureOverride(availableSize);
        }

        var width = 0d;
        var height = 0d;
        foreach (var child in Children)
        {
            child.Measure(new Size(availableSize.Width, double.PositiveInfinity));
            width = Math.Max(width, child.DesiredSize.Width);
            height += child.DesiredSize.Height;
        }

        return new Size(width, height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        if (!Ranked())
        {
            return base.ArrangeOverride(finalSize);
        }

        var top = 0d;
        foreach (var child in Children.Select((control, index) => (control, index)).OrderByDescending(pair => pair.control.GetValue(Command.RankProperty)).ThenBy(pair => pair.index).Select(pair => pair.control))
        {
            child.Arrange(new Rect(0, top, finalSize.Width, child.DesiredSize.Height));
            top += child.DesiredSize.Height;
        }

        return finalSize;
    }

    private bool Ranked() => Children.Any(child => child.GetValue(Command.RankProperty) != 0);
}
