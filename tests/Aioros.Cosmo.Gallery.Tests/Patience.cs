namespace Aioros.Cosmo.Gallery.Tests;

/// <summary>
/// A test's bound on how long it waits for the Gallery or the browser: the test's own cancellation, or 30 seconds on the
/// system clock, after which the wait fails (the bound of a failure, never the signal a test waits for).
/// </summary>
internal sealed class Patience : IDisposable
{
    public static readonly TimeSpan Limit = TimeSpan.FromSeconds(30);

    private readonly CancellationTokenSource _timeout = new(Limit, TimeProvider.System);
    private readonly CancellationTokenSource _linked;

    public Patience(CancellationToken cancellationToken) => _linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _timeout.Token);

    public CancellationToken Token => _linked.Token;

    public void Dispose()
    {
        _linked.Dispose();
        _timeout.Dispose();
    }
}
