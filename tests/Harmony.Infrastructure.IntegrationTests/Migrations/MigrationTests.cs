using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Migrations
{
    /// <summary>
    /// Each context's migrations, as its host applies them at startup, on both providers. Each case migrates its own
    /// empty database, so these tests do not use a fresh database per class (FR-006 makes them the exception).
    /// </summary>
    public class MigrationTests(DatabaseServers servers)
    {
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-010")]
        public Task HarmonyContext_MigrateEmptyDatabase_AppliesAllMigrationsInOrder(RelationalProvider provider) =>
            MigrateEmptyDatabase_AppliesAllMigrationsInOrder<HarmonyContext>(provider);

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-010")]
        public Task AutomationContext_MigrateEmptyDatabase_AppliesAllMigrationsInOrder(RelationalProvider provider) =>
            MigrateEmptyDatabase_AppliesAllMigrationsInOrder<AutomationContext>(provider);

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-010")]
        public Task NotificationContext_MigrateEmptyDatabase_AppliesAllMigrationsInOrder(RelationalProvider provider) =>
            MigrateEmptyDatabase_AppliesAllMigrationsInOrder<NotificationContext>(provider);

        /// <summary>
        /// EF Core 9 and later also throw from Migrate when the model has a pending change, so this guards host
        /// startup as well as the ADR rule that every schema change adds a migration for both providers.
        /// </summary>
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-011")]
        public void HarmonyContext_Model_HasNoPendingModelChanges(RelationalProvider provider) =>
            Model_HasNoPendingModelChanges<HarmonyContext>(provider);

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-011")]
        public void AutomationContext_Model_HasNoPendingModelChanges(RelationalProvider provider) =>
            Model_HasNoPendingModelChanges<AutomationContext>(provider);

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-011")]
        public void NotificationContext_Model_HasNoPendingModelChanges(RelationalProvider provider) =>
            Model_HasNoPendingModelChanges<NotificationContext>(provider);

        private async Task MigrateEmptyDatabase_AppliesAllMigrationsInOrder<TContext>(RelationalProvider provider)
            where TContext : DbContext
        {
            var database = await TestDatabase.CreateEmptyAsync(servers, provider);
            await using var context = database.CreateContext<TContext>();
            var migrations = context.Database.GetMigrations().ToList();

            await context.Database.MigrateAsync(TestContext.Current.CancellationToken);

            migrations.ShouldNotBeEmpty();
            var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
            applied.ShouldBe(migrations);
        }

        /// <summary>
        /// Compares the model with the provider's last migration snapshot; no database is opened.
        /// </summary>
        private static void Model_HasNoPendingModelChanges<TContext>(RelationalProvider provider)
            where TContext : DbContext
        {
            using var context = TestDatabase.CreateUnconnectedContext<TContext>(provider);

            context.Database.HasPendingModelChanges().ShouldBeFalse(
                $"{typeof(TContext).Name} has a schema change without a {provider} migration. "
                + "Add a migration to both migrations projects (docs/migrations.md).");
        }
    }
}
