using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Aioros.Cosmo.Navigation;

/// <summary>
/// shadcn/ui's Pagination (with <c>PaginationContent</c>): its items, written or built from the model through
/// <c>ItemTemplate</c>, in a centered row 4 px apart: <see cref="PaginationPrevious"/>, page buttons
/// (<c>Classes="ghost icon"</c>, the current one <c>outline</c>), <see cref="PaginationEllipsis"/> and
/// <see cref="PaginationNext"/>.
/// </summary>
public sealed class Pagination : ItemsControl
{
    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new PaginationItem();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey)
    {
        recycleKey = null;
        return item is not PaginationItem;
    }
}

/// <summary>shadcn/ui's PaginationItem: one item of a <see cref="Pagination"/>, a list item on the web.</summary>
public sealed class PaginationItem : ContentControl
{
}

/// <summary>shadcn/ui's PaginationPrevious: a ghost button with Lucide's <c>chevron-left</c> before its content ("Previous").</summary>
public sealed class PaginationPrevious : Button
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PaginationPrevious);
}

/// <summary>shadcn/ui's PaginationNext: a ghost button with Lucide's <c>chevron-right</c> after its content ("Next").</summary>
public sealed class PaginationNext : Button
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(PaginationNext);
}

/// <summary>shadcn/ui's PaginationEllipsis: Lucide's <c>ellipsis</c> for hidden pages, 36 px square.</summary>
public sealed class PaginationEllipsis : TemplatedControl
{
}
