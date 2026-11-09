using Avalonia.Controls;

namespace Aioros.Cosmo.Display;

/// <summary>
/// shadcn/ui's TableFooter row: a 1 px top border over <c>--muted</c> at half opacity, medium text with the cells' 8 px
/// padding. Placed under a <see cref="TableView"/> (themed as shadcn's table), its content lays out the totals.
/// </summary>
public sealed class TableFooter : ContentControl
{
}

/// <summary>shadcn/ui's TableCaption: muted 14 px text centered 16 px under the table.</summary>
public sealed class TableCaption : TextBlock
{
    /// <inheritdoc />
    protected override Type StyleKeyOverride => typeof(TableCaption);
}
