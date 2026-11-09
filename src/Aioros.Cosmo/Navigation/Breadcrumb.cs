using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;

namespace Aioros.Cosmo.Navigation;

/// <summary>
/// shadcn/ui's Breadcrumb: its items (links, a <see cref="BreadcrumbPage"/>, a <see cref="BreadcrumbEllipsis"/>, a menu
/// button, or data items through <c>ItemTemplate</c>) in a row, each after the separator <see cref="SeparatorTemplate"/>
/// draws (Lucide's <c>chevron-right</c> by default).
/// </summary>
public sealed class Breadcrumb : ItemsControl
{
    /// <summary>Defines the <see cref="SeparatorTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty = AvaloniaProperty.Register<Breadcrumb, IDataTemplate?>(nameof(SeparatorTemplate));

    static Breadcrumb() => SeparatorTemplateProperty.Changed.AddClassHandler<Breadcrumb>((breadcrumb, _) =>
    {
        foreach (var item in breadcrumb.GetRealizedContainers().OfType<BreadcrumbItem>())
        {
            item.SeparatorTemplate = breadcrumb.SeparatorTemplate;
        }
    });

    /// <summary>What separates two items (shadcn's <c>BreadcrumbSeparator</c> children).</summary>
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }

    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new BreadcrumbItem();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) => NeedsContainer<BreadcrumbItem>(item, out recycleKey);

    /// <inheritdoc />
    protected override void PrepareContainerForItemOverride(Control container, object? item, int index)
    {
        base.PrepareContainerForItemOverride(container, item, index);
        if (container is BreadcrumbItem crumb)
        {
            crumb.SeparatorTemplate = SeparatorTemplate;
            crumb.IsFirst = index == 0;
        }
    }

    /// <inheritdoc />
    protected override void ContainerIndexChangedOverride(Control container, int oldIndex, int newIndex)
    {
        base.ContainerIndexChangedOverride(container, oldIndex, newIndex);
        if (container is BreadcrumbItem crumb)
        {
            crumb.IsFirst = newIndex == 0;
        }
    }
}

/// <summary>An item of a <see cref="Breadcrumb"/>: the separator (but before the first item) and its content.</summary>
public sealed class BreadcrumbItem : ContentControl
{
    /// <summary>Defines the <see cref="SeparatorTemplate"/> property.</summary>
    public static readonly StyledProperty<IDataTemplate?> SeparatorTemplateProperty = Breadcrumb.SeparatorTemplateProperty.AddOwner<BreadcrumbItem>();

    /// <summary>Defines the <see cref="IsFirst"/> property.</summary>
    public static readonly StyledProperty<bool> IsFirstProperty = AvaloniaProperty.Register<BreadcrumbItem, bool>(nameof(IsFirst));

    /// <summary>The separator drawn before the item.</summary>
    public IDataTemplate? SeparatorTemplate
    {
        get => GetValue(SeparatorTemplateProperty);
        set => SetValue(SeparatorTemplateProperty, value);
    }

    /// <summary>Whether the item is the breadcrumb's first, which has no separator.</summary>
    public bool IsFirst
    {
        get => GetValue(IsFirstProperty);
        set => SetValue(IsFirstProperty, value);
    }
}

/// <summary>shadcn/ui's BreadcrumbPage: the current page, in <c>--foreground</c>.</summary>
public sealed class BreadcrumbPage : TextBlock
{
}

/// <summary>shadcn/ui's BreadcrumbEllipsis: Lucide's <c>ellipsis</c> standing for collapsed items, 36 px square by default.</summary>
public sealed class BreadcrumbEllipsis : TemplatedControl
{
}
