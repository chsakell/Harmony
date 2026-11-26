using Harmony.Application.Constants;
using Harmony.Persistence.Configurations.MongoDb;
using MongoDB.Driver;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Collection fixture for <see cref="MongoDbCollection"/>: one client on the run's MongoDB instance, which starts
    /// only when a MongoDB test runs (FR-001). The hosts call <see cref="MongoDbConfiguration.Configure"/> once at
    /// startup; it registers serializers and class maps process-wide and throws if called twice, so it runs once here.
    /// </summary>
    public sealed class MongoDbFixture(DatabaseServers servers) : IAsyncLifetime
    {
        private static readonly Lazy<bool> Configured = new(() =>
        {
            MongoDbConfiguration.Configure();
            return true;
        });

        private IMongoClient? client;

        public IMongoClient Client => client ?? throw new InvalidOperationException("The MongoDB fixture is not initialized.");

        public async ValueTask InitializeAsync()
        {
            _ = Configured.Value;
            var container = await servers.GetMongoDbAsync();
            client = new MongoClient(container.GetConnectionString());
        }

        /// <summary>
        /// Call before each test: the stores' database names are fixed.
        /// </summary>
        public async Task DropDatabasesAsync()
        {
            await Client.DropDatabaseAsync(MongoDbConstants.AutomationsDatabase);
            await Client.DropDatabaseAsync(MongoDbConstants.SourceControlDatabase);
        }

        public ValueTask DisposeAsync()
        {
            client?.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
