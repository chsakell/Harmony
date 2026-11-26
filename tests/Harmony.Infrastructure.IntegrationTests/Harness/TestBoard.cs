using Harmony.Domain.Entities;
using Harmony.Persistence.Identity;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// The usual starting point of a relational test: a new user who owns a new workspace with one board, one list
    /// and one issue type, all with unique IDs and keys (the test data rule).
    /// </summary>
    public sealed record TestBoard(TestDatabase Database, TestData Data, HarmonyUser User, Workspace Workspace,
        Board Board, BoardList List, IssueType IssueType)
    {
        public static async Task<TestBoard> CreateAsync(TestDatabase database)
        {
            var data = new TestData(database);
            var user = await data.UserAsync();
            var workspace = await data.WorkspaceAsync(user);
            var board = await data.BoardAsync(workspace, user);
            var list = await data.ListAsync(board, user);
            var issueType = await data.IssueTypeAsync(board);
            return new TestBoard(database, data, user, workspace, board, list, issueType);
        }
    }
}
