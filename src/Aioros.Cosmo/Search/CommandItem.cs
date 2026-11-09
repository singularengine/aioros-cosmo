using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Metadata;
using Avalonia.VisualTree;

namespace Aioros.Cosmo.Search;

/// <summary>
/// shadcn/ui's CommandItem (cmdk's <c>Command.Item</c>): an option of a <see cref="Command"/> list, 32 px (8 by 6 px of
/// padding, a 6 px radius, 14 px text, its icons 16 px), <c>--accent</c> while selected. It is matched against the typed
/// search by its <see cref="Value"/> (its text, its <see cref="Shortcut"/> included, when unset) and
/// <see cref="Keywords"/>; choosing it (a click, or Enter while it is selected) runs its <c>Command</c>. The keyboard focus
/// stays in the command's input, as cmdk's does.
/// </summary>
public sealed class CommandItem : Button
{
    /// <summary>Defines <see cref="Value"/>.</summary>
    public static readonly StyledProperty<string?> ValueProperty = AvaloniaProperty.Register<CommandItem, string?>(nameof(Value));

    /// <summary>Defines <see cref="Keywords"/>.</summary>
    public static readonly StyledProperty<IReadOnlyList<string>?> KeywordsProperty = AvaloniaProperty.Register<CommandItem, IReadOnlyList<string>?>(nameof(Keywords));

    /// <summary>Defines <see cref="Shortcut"/>.</summary>
    public static readonly StyledProperty<string?> ShortcutProperty = AvaloniaProperty.Register<CommandItem, string?>(nameof(Shortcut));

    /// <summary>Defines <see cref="IsChecked"/>.</summary>
    public static readonly StyledProperty<bool?> IsCheckedProperty = AvaloniaProperty.Register<CommandItem, bool?>(nameof(IsChecked));

    static CommandItem()
    {
        FocusableProperty.OverrideDefaultValue<CommandItem>(false);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<CommandItem>(AutomationControlType.ListItem);
        IsCheckedProperty.Changed.AddClassHandler<CommandItem>((item, change) =>
        {
            item.PseudoClasses.Set(":checkable", change.GetNewValue<bool?>() is not null);
            item.PseudoClasses.Set(":checked", change.GetNewValue<bool?>() == true);
        });
        ValueProperty.Changed.AddClassHandler<CommandItem>((item, _) => item.Searched());
        KeywordsProperty.Changed.AddClassHandler<CommandItem>((item, _) => item.Searched());
        ClickEvent.AddClassHandler<CommandItem>((item, _) => item.FindLogicalAncestorOfType<Command>()?.Select(item));
    }

    /// <summary>
    /// The value the browser matches the search against, as Avalonia's <c>TextSearch.Text</c> (Aioros's filtered lists read
    /// it as the option's <c>data-value</c>): the <see cref="Value"/> with its <see cref="Keywords"/> after it, as cmdk's
    /// <c>command-score</c> joins them; the item's text and shortcut while no value is set.
    /// </summary>
    private void Searched()
    {
        if (Value is { } value)
        {
            TextSearch.SetText(this, Keywords is { Count: > 0 } keywords ? value + " " + string.Join(' ', keywords) : value);
        }
        else
        {
            ClearValue(TextSearch.TextProperty);
        }
    }

    /// <summary>The text the search is matched against (cmdk's <c>value</c>); the item's text and shortcut when unset.</summary>
    public string? Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>More words the search matches (cmdk's <c>keywords</c>).</summary>
    public IReadOnlyList<string>? Keywords
    {
        get => GetValue(KeywordsProperty);
        set => SetValue(KeywordsProperty, value);
    }

    /// <summary>The shortcut shown at the end of the item (shadcn's <c>CommandShortcut</c>).</summary>
    public string? Shortcut
    {
        get => GetValue(ShortcutProperty);
        set => SetValue(ShortcutProperty, value);
    }

    /// <summary>Whether the item shows a check at its end (a combobox's chosen option); no check column when null.</summary>
    public bool? IsChecked
    {
        get => GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }

    /// <summary>Whether the item is the command's selected one (cmdk's <c>data-selected</c>).</summary>
    public bool IsSelected { get; private set; }

    /// <summary>The value the search is matched against: <see cref="Value"/>, or the item's text followed by its shortcut.</summary>
    public string EffectiveValue => (Value ?? TextOf(this) + Shortcut).Trim();

    internal double Score { get; set; } = 1;

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CommandItem);

    internal void Select(bool selected)
    {
        IsSelected = selected;
        PseudoClasses.Set(":selected", selected);
        if (selected)
        {
            this.BringIntoView();
        }
    }

    internal void Choose()
    {
        if (IsEffectivelyEnabled)
        {
            OnClick();
        }
    }

    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);
        if (IsEffectivelyEnabled && !IsSelected)
        {
            this.FindLogicalAncestorOfType<Command>()?.Select(this);
        }
    }

    private static string TextOf(Control item)
    {
        if (item is ContentControl { Content: string text })
        {
            return text;
        }

        var visual = item.GetVisualDescendants().OfType<TextBlock>().Where(block => block.Name != "PART_Shortcut").Select(block => block.Text);
        var logical = item.GetLogicalDescendants().OfType<TextBlock>().Select(block => block.Text);
        var texts = visual.Any() ? visual : logical;
        return string.Concat(texts);
    }
}

/// <summary>
/// shadcn/ui's CommandGroup (cmdk's <c>Command.Group</c>): a <c>Header</c> (12 px, medium, muted, 8 by 6 px of
/// padding) over its <see cref="CommandItem"/>s, 4 px of padding around; hidden while no item of it matches the search.
/// Its items can come from <c>ItemsSource</c> with an <c>ItemTemplate</c>, each in a <see cref="CommandItem"/> that runs
/// <see cref="ItemCommand"/> with <see cref="ItemCommandParameter"/> read from its item (cmdk's <c>onSelect</c>).
/// </summary>
public sealed class CommandGroup : HeaderedItemsControl
{
    /// <summary>The binding, read from each item of <c>ItemsSource</c>, that gives its item the command it runs when chosen, unless the item has its own.</summary>
    [Avalonia.Data.AssignBinding]
    public Avalonia.Data.BindingBase? ItemCommand { get; set; }

    /// <summary>The binding, read from each item of <c>ItemsSource</c>, that gives <see cref="ItemCommand"/> its parameter.</summary>
    [Avalonia.Data.AssignBinding]
    public Avalonia.Data.BindingBase? ItemCommandParameter { get; set; }

    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CommandGroup);

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is not CommandItem entry || ReferenceEquals(container, item))
        {
            return;
        }

        if (ItemCommand is { } command && !entry.IsSet(Button.CommandProperty))
        {
            entry.Bind(Button.CommandProperty, command);
        }

        if (ItemCommandParameter is { } parameter)
        {
            entry.Bind(Button.CommandParameterProperty, parameter);
        }
    }

    internal void Follow(bool following) => PseudoClasses.Set(":following", following);

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new CommandItem();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) => NeedsContainer<CommandItem>(item, out recycleKey);

    /// <inheritdoc />
    protected override void ContainerForItemPreparedOverride(Control container, object? item, int index)
    {
        base.ContainerForItemPreparedOverride(container, item, index);
        this.FindLogicalAncestorOfType<Command>()?.ItemAdded();
    }
}

/// <summary>shadcn/ui's CommandSeparator: a 1 px <c>--border</c> line between groups, shown only while nothing is typed (cmdk's).</summary>
public sealed class CommandSeparator : Separator
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(CommandSeparator);
}
