using Avalonia.Controls;

namespace Aioros.Cosmo.Display;

/// <summary>shadcn/ui's typography list: its items as a bulleted list (<c>list-disc</c>), 24 px in and 8 px apart; the <c>inside</c> class keeps the bullets inside.</summary>
public sealed class List : ItemsControl
{
    /// <inheritdoc />
    protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey) => new ListItem();

    /// <inheritdoc />
    protected override bool NeedsContainerOverride(object? item, int index, out object? recycleKey) => NeedsContainer<ListItem>(item, out recycleKey);
}

/// <summary>An item of a <see cref="List"/>: its bullet and its content.</summary>
public sealed class ListItem : ContentControl
{
}
