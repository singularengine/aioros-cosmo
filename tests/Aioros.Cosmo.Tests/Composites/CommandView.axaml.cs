using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Composites;

public sealed partial class CommandView : UserControl
{
    public CommandView() => AvaloniaXamlLoader.Load(this);
}
