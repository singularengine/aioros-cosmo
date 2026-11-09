using System.Collections;
using System.Collections.Specialized;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;

namespace Aioros.Cosmo.Search;

/// <summary>
/// shadcn/ui's Combobox (its Popover and Command composition): an outline trigger showing the chosen item (or
/// <see cref="PlaceholderText"/>) and Lucide's <c>chevrons-up-down</c>, opening a popover as wide as the trigger with a
/// <see cref="Command"/> that searches its items. Choosing an item selects it and closes the popover, choosing the selected
/// one clears it (shadcn's toggle); with <c>SelectionMode="Multiple"</c> choosing toggles the item in
/// <c>SelectedItems</c> and keeps the popover open, the trigger showing the chosen items as badges. The items come from
/// <c>ItemsSource</c> with an <c>ItemTemplate</c> or <c>DisplayMemberBinding</c>; <c>SelectedItem</c>,
/// <c>SelectedItems</c> and <see cref="IsDropDownOpen"/> bind two way.
/// </summary>
[TemplatePart("PART_Trigger", typeof(Button))]
public sealed class Combobox : SelectingItemsControl
{
    /// <summary>Defines <see cref="SelectedItems"/>.</summary>
    public static readonly new DirectProperty<SelectingItemsControl, IList?> SelectedItemsProperty = SelectingItemsControl.SelectedItemsProperty;

    /// <summary>Defines <see cref="SelectionMode"/>.</summary>
    public static readonly new StyledProperty<SelectionMode> SelectionModeProperty = SelectingItemsControl.SelectionModeProperty;

    /// <summary>Defines <see cref="PlaceholderText"/>.</summary>
    public static readonly StyledProperty<string?> PlaceholderTextProperty = AvaloniaProperty.Register<Combobox, string?>(nameof(PlaceholderText), "Select...");

    /// <summary>Defines <see cref="SearchPlaceholder"/>.</summary>
    public static readonly StyledProperty<string?> SearchPlaceholderProperty = AvaloniaProperty.Register<Combobox, string?>(nameof(SearchPlaceholder), "Search...");

    /// <summary>Defines <see cref="Empty"/>.</summary>
    public static readonly StyledProperty<object?> EmptyProperty = AvaloniaProperty.Register<Combobox, object?>(nameof(Empty), "No results found.");

    /// <summary>Defines <see cref="IsDropDownOpen"/>.</summary>
    public static readonly StyledProperty<bool> IsDropDownOpenProperty = AvaloniaProperty.Register<Combobox, bool>(nameof(IsDropDownOpen), defaultBindingMode: BindingMode.TwoWay);

    /// <summary>Defines <see cref="SelectionTemplate"/>.</summary>
    public static readonly DirectProperty<Combobox, IDataTemplate?> SelectionTemplateProperty =
        AvaloniaProperty.RegisterDirect<Combobox, IDataTemplate?>(nameof(SelectionTemplate), combobox => combobox.SelectionTemplate);

    /// <summary>Defines <see cref="IsMultiple"/>.</summary>
    public static readonly DirectProperty<Combobox, bool> IsMultipleProperty = AvaloniaProperty.RegisterDirect<Combobox, bool>(nameof(IsMultiple), combobox => combobox.IsMultiple);

    private readonly Avalonia.Collections.AvaloniaList<object?> _items = [];
    private readonly Command _command;
    private readonly CommandGroup _group;
    private readonly Flyout _flyout;
    private Button? _trigger;
    private IDataTemplate? _selectionTemplate;
    private bool _multiple;

    static Combobox()
    {
        SelectionModeProperty.OverrideDefaultValue<Combobox>(SelectionMode.Single | SelectionMode.Toggle);
        IsDropDownOpenProperty.Changed.AddClassHandler<Combobox>((combobox, change) => combobox.Open(change.GetNewValue<bool>()));
        SelectionModeProperty.Changed.AddClassHandler<Combobox>((combobox, _) =>
        {
            combobox.IsMultiple = (combobox.SelectionMode & SelectionMode.Multiple) != 0;
            Aioros.Markup.Web.SetChoice(combobox, combobox.IsMultiple ? null : nameof(SelectedIndex));
        });
        ItemsSourceProperty.Changed.AddClassHandler<Combobox>((combobox, _) => combobox.Mirror());
        ItemTemplateProperty.Changed.AddClassHandler<Combobox>((combobox, _) => combobox.Templates());
        DisplayMemberBindingProperty.Changed.AddClassHandler<Combobox>((combobox, _) => combobox.Templates());
        SearchPlaceholderProperty.Changed.AddClassHandler<Combobox>((combobox, change) => combobox._command.Placeholder = change.GetNewValue<string?>());
        EmptyProperty.Changed.AddClassHandler<Combobox>((combobox, change) => combobox._command.Empty = change.GetNewValue<object?>());
    }

    /// <summary>Builds the popover's command over the combobox's items.</summary>
    public Combobox()
    {
        _group = new CommandGroup { ItemsSource = _items };
        Items.CollectionChanged += (_, _) => Mirror();
        _group.ContainerPrepared += (_, e) =>
        {
            Aioros.Markup.Web.SetChooses(e.Container, e.Index);
            Check(e.Container);
        };
        _command = new Command { Placeholder = SearchPlaceholder, Empty = Empty, Items = { _group }, Classes = { "combobox" } };
        _command.AddHandler(Button.ClickEvent, OnChosen);
        _flyout = new Flyout { Content = _command, Placement = PlacementMode.BottomEdgeAlignedLeft, FlyoutPresenterClasses = { "combobox" } };
        _flyout.Opened += (_, _) => Opened();
        _flyout.Closed += (_, _) => SetCurrentValue(IsDropDownOpenProperty, false);
        SelectionChanged += (_, _) => Refresh();
        Aioros.Markup.Web.SetChoice(this, nameof(SelectedIndex));
        Templates();
    }

    /// <summary>The chosen items of a multiple combobox, two way.</summary>
    public new IList? SelectedItems
    {
        get => base.SelectedItems;
        set => base.SelectedItems = value;
    }

    /// <summary>How items are chosen: <c>Single</c> (with <c>Toggle</c>, the default, as shadcn's toggle) or <c>Multiple</c>.</summary>
    public new SelectionMode SelectionMode
    {
        get => base.SelectionMode;
        set => base.SelectionMode = value;
    }

    /// <summary>What the trigger shows while nothing is chosen.</summary>
    public string? PlaceholderText
    {
        get => GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    /// <summary>The search input's placeholder.</summary>
    public string? SearchPlaceholder
    {
        get => GetValue(SearchPlaceholderProperty);
        set => SetValue(SearchPlaceholderProperty, value);
    }

    /// <summary>What the list shows when no item matches the search.</summary>
    public object? Empty
    {
        get => GetValue(EmptyProperty);
        set => SetValue(EmptyProperty, value);
    }

    /// <summary>Whether the popover is open, two way.</summary>
    public bool IsDropDownOpen
    {
        get => GetValue(IsDropDownOpenProperty);
        set => SetValue(IsDropDownOpenProperty, value);
    }

    /// <summary>The template the trigger draws the chosen item with: <c>ItemTemplate</c>, or one from <c>DisplayMemberBinding</c>.</summary>
    public IDataTemplate? SelectionTemplate
    {
        get => _selectionTemplate;
        private set => SetAndRaise(SelectionTemplateProperty, ref _selectionTemplate, value);
    }

    /// <summary>Whether several items can be chosen (<c>SelectionMode</c> has <c>Multiple</c>).</summary>
    public bool IsMultiple
    {
        get => _multiple;
        private set => SetAndRaise(IsMultipleProperty, ref _multiple, value);
    }

    /// <summary>The popover's command.</summary>
    public Command List => _command;

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == WidthProperty || change.Property == BoundsProperty)
        {
            Fit();
        }
    }

    /// <summary>
    /// Sizes the command to the trigger (shadcn's 200 px popover beside its 200 px trigger) from the combobox's width as
    /// soon as it is set, its laid-out width otherwise, so a popover written before it is ever opened (the web traces a
    /// page without laying it out) has its width.
    /// </summary>
    private void Fit()
    {
        if ((double.IsNaN(Width) ? Bounds.Width : Width) is var width and > 0)
        {
            _command.Width = Math.Max(0, width - 2);
        }
    }

    /// <inheritdoc />
    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);
        if (_trigger is not null)
        {
            _trigger.Flyout = null;
        }

        _trigger = e.NameScope.Find<Button>("PART_Trigger");
        if (_trigger is not null)
        {
            _trigger.Flyout = _flyout;
        }
    }

    private void Mirror()
    {
        _items.Clear();
        _items.AddRange(Items);
    }

    private void Templates()
    {
        var template = ItemTemplate ?? (DisplayMemberBinding is { } binding ? new FuncDataTemplate<object?>((_, _) => new TextBlock { [!TextBlock.TextProperty] = binding }) : null);
        _group.ItemTemplate = template;
        SelectionTemplate = template;
    }

    private void Open(bool open)
    {
        if (open && !_flyout.IsOpen && _trigger is not null)
        {
            _flyout.ShowAt(_trigger);
        }
        else if (!open && _flyout.IsOpen)
        {
            _flyout.Hide();
        }
    }

    private void Opened()
    {
        SetCurrentValue(IsDropDownOpenProperty, true);
        _command.Text = string.Empty;
        Fit();
        Refresh();
    }

    private void OnChosen(object? sender, RoutedEventArgs e)
    {
        if (e.Source is not CommandItem item || item.FindLogicalAncestorOfType<Combobox>() is { } nested && nested != this)
        {
            return;
        }

        var data = _group.ItemFromContainer(item) ?? item;
        if (IsMultiple)
        {
            Toggle(SelectedItems, data);
            return;
        }

        SelectedItem = Equals(SelectedItem, data) ? null : data;
        SetCurrentValue(IsDropDownOpenProperty, false);
    }

    private static void Toggle(IList? selected, object data)
    {
        if (selected is null)
        {
            return;
        }

        if (selected.Contains(data))
        {
            selected.Remove(data);
        }
        else
        {
            selected.Add(data);
        }
    }

    private void Refresh()
    {
        PseudoClasses.Set(":multiple", IsMultiple);
        PseudoClasses.Set(":chosen", IsMultiple ? SelectedItems?.Count > 0 : SelectedItem is not null);
        foreach (var container in _group.GetRealizedContainers())
        {
            Check(container);
        }
    }

    private void Check(Control container)
    {
        if (container is CommandItem item)
        {
            var data = _group.ItemFromContainer(item) ?? item;
            item.IsChecked = IsMultiple ? SelectedItems?.Contains(data) == true : Equals(SelectedItem, data);
        }
    }
}
