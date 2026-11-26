using DotNet.Testcontainers.Containers;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Builds and starts a container, and turns any failure to do so into one clear error (FR-003). Builders throw
    /// <c>DockerUnavailableException</c> when they find no Docker endpoint; Start fails when the endpoint does not
    /// answer or cannot run the Linux image. Either way the selected tests fail; none is skipped.
    /// </summary>
    public static class DockerGuard
    {
        public const string Message = "Docker with Linux containers is required to run the integration tests";

        public static async Task<TContainer> StartAsync<TContainer>(Func<TContainer> build)
            where TContainer : IContainer
        {
            TContainer container;
            try
            {
                container = build();
            }
            catch (Exception exception)
            {
                throw Unavailable("could not create the container", exception);
            }

            try
            {
                await container.StartAsync();
                return container;
            }
            catch (Exception exception)
            {
                await TryDisposeAsync(container);
                throw Unavailable($"could not start {container.Image.FullName}", exception);
            }
        }

        /// <summary>
        /// Cleaning up a container that did not start can fail too when Docker is unreachable. That failure is
        /// dropped so the Docker message is what the test reports; Ryuk removes anything left behind.
        /// </summary>
        private static async Task TryDisposeAsync(IContainer container)
        {
            try
            {
                await container.DisposeAsync();
            }
            catch (Exception)
            {
                // The start failure is the one to report.
            }
        }

        private static InvalidOperationException Unavailable(string what, Exception exception) =>
            new($"{Message}: {what}. Start Docker Desktop (Linux containers) or the Docker daemon, then run again. {exception.Message}", exception);
    }
}
