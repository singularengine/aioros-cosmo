using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Display;

public sealed partial class NavigationView : UserControl
{
    public NavigationView() => AvaloniaXamlLoader.Load(this);
}
