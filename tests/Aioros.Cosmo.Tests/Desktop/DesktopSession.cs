using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Themes.Fluent;

[assembly: AssemblyFixture(typeof(Aioros.Cosmo.Tests.Desktop.DesktopSession))]

namespace Aioros.Cosmo.Tests.Desktop;

/// <summary>The headless Avalonia app the desktop tests run in: Skia drawing, the Inter font and Fluent, as an Aioros desktop app has.</summary>
public sealed class TestApp : Application
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<TestApp>()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
        .WithInterFont()
        .With(new FontManagerOptions { DefaultFamilyName = "fonts:Inter#Inter" });

    public override void Initialize()
    {
        Styles.Add(new FluentTheme());
        Resources["CosmoCodeHighlighter"] = new Aioros.Cosmo.Chat.ShikiTokens { Source = new Uri("avares://Aioros.Cosmo.Tests/Assets/highlighted-markdown.json") };
    }
}

/// <summary>One headless session for the whole assembly; every test dispatches its work to its UI thread.</summary>
public sealed class DesktopSession : IAsyncLifetime
{
    public HeadlessUnitTestSession Session { get; } = HeadlessUnitTestSession.StartNew(typeof(TestApp));

    public async ValueTask InitializeAsync() => await Session.Dispatch(static () => 0, CancellationToken.None);

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>Runs <paramref name="work"/> on the session's UI thread.</summary>
    public Task<T> RunAsync<T>(Func<T> work) => Session.Dispatch(work, TestContext.Current.CancellationToken);
}
