using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Configures a context for a provider exactly as its host does in Extensions/ServiceCollectionExtensions.cs
    /// (NFR-005): Api for HarmonyContext, Automations for AutomationContext, Notifications for NotificationContext.
    /// ProviderSetupParityTests keeps this copy equal to the hosts. Detailed errors and sensitive data logging are
    /// also on in the hosts; they change only error messages.
    /// </summary>
    public static class ProviderSetup
    {
        public const string SqlServerMigrationsAssembly = "Harmony.Persistence.Migrations.SqlServer";
        public const string PostgreSqlMigrationsAssembly = "Harmony.Persistence.Migrations.PostgreSql";
        public const string LegacyTimestampSwitch = "Npgsql.EnableLegacyTimestampBehavior";

        /// <summary>
        /// The switch is process-wide and read when Npgsql is first used, so it is set when this assembly loads,
        /// before any test code runs, as the hosts set it before AddDbContext.
        /// </summary>
        [ModuleInitializer]
        internal static void EnableLegacyTimestampBehavior() => AppContext.SetSwitch(LegacyTimestampSwitch, true);

        public static DbContextOptions<TContext> For<TContext>(RelationalProvider provider, string connectionString)
            where TContext : DbContext
        {
            var options = new DbContextOptionsBuilder<TContext>();

            _ = provider switch
            {
                RelationalProvider.SqlServer => options.UseSqlServer(connectionString,
                    sqlServer => sqlServer.MigrationsAssembly(SqlServerMigrationsAssembly)),
                RelationalProvider.PostgreSql => options.UseNpgsql(connectionString,
                    npgsql => npgsql.MigrationsAssembly(PostgreSqlMigrationsAssembly)),
                _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, null),
            };

            options.EnableDetailedErrors();
            options.EnableSensitiveDataLogging();
            return options.Options;
        }
    }
}
