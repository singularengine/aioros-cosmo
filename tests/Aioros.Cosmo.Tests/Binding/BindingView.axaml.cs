using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Aioros.Cosmo.Tests.Binding;

public sealed partial class BindingView : UserControl
{
    public BindingView() => AvaloniaXamlLoader.Load(this);
}
