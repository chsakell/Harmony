using Harmony.Persistence.DbContext;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Class fixture: the class's own fresh Harmony database per provider (FR-006), created and migrated the first
    /// time a test of the class asks for that provider. Tests in the class share it, never their data.
    /// </summary>
    public sealed class RelationalDatabaseFixture
    {
        private readonly Dictionary<RelationalProvider, Lazy<Task<TestDatabase>>> databases;

        public RelationalDatabaseFixture(DatabaseServers servers)
        {
            databases = Enum.GetValues<RelationalProvider>().ToDictionary(
                provider => provider,
                provider => new Lazy<Task<TestDatabase>>(() => CreateFreshAsync(servers, provider)));
        }

        public Task<TestDatabase> GetAsync(RelationalProvider provider) => databases[provider].Value;

        private static async Task<TestDatabase> CreateFreshAsync(DatabaseServers servers, RelationalProvider provider)
        {
            var database = await TestDatabase.CreateEmptyAsync(servers, provider);
            await database.MigrateAsync<HarmonyContext>();
            return database;
        }
    }
}
