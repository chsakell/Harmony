using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Repositories
{
    public class CardRepositoryTests(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-020")]
        public async Task CountCards_ListWithSubtask_ExcludesSubtask(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: card);

            await using var services = new ServiceFactory(board.Database);
            var count = await services.CardRepository.CountCards(board.List.Id);

            count.ShouldBe(1);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-020")]
        public async Task CountActiveCards_ListWithSubtaskAndArchivedCard_CountsActiveTopLevelOnly(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: card);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, status: CardStatus.Archived, position: 1);

            await using var services = new ServiceFactory(board.Database);
            var count = await services.CardRepository.CountActiveCards(board.List.Id);

            count.ShouldBe(1);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-020")]
        public async Task Get_HiddenSubtask_ReturnsIt(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType);
            var subtask = await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: card);

            await using var services = new ServiceFactory(board.Database);
            var read = await services.CardRepository.Get(subtask.Id);

            read.ShouldNotBeNull();
            read.ParentCardId.ShouldBe(card.Id);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-020")]
        public async Task Load_HiddenSubtask_ReturnsIt(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType);
            var subtask = await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: card);

            await using var services = new ServiceFactory(board.Database);
            var read = await services.CardRepository.Load(subtask.Id);

            read.ShouldNotBeNull();
            read.ParentCardId.ShouldBe(card.Id);
        }

        /// <summary>
        /// Known-wrong (spec, Flagged concern 5e; roadmap R-22): the next serial number is a count of the board's
        /// cards plus one, so it would repeat a number if a card were ever deleted. This pins today's count.
        /// </summary>
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-033")]
        public async Task GetNextSerialNumber_MixedCards_ReturnsCountOfAllBoardCardsPlusOne_KnownIssue(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 0);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 1);
            await board.Data.CardAsync(board.User, issueType: board.IssueType, status: CardStatus.Backlog, position: 1);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: card);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, status: CardStatus.Archived, position: 2);
            var other = await NewBoardAsync(provider);
            await other.Data.CardAsync(other.User, other.List, other.IssueType);

            await using var services = new ServiceFactory(board.Database);
            var next = await services.CardRepository.GetNextSerialNumber(board.Board.Id);

            next.ShouldBe(6);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-034")]
        public async Task GetMaxActivePosition_EmptyList_ReturnsMinusOne(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);

            await using var services = new ServiceFactory(board.Database);
            var max = await services.CardRepository.GetMaxActivePosition(board.List.Id);

            max.ShouldBe((short)-1);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-034")]
        public async Task GetMaxActivePosition_ActiveAndArchived_ReturnsHighestActive(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var card = await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 0);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 1);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 4);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, status: CardStatus.Archived, position: 7);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: card, position: 9);

            await using var services = new ServiceFactory(board.Database);
            var max = await services.CardRepository.GetMaxActivePosition(board.List.Id);

            max.ShouldBe((short)4);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-034")]
        public async Task GetNextBacklogPosition_EmptyBacklog_ReturnsOne(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);

            await using var services = new ServiceFactory(board.Database);
            var next = await services.CardRepository.GetNextBacklogPosition(board.Board.Id);

            next.ShouldBe(1);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-034")]
        public async Task GetNextBacklogPosition_OtherBoardHigher_ReturnsOwnMaxPlusOne(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            foreach (var position in new short[] { 1, 2, 5 })
            {
                await board.Data.CardAsync(board.User, issueType: board.IssueType, status: CardStatus.Backlog, position: position);
            }

            var other = await NewBoardAsync(provider);
            await other.Data.CardAsync(other.User, issueType: other.IssueType, status: CardStatus.Backlog, position: 9);

            await using var services = new ServiceFactory(board.Database);
            var next = await services.CardRepository.GetNextBacklogPosition(board.Board.Id);

            next.ShouldBe(6);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-035")]
        public async Task GetTotalChildren_ActiveAndArchivedSubtasks_CountsActiveOnly(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var cardA = await CardWithSubtasksAsync(board);

            await using var services = new ServiceFactory(board.Database);
            var total = await services.CardRepository.GetTotalChildren(cardA.Id);

            total.ShouldBe(2);
        }

        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-035")]
        public async Task GetTotalChildren_Set_OmitsCardsWithoutChildren(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var cardA = await CardWithSubtasksAsync(board);
            var cardB = await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 1);

            await using var services = new ServiceFactory(board.Database);
            var totals = await services.CardRepository.GetTotalChildren([cardA.Id, cardB.Id]);

            totals.ShouldBe(new Dictionary<Guid, int> { [cardA.Id] = 2 });
        }

        /// <summary>
        /// Card A with 2 active subtasks and 1 archived subtask.
        /// </summary>
        private static async Task<Card> CardWithSubtasksAsync(TestBoard board)
        {
            var cardA = await board.Data.CardAsync(board.User, board.List, board.IssueType);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: cardA, position: 0);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: cardA, position: 1);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, parent: cardA, position: 2, status: CardStatus.Archived);
            return cardA;
        }

        private async Task<TestBoard> NewBoardAsync(RelationalProvider provider) =>
            await TestBoard.CreateAsync(await databases.GetAsync(provider));
    }
}
