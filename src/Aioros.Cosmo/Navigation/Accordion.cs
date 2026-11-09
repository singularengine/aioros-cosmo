using Aioros.Markup;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Interactivity;

namespace Aioros.Cosmo.Navigation;

/// <summary>
/// shadcn/ui's Accordion (Radix Accordion): a column of <see cref="Expander"/>s, its items or containers built for its data
/// items (each item the header through <c>ItemTemplate</c> and the content through <see cref="ContentTemplate"/>). One
/// item opens at a time unless <see cref="IsMultiple"/>; the open item closes only when <see cref="IsCollapsible"/>.
/// </summary>
public sealed class Accordion : ItemsControl
{
    /// <summary>Defines the <see cref="IsMultiple"/> property.</summary>
    public static readonly StyledProperty<bool> IsMultipleProperty = AvaloniaProperty.Register<Accordion, bool>(nameof(IsMultiple));

    /// <summary>Defines the <see cref="IsCollapsible"/> property.</summary>
    public static readonly StyledProperty<bool> IsCollapsibleProperty = AvaloniaProperty.Register<Accordion, bool>(nameof(IsCollapsible));

    /// <summary>Defines the <see cref="ContentTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> ContentTemplateProperty = AvaloniaProperty.Register<Accordion, IDataTemplate?>(nameof(ContentTemplate));

    private bool _switching;

    static Accordion() =>
        IsMultipleProperty.Changed.AddClassHandler<Accordion>((accordion, change) => Web.SetExclusive(accordion, !change.GetNewValue<bool>()));

    /// <summary>Hooks the items' opening and closing; a single accordion's items are one exclusive group on the web (<c>a:Web.Exclusive</c>, HTML's <c>&lt;details name&gt;</c>).</summary>
    public Accordion()
    {
        Web.SetExclusive(this, true);
        AddHandler(Expander.ExpandingEvent, Expanding);
        AddHandler(Expander.CollapsingEvent, Collapsing);
    }

    /// <summary>Whether several items may be open at once (Radix's <c>type="multiple"</c>); one at a time by default.</summary>
    public bool IsMultiple
    {
        get => GetValue(IsMultipleProperty);
        set => SetValue(IsMultipleProperty, value);
    }

    /// <summary>Whether the open item of a single accordion closes when its header is pressed (Radix's <c>collapsible</c>).</summary>
    public bool IsCollapsible
    {
        get => GetValue(IsCollapsibleProperty);
        set => SetValue(IsCollapsibleProperty, value);
    }

    /// <summary>The template of a data item's content, as <c>ItemTemplate</c> is its header's.</summary>
    public IDataTemplate? ContentTemplate
    {
        get => GetValue(ContentTemplateProperty);
        set => SetValue(ContentTemplateProperty, value);
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new Expander();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) => NeedsContainer<Expander>(item, out recycleKey);

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is Expander expander && !ReferenceEquals(container, item))
        {
            expander.Header = item;
            expander.HeaderTemplate = ItemTemplate;
            expander.Content = item;
            expander.ContentTemplate = ContentTemplate;
        }
    }

    private IEnumerable<Expander> Expanders() => GetRealizedContainers().OfType<Expander>();

    private void Expanding(object? sender, CancelRoutedEventArgs e)
    {
        if (IsMultiple || e.Source is not Expander opening)
        {
            return;
        }

        _switching = true;
        foreach (var open in Expanders().Where(item => item.IsExpanded && !ReferenceEquals(item, opening)))
        {
            open.IsExpanded = false;
        }

        _switching = false;
    }

    private void Collapsing(object? sender, CancelRoutedEventArgs e) =>
        e.Cancel = !IsMultiple && !IsCollapsible && !_switching && e.Source is Expander;
}
