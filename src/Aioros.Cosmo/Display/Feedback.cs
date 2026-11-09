using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Aioros.Cosmo.Display;

/// <summary>shadcn/ui's Skeleton: a placeholder box in <c>--accent</c>, rounded, pulsing while content loads; size it with <c>Width</c> and <c>Height</c>.</summary>
public sealed class Skeleton : TemplatedControl
{
}

/// <summary>shadcn/ui's Spinner: Lucide's <c>loader-circle</c> turning once a second, 16 px by default, in its parent's text color, named "Loading" for assistive technologies.</summary>
public sealed class Spinner : TemplatedControl
{
}

/// <summary>shadcn/ui's Kbd: a key of the keyboard, 20 px high on <c>--muted</c>.</summary>
public sealed class Kbd : ContentControl
{
}

/// <summary>shadcn/ui's KbdGroup: keys in a row, 4 px apart.</summary>
public sealed class KbdGroup : StackPanel
{
}
