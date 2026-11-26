using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Two classes that insert a board with the same key and run in parallel (xUnit runs test classes in parallel).
    /// Each must see only its own board, so each class has its own fresh database per provider (FR-006). Asserting on
    /// a shared key is the one documented exception to the test data rule: it is what proves the isolation.
    /// </summary>
    internal static class DatabaseIsolation
    {
        public const string SharedKey = "ISOL";

        public static async Task FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn(RelationalDatabaseFixture databases, RelationalProvider provider)
        {
            var database = await databases.GetAsync(provider);
            var data = new TestData(database);
            var owner = await data.UserAsync();
            var workspace = await data.WorkspaceAsync(owner);
            await data.BoardAsync(workspace, owner, key: SharedKey);

            await using var context = database.CreateContext<HarmonyContext>();
            var boards = await context.Boards.CountAsync(board => board.Key == SharedKey, TestContext.Current.CancellationToken);

            boards.ShouldBe(1);
        }
    }

    [Trait("Requirement", "FR-006")]
    public class DatabaseIsolationTestsA(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        [Theory]
        [BothProviders]
        public Task FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn(RelationalProvider provider) =>
            DatabaseIsolation.FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn(databases, provider);
    }

    [Trait("Requirement", "FR-006")]
    public class DatabaseIsolationTestsB(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        [Theory]
        [BothProviders]
        public Task FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn(RelationalProvider provider) =>
            DatabaseIsolation.FreshDatabase_BoardWithSharedKey_SeesOnlyItsOwn(databases, provider);
    }
}
