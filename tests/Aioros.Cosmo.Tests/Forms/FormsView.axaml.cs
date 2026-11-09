using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Forms;

public sealed partial class FormsView : UserControl
{
    public FormsView() => AvaloniaXamlLoader.Load(this);
}
