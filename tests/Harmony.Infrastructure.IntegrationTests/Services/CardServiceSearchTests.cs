using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Services
{
    public class CardServiceSearchTests(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        private const string Term = "Harbour";

        /// <summary>
        /// The #17 case: with no skipped card the search returned nothing. Results include subtasks and backlog
        /// cards, leave out archived cards and other workspaces, and are ordered by (distinct) position.
        /// </summary>
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-041")]
        [Trait("Bug", "17")]
        public async Task SearchWorkspaceCards_NoSkippedCard_ReturnsNonArchivedMatchesInPositionOrderAndCountAgrees(RelationalProvider provider)
        {
            var search = await SearchDataAsync(provider);

            var (found, count) = await SearchAsync(search.Board, Term, skipCardId: null);

            found.Select(card => card.Id).ShouldBe(search.ExpectedInOrder.Select(card => card.Id));
            count.ShouldBe(found.Count);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-041")]
        [Trait("Bug", "17")]
        public async Task SearchWorkspaceCards_SkippedCard_LeavesItOut(RelationalProvider provider)
        {
            var search = await SearchDataAsync(provider);
            var skipped = search.ExpectedInOrder[1];

            var (found, count) = await SearchAsync(search.Board, Term, skipCardId: skipped.Id);

            found.Select(card => card.Id).ShouldBe(search.ExpectedInOrder.Where(card => card != skipped).Select(card => card.Id));
            count.ShouldBe(found.Count);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-041")]
        [Trait("Bug", "17")]
        public async Task SearchWorkspaceCards_Paging_SplitsAndCountsAll(RelationalProvider provider)
        {
            var search = await SearchDataAsync(provider);

            var (firstPage, count) = await SearchAsync(search.Board, Term, skipCardId: null, pageNumber: 1, pageSize: 2);
            var (secondPage, _) = await SearchAsync(search.Board, Term, skipCardId: null, pageNumber: 2, pageSize: 2);

            firstPage.Select(card => card.Id).ShouldBe(search.ExpectedInOrder.Take(2).Select(card => card.Id));
            secondPage.Select(card => card.Id).ShouldBe(search.ExpectedInOrder.Skip(2).Select(card => card.Id));
            count.ShouldBe(3);
        }

        /// <summary>
        /// Known-wrong (spec, FR-042 and Flagged concern 5c; roadmap "Known defects found by R-03"): the search
        /// matches titles with the server's default collation. SQL Server's is case-insensitive, PostgreSQL's is
        /// case-sensitive, so on PostgreSQL "login" does not find "Fix Login". The fix changes the PostgreSQL case to
        /// "found".
        /// </summary>
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-042")]
        public async Task SearchWorkspaceCards_TermDiffersInCase_CaseInsensitiveOnSqlServerOnly_KnownIssue(RelationalProvider provider)
        {
            var board = await TestBoard.CreateAsync(await databases.GetAsync(provider));
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType, title: "Fix Login");

            var (found, count) = await SearchAsync(board, "login", skipCardId: null);

            var expected = provider == RelationalProvider.SqlServer ? new[] { card.Id } : [];
            found.Select(result => result.Id).ShouldBe(expected);
            count.ShouldBe(expected.Length);
        }

        private static async Task<(List<CardDto> Found, int Count)> SearchAsync(TestBoard board, string term, Guid? skipCardId,
            int pageNumber = 1, int pageSize = 10)
        {
            await using var services = new ServiceFactory(board.Database);
            var found = await services.CardService.SearchWorkspaceCards(board.Workspace.Id, term, pageNumber, pageSize, skipCardId);
            var count = await services.CardService.CountWorkspaceCards(board.Workspace.Id, term, skipCardId);
            return (found, count);
        }

        /// <summary>
        /// In the board's workspace: a backlog card (position 1), a subtask (2) and an active card (3) that match,
        /// an archived match (4) and a card that does not match (0). Another workspace has a match at position 0.
        /// </summary>
        private async Task<(TestBoard Board, List<Card> ExpectedInOrder)> SearchDataAsync(RelationalProvider provider)
        {
            var database = await databases.GetAsync(provider);
            var board = await TestBoard.CreateAsync(database);
            var data = board.Data;
            var active = await data.CardAsync(board.User, board.List, board.IssueType, position: 3, title: $"Paint the {Term} wall");
            var backlog = await data.CardAsync(board.User, issueType: board.IssueType, status: CardStatus.Backlog, position: 1, title: $"{Term} lights");
            var subtask = await data.CardAsync(board.User, board.List, board.IssueType, parent: active, position: 2, title: $"Buy {Term} paint");
            await data.CardAsync(board.User, board.List, board.IssueType, status: CardStatus.Archived, position: 4, title: $"Old {Term} sign");
            await data.CardAsync(board.User, board.List, board.IssueType, position: 0, title: "Unrelated");

            var other = await TestBoard.CreateAsync(database);
            await other.Data.CardAsync(other.User, other.List, other.IssueType, position: 0, title: $"{Term} elsewhere");

            return (board, [backlog, subtask, active]);
        }
    }
}
