using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Display;

public sealed partial class DisplayView : UserControl
{
    public DisplayView() => AvaloniaXamlLoader.Load(this);
}
