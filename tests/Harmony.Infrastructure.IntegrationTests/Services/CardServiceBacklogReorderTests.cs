using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Services
{
    /// <summary>
    /// Moving a backlog card renumbers only its own board's backlog (#11, here against real SQL). Boards X and Y
    /// both have backlog cards at positions 1 to 4; X also has an active card in a list at position 2.
    /// </summary>
    [Trait("Requirement", "FR-040")]
    [Trait("Bug", "11")]
    public class CardServiceBacklogReorderTests(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        [Theory]
        [BothProviders]
        public async Task PositionCard_MoveUpInBacklog_ShiftsOnlySameBoard(RelationalProvider provider)
        {
            var (database, x, y, inList) = await TwoBacklogsAsync(provider);

            await MoveAsync(database, x[3], newPosition: 2);

            (await PositionsAsync(database, x)).ShouldBe([1, 3, 4, 2]);
            (await PositionsAsync(database, y)).ShouldBe([1, 2, 3, 4], "board Y's backlog must not change");
            (await PositionsAsync(database, [inList])).ShouldBe([2], "a card in a list is not in the backlog");
        }

        [Theory]
        [BothProviders]
        public async Task PositionCard_MoveDownInBacklog_ShiftsOnlySameBoard(RelationalProvider provider)
        {
            var (database, x, y, inList) = await TwoBacklogsAsync(provider);

            await MoveAsync(database, x[1], newPosition: 4);

            (await PositionsAsync(database, x)).ShouldBe([1, 4, 2, 3]);
            (await PositionsAsync(database, y)).ShouldBe([1, 2, 3, 4], "board Y's backlog must not change");
            (await PositionsAsync(database, [inList])).ShouldBe([2], "a card in a list is not in the backlog");
        }

        /// <summary>
        /// As the move handler does: read the card and position it in one scope.
        /// </summary>
        private static async Task MoveAsync(TestDatabase database, Card card, short newPosition)
        {
            await using var services = new ServiceFactory(database);
            var tracked = await services.CardRepository.Get(card.Id);
            tracked.ShouldNotBeNull();

            var moved = await services.CardService.PositionCard(tracked, newListId: null, newPosition, CardStatus.Backlog);

            moved.ShouldBeTrue();
        }

        private static async Task<List<short>> PositionsAsync(TestDatabase database, List<Card> cards)
        {
            await using var context = database.CreateContext<HarmonyContext>();
            var ids = cards.Select(card => card.Id).ToList();
            var positions = await context.Cards.IgnoreQueryFilters()
                .Where(card => ids.Contains(card.Id))
                .ToDictionaryAsync(card => card.Id, card => card.Position, TestContext.Current.CancellationToken);
            return ids.Select(id => positions[id]).ToList();
        }

        private async Task<(TestDatabase Database, List<Card> X, List<Card> Y, Card InList)> TwoBacklogsAsync(RelationalProvider provider)
        {
            var database = await databases.GetAsync(provider);
            var boardX = await TestBoard.CreateAsync(database);
            var boardY = await TestBoard.CreateAsync(database);
            var x = await BacklogAsync(boardX);
            var y = await BacklogAsync(boardY);
            var inList = await boardX.Data.CardAsync(boardX.User, boardX.List, boardX.IssueType, position: 2);
            return (database, x, y, inList);
        }

        private static async Task<List<Card>> BacklogAsync(TestBoard board)
        {
            var cards = new List<Card>();
            for (short position = 1; position <= 4; position++)
            {
                cards.Add(await board.Data.CardAsync(board.User, issueType: board.IssueType, status: CardStatus.Backlog, position: position));
            }

            return cards;
        }
    }
}
