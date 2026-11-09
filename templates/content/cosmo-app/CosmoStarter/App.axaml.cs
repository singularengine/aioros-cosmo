using Avalonia;
using Avalonia.Markup.Xaml;

namespace CosmoStarter;

public sealed partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
}
