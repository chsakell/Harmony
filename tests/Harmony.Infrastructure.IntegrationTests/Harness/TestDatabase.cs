using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// A uniquely named database on the run's server for one provider. It starts empty (no tables); migrate it to
    /// make it a fresh database. It is not dropped: the server container is removed at the end of the run.
    /// </summary>
    public sealed class TestDatabase
    {
        private TestDatabase(RelationalProvider provider, string connectionString)
        {
            Provider = provider;
            ConnectionString = connectionString;
        }

        public RelationalProvider Provider { get; }

        public string ConnectionString { get; }

        public static async Task<TestDatabase> CreateEmptyAsync(DatabaseServers servers, RelationalProvider provider)
        {
            var name = $"h_{Guid.NewGuid():N}"[..10];
            var database = new TestDatabase(provider, await ConnectionStringForAsync(servers, provider, name));
            await using var context = database.CreateContext<Persistence.DbContext.HarmonyContext>();
            await context.GetService<IRelationalDatabaseCreator>().CreateAsync();
            return database;
        }

        /// <summary>
        /// A context configured for the provider whose connection is never opened, for checks on the model alone.
        /// </summary>
        public static TContext CreateUnconnectedContext<TContext>(RelationalProvider provider)
            where TContext : DbContext
        {
            var connectionString = provider switch
            {
                RelationalProvider.SqlServer => "Server=unused;Database=unused",
                RelationalProvider.PostgreSql => "Host=unused;Database=unused",
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
            };
            return Create<TContext>(provider, connectionString);
        }

        public async Task MigrateAsync<TContext>()
            where TContext : DbContext
        {
            await using var context = CreateContext<TContext>();
            await context.Database.MigrateAsync();
        }

        /// <summary>
        /// A new context on this database. Use one per unit of work, as the hosts do per request.
        /// </summary>
        public TContext CreateContext<TContext>()
            where TContext : DbContext => Create<TContext>(Provider, ConnectionString);

        private static async Task<string> ConnectionStringForAsync(DatabaseServers servers, RelationalProvider provider, string database)
        {
            switch (provider)
            {
                case RelationalProvider.SqlServer:
                    var sqlServer = new SqlConnectionStringBuilder((await servers.GetSqlServerAsync()).GetConnectionString());
                    sqlServer.InitialCatalog = database;
                    return sqlServer.ConnectionString;
                case RelationalProvider.PostgreSql:
                    var postgreSql = new NpgsqlConnectionStringBuilder((await servers.GetPostgreSqlAsync()).GetConnectionString());
                    postgreSql.Database = database;
                    return postgreSql.ConnectionString;
                default:
                    throw new ArgumentOutOfRangeException(nameof(provider), provider, null);
            }
        }

        private static TContext Create<TContext>(RelationalProvider provider, string connectionString)
            where TContext : DbContext =>
            (TContext)Activator.CreateInstance(typeof(TContext), ProviderSetup.For<TContext>(provider, connectionString))!;
    }
}
