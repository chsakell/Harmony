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
                await container.DisposeAsync();
                throw Unavailable($"could not start {container.Image.FullName}", exception);
            }
        }

        private static InvalidOperationException Unavailable(string what, Exception exception) =>
            new($"{Message}: {what}. Start Docker Desktop (Linux containers) or the Docker daemon, then run again. {exception.Message}", exception);
    }
}
