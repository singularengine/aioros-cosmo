using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Composites;

public sealed partial class CompositesView : UserControl
{
    public CompositesView() => AvaloniaXamlLoader.Load(this);
}
