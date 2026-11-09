using Avalonia.Controls;

namespace Aioros.Cosmo.Forms;

/// <summary>
/// shadcn/ui's InputGroup: one bordered field holding a <see cref="TextBox"/> (single or multi-line) and its
/// <see cref="InputGroupAddon"/>s, laid out in a grid: addons before the text box by default, after it with the
/// <c>inline-end</c> class, below it with <c>block-end</c>. The group shows the focus ring while its text box has the focus.
/// </summary>
public sealed class InputGroup : ItemsControl
{
}

/// <summary>shadcn/ui's InputGroupAddon: an icon, a text or buttons beside or below an input group's text box.</summary>
public sealed class InputGroupAddon : ContentControl
{
}
