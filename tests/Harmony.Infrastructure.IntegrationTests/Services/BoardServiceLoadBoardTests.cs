using Harmony.Application.Features.Boards.Queries.Get;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Services
{
    /// <summary>
    /// Loads a board as GetBoardHandler does: BoardService.GetBoard, then LoadBoard with the query and sprint.
    /// The order of cards within a list is not specified (FR-030), so lists are compared ignoring order.
    /// </summary>
    [Trait("Requirement", "FR-030")]
    public class BoardServiceLoadBoardTests(RelationalDatabaseFixture databases) : IClassFixture<RelationalDatabaseFixture>
    {
        [Theory]
        [BothProviders]
        [Trait("Requirement", "FR-020")]
        public async Task LoadBoard_FullBoard_ListsHoldActiveTopLevelCardsWithSummaries(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var data = board.Data;
            var secondList = await data.ListAsync(board.Board, board.User, position: 1);
            var cardA = await data.CardAsync(board.User, board.List, board.IssueType, position: 0);
            var cardB = await data.CardAsync(board.User, board.List, board.IssueType, position: 1);
            await data.CardAsync(board.User, board.List, board.IssueType, status: CardStatus.Archived, position: 2);
            await data.CardAsync(board.User, board.List, board.IssueType, parent: cardA, position: 3);
            var cardC = await data.CardAsync(board.User, secondList, board.IssueType, position: 0);
            var label = await data.LabelAsync(board.Board);
            await data.CardLabelAsync(cardA, label);
            await data.CardMemberAsync(cardA, board.User);
            await data.CheckListAsync(cardA, board.User, items: 3, checkedItems: 1);
            await data.CommentAsync(cardA, board.User);
            await data.LinkAsync(cardA, cardC, board.User);
            await data.AttachmentAsync(cardA, board.User);

            var loaded = await LoadAsync(board);

            CardIds(loaded, board.List).ShouldBe([cardA.Id, cardB.Id], ignoreOrder: true);
            CardIds(loaded, secondList).ShouldBe([cardC.Id]);
            var a = loaded.Lists.Single(list => list.Id == board.List.Id).Cards.Single(card => card.Id == cardA.Id);
            a.Children.Count.ShouldBe(1);
            a.Comments.Count.ShouldBe(1);
            a.Links.Count.ShouldBe(1);
            a.Attachments.Count.ShouldBe(1);
            var checkList = a.CheckLists.ShouldHaveSingleItem();
            checkList.Items.Count.ShouldBe(3);
            checkList.Items.Count(item => item.IsChecked).ShouldBe(1);
            a.Labels.Select(cardLabel => cardLabel.LabelId).ShouldBe([label.Id]);
            a.Members.Select(member => member.UserId).ShouldBe([board.User.Id]);
        }

        [Theory]
        [BothProviders]
        public async Task LoadBoard_SprintAndIssueTypeFilters_ReturnsOnlyCardsMatchingBoth(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var sprint = await board.Data.SprintAsync(board.Board);
            var otherSprint = await board.Data.SprintAsync(board.Board);
            var bug = await board.Data.IssueTypeAsync(board.Board, "Bug");
            await board.Data.CardAsync(board.User, board.List, bug, sprint: sprint, position: 0);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, sprint: otherSprint, position: 1);
            var both = await board.Data.CardAsync(board.User, board.List, board.IssueType, sprint: sprint, position: 2);

            var loaded = await LoadAsync(board, sprintId: sprint.Id, issueTypeIds: [board.IssueType.Id]);

            CardIds(loaded, board.List).ShouldBe([both.Id]);
        }

        [Theory]
        [BothProviders]
        public async Task LoadBoard_SprintFilter_ReturnsOnlyThatSprintsCards(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var sprint = await board.Data.SprintAsync(board.Board);
            var otherSprint = await board.Data.SprintAsync(board.Board);
            var inSprint = await board.Data.CardAsync(board.User, board.List, board.IssueType, sprint: sprint, position: 0);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, sprint: otherSprint, position: 1);

            var loaded = await LoadAsync(board, sprintId: sprint.Id);

            CardIds(loaded, board.List).ShouldBe([inSprint.Id]);
        }

        [Theory]
        [BothProviders]
        public async Task LoadBoard_IssueTypeFilter_ReturnsOnlyCardsOfThoseTypes(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var bug = await board.Data.IssueTypeAsync(board.Board, "Bug");
            var epic = await board.Data.IssueTypeAsync(board.Board, "Epic");
            var task = await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 0);
            var bugCard = await board.Data.CardAsync(board.User, board.List, bug, position: 1);
            await board.Data.CardAsync(board.User, board.List, epic, position: 2);

            var loaded = await LoadAsync(board, issueTypeIds: [board.IssueType.Id, bug.Id]);

            CardIds(loaded, board.List).ShouldBe([task.Id, bugCard.Id], ignoreOrder: true);
        }

        [Theory]
        [BothProviders]
        public async Task LoadBoard_AssigneeFilter_ReturnsOnlyCardsAssignedToThem(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            var assignee = await board.Data.UserAsync();
            var other = await board.Data.UserAsync();
            var assigned = await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 0);
            var otherCard = await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 1);
            await board.Data.CardAsync(board.User, board.List, board.IssueType, position: 2);
            await board.Data.CardMemberAsync(assigned, assignee);
            await board.Data.CardMemberAsync(otherCard, other);

            var loaded = await LoadAsync(board, assignees: [assignee.Id]);

            CardIds(loaded, board.List).ShouldBe([assigned.Id]);
        }

        /// <summary>
        /// Known-wrong (spec, Flagged concern 5a; roadmap R-22): LoadBoard passes MaxCardsPerList on, but
        /// GetBoardListCards never applies it, so every active card of each list is returned.
        /// </summary>
        [Theory]
        [BothProviders]
        public async Task LoadBoard_CardsPerListLimit_ReturnsAllCards_KnownIssue(RelationalProvider provider)
        {
            var board = await NewBoardAsync(provider);
            for (short position = 0; position < 5; position++)
            {
                await board.Data.CardAsync(board.User, board.List, board.IssueType, position: position);
            }

            var loaded = await LoadAsync(board, maxCardsPerList: 2);

            CardIds(loaded, board.List).Count.ShouldBe(5);
        }

        private static async Task<Board> LoadAsync(TestBoard board, Guid? sprintId = null,
            List<Guid>? issueTypeIds = null, List<string>? assignees = null, int maxCardsPerList = 20)
        {
            await using var services = new ServiceFactory(board.Database);
            var loaded = await services.BoardService.GetBoard(board.Board.Id);
            loaded.ShouldNotBeNull();

            return await services.BoardService.LoadBoard(loaded, new GetBoardQuery
            {
                BoardId = board.Board.Id,
                MaxCardsPerList = maxCardsPerList,
                SprintId = sprintId,
                IssueTypeIds = issueTypeIds,
                Assignees = assignees ?? [],
                ListIds = [],
            }, sprintId);
        }

        private static List<Guid> CardIds(Board loaded, BoardList list) =>
            loaded.Lists.Single(boardList => boardList.Id == list.Id).Cards.Select(card => card.Id).ToList();

        private async Task<TestBoard> NewBoardAsync(RelationalProvider provider) =>
            await TestBoard.CreateAsync(await databases.GetAsync(provider));
    }
}
