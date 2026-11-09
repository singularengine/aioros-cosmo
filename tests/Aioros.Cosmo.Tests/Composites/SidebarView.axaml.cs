using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Composites;

public sealed partial class SidebarView : UserControl
{
    public SidebarView() => AvaloniaXamlLoader.Load(this);
}
