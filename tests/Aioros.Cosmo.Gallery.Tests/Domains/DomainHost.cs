using Aioros.Avalonia.Hosting;
using Aioros.Cosmo.Theming;
using Avalonia;
using Avalonia.Themes.Fluent;

namespace Aioros.Cosmo.Gallery.Tests.Domains;

/// <summary>An application under Fluent and Cosmo's theme only, as a Cosmo app styles its pages.</summary>
public sealed class DomainApp : Application
{
    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Styles.Add(new CosmoTheme());
    }
}

/// <summary>Aioros's engine over <see cref="DomainApp"/>, in this process, for the domain walks.</summary>
public sealed class DomainHost : IAsyncLifetime
{
    private AvaloniaHost? _host;

    public AvaloniaPageEngine Engine { get; private set; } = null!;

    public async ValueTask InitializeAsync()
    {
        _host = await AvaloniaHost.StartAsync(typeof(DomainApp), web: true);
        Engine = new AvaloniaPageEngine(_host, new XamlSources(typeof(DomainApp).Assembly));
    }

    public async ValueTask DisposeAsync()
    {
        if (_host is not null)
        {
            await _host.DisposeAsync();
        }
    }
}
