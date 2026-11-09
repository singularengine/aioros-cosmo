using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Display;

public sealed partial class StructureView : UserControl
{
    public StructureView() => AvaloniaXamlLoader.Load(this);
}
