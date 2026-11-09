namespace Aioros.Cosmo.Testing;

/// <summary>The repository the tests run in, found from the test assembly's folder upward.</summary>
internal static class Repository
{
    public static string Root { get; } = Find();

    /// <summary>A path under the repository's root.</summary>
    public static string Path(params string[] parts) => System.IO.Path.Combine([Root, .. parts]);

    private static string Find()
    {
        var folder = new DirectoryInfo(AppContext.BaseDirectory);
        while (folder is not null && !File.Exists(System.IO.Path.Combine(folder.FullName, "Aioros.Cosmo.slnx")))
        {
            folder = folder.Parent;
        }

        return folder?.FullName ?? throw new InvalidOperationException("The repository root was not found.");
    }
}
