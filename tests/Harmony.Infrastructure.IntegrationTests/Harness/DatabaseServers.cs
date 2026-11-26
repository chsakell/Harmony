using DotNet.Testcontainers.Containers;
using Testcontainers.MongoDb;
using Testcontainers.MsSql;
using Testcontainers.PostgreSql;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// The run's database servers: at most one disposable instance per engine (FR-001). Each starts the first time
    /// a test asks for it, so a run that selects only MongoDB tests starts no SQL Server or PostgreSQL container.
    /// The builders use no container name, no fixed host port and no volume (NFR-002): Testcontainers gives each
    /// container a random name and port, and its clean-up container (Ryuk) removes them if the process is killed.
    /// </summary>
    public sealed class DatabaseServers : IAsyncDisposable
    {
        private readonly Lazy<Task<MsSqlContainer>> sqlServer =
            new(() => DockerGuard.StartAsync(() => new MsSqlBuilder(ImageTags.SqlServer).Build()));

        private readonly Lazy<Task<PostgreSqlContainer>> postgreSql =
            new(() => DockerGuard.StartAsync(() => new PostgreSqlBuilder(ImageTags.PostgreSql).Build()));

        private readonly Lazy<Task<MongoDbContainer>> mongoDb =
            new(() => DockerGuard.StartAsync(() => new MongoDbBuilder(ImageTags.MongoDb).Build()));

        public Task<MsSqlContainer> GetSqlServerAsync() => sqlServer.Value;

        public Task<PostgreSqlContainer> GetPostgreSqlAsync() => postgreSql.Value;

        public Task<MongoDbContainer> GetMongoDbAsync() => mongoDb.Value;

        public async ValueTask DisposeAsync()
        {
            await DisposeIfStartedAsync(sqlServer);
            await DisposeIfStartedAsync(postgreSql);
            await DisposeIfStartedAsync(mongoDb);
        }

        /// <summary>
        /// Disposes only a container that started; a failed start has already disposed its own container.
        /// </summary>
        private static async Task DisposeIfStartedAsync<TContainer>(Lazy<Task<TContainer>> container)
            where TContainer : IContainer
        {
            if (container.IsValueCreated && container.Value.IsCompletedSuccessfully)
            {
                await container.Value.Result.DisposeAsync();
            }
        }
    }
}
