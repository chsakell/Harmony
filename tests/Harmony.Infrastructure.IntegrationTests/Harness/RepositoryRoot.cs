namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// The folder that holds Harmony.slnx, found by walking up from the test assembly.
    /// </summary>
    public static class RepositoryRoot
    {
        private static readonly Lazy<string> Root = new(Find);

        public static string Path => Root.Value;

        private static string Find()
        {
            for (var folder = new DirectoryInfo(AppContext.BaseDirectory); folder is not null; folder = folder.Parent)
            {
                if (File.Exists(System.IO.Path.Combine(folder.FullName, "Harmony.slnx")))
                {
                    return folder.FullName;
                }
            }

            throw new InvalidOperationException($"No folder above {AppContext.BaseDirectory} holds Harmony.slnx.");
        }
    }
}
