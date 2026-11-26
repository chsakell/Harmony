using DotNet.Testcontainers.Builders;
using Shouldly;
using Testcontainers.MongoDb;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    public class DatabaseServersTests(DatabaseServers servers)
    {
        [Fact]
        [Trait("Requirement", "FR-001")]
        public async Task GetSqlServerAsync_RequestedTwice_ReturnsSameContainer()
        {
            var first = await servers.GetSqlServerAsync();
            var second = await servers.GetSqlServerAsync();

            second.ShouldBeSameAs(first);
        }

        [Fact]
        [Trait("Requirement", "FR-001")]
        public async Task GetPostgreSqlAsync_RequestedTwice_ReturnsSameContainer()
        {
            var first = await servers.GetPostgreSqlAsync();
            var second = await servers.GetPostgreSqlAsync();

            second.ShouldBeSameAs(first);
        }

        [Fact]
        [Trait("Requirement", "FR-001")]
        public async Task GetMongoDbAsync_RequestedTwice_ReturnsSameContainer()
        {
            var first = await servers.GetMongoDbAsync();
            var second = await servers.GetMongoDbAsync();

            second.ShouldBeSameAs(first);
        }

        /// <summary>
        /// Points a real builder at an endpoint where no Docker daemon listens, so Start fails as it does when
        /// Docker is stopped, without stopping the developer's Docker.
        /// </summary>
        [Fact]
        [Trait("Requirement", "FR-003")]
        public async Task DockerGuard_DockerUnavailable_ThrowsMessageNamingDocker()
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => DockerGuard.StartAsync(
                () => new MongoDbBuilder(ImageTags.MongoDb).WithDockerEndpoint("tcp://127.0.0.1:1").Build()));

            exception.Message.ShouldStartWith(DockerGuard.Message);
            exception.InnerException.ShouldNotBeNull();
        }

        /// <summary>
        /// Builders throw <see cref="DockerUnavailableException"/> from Build when they find no Docker endpoint at all.
        /// </summary>
        [Fact]
        [Trait("Requirement", "FR-003")]
        public async Task DockerGuard_BuildFindsNoDocker_ThrowsMessageNamingDocker()
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(() => DockerGuard.StartAsync<MongoDbContainer>(
                () => throw new DockerUnavailableException("No Docker endpoint found.")));

            exception.Message.ShouldStartWith(DockerGuard.Message);
            exception.InnerException.ShouldBeOfType<DockerUnavailableException>();
        }

        [Fact]
        [Trait("Requirement", "NFR-002")]
        public async Task StartedContainers_NamesDoNotStartWithHarmony()
        {
            var names = new[]
            {
                (await servers.GetSqlServerAsync()).Name,
                (await servers.GetPostgreSqlAsync()).Name,
                (await servers.GetMongoDbAsync()).Name,
            };

            names.ShouldAllBe(name => !name.TrimStart('/').StartsWith("harmony-", StringComparison.OrdinalIgnoreCase));
        }
    }
}
