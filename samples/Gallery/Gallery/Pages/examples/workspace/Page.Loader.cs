using Aioros.Pages;
using Gallery.Models;

namespace Gallery.Pages.Examples.Workspace;

/// <summary>
/// Reads the sidebar's state the browser keeps in its <c>sidebar_state</c> cookie (the provider's <c>StateKey</c>), so
/// the page is written folded or open as it was left, as shadcn/ui's layout reads it with <c>cookies()</c>; the desktop,
/// which has no cookies, opens it.
/// </summary>
public sealed class WorkspaceLoader : ILoader<WorkspaceState>
{
    public ValueTask<LoaderResult<WorkspaceState>> LoadAsync(PageRequest request, CancellationToken cancellationToken) =>
        ValueTask.FromResult(new LoaderResult<WorkspaceState>(new WorkspaceState(request.Cookies.GetValueOrDefault("sidebar_state") != "false")));
}
