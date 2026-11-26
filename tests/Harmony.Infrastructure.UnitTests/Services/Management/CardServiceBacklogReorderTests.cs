using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.Services.Management;
using Harmony.Infrastructure.UnitTests.TestSupport;
using MediatR;
using Microsoft.Extensions.Options;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.UnitTests.Services.Management
{
    /// <summary>
    /// Moving an issue up or down a backlog (the backlog page's arrows) must renumber that board's backlog only.
    /// Two boards with backlogs at the same positions; board A has two issue types, so the reorder must follow
    /// the board, not the issue type.
    /// </summary>
    [Trait("Bug", "11")]
    public class CardServiceBacklogReorderTests
    {
        private readonly Board _boardA = new() { Id = Guid.NewGuid(), Title = "Board A" };
        private readonly Board _boardB = new() { Id = Guid.NewGuid(), Title = "Board B" };

        private readonly List<IssueType> _issueTypes;
        private readonly List<Card> _cards;

        private readonly Card _a0;
        private readonly Card _a1;
        private readonly Card _a2;
        private readonly Card _b0;
        private readonly Card _b1;
        private readonly Card _b2;

        private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
        private readonly CardService _service;

        public CardServiceBacklogReorderTests()
        {
            var storyA = new IssueType { Id = Guid.NewGuid(), Summary = "Story", BoardId = _boardA.Id };
            var bugA = new IssueType { Id = Guid.NewGuid(), Summary = "Bug", BoardId = _boardA.Id };
            var storyB = new IssueType { Id = Guid.NewGuid(), Summary = "Story", BoardId = _boardB.Id };
            _issueTypes = [storyA, bugA, storyB];

            _a0 = BacklogCard("A-0", storyA, position: 0);
            _a1 = BacklogCard("A-1", bugA, position: 1);
            _a2 = BacklogCard("A-2", storyA, position: 2);
            _b0 = BacklogCard("B-0", storyB, position: 0);
            _b1 = BacklogCard("B-1", storyB, position: 1);
            _b2 = BacklogCard("B-2", storyB, position: 2);
            _cards = [_a0, _a1, _a2, _b0, _b1, _b2];

            _cardRepository.Entities.Returns(_ => _cards.AsAsyncQueryable());
            _cardRepository.Update(Arg.Any<Card>()).Returns(1);

            var issueTypeRepository = Substitute.For<IIssueTypeRepository>();
            issueTypeRepository.Entities.Returns(_ => _issueTypes.AsAsyncQueryable());

            var boardRepository = Substitute.For<IBoardRepository>();
            boardRepository.Entities.Returns(_ => new[] { _boardA, _boardB }.AsAsyncQueryable());

            _service = new CardService(
                _cardRepository,
                boardRepository,
                issueTypeRepository,
                Substitute.For<IBoardListRepository>(),
                Options.Create(new AppEndpointConfiguration()),
                Substitute.For<IMediator>());
        }

        [Fact]
        public async Task PositionCard_MoveUpInBacklog_ReordersOnlyThatBoard()
        {
            var moved = await _service.PositionCard(_a2, newListId: null, newPosition: 0, CardStatus.Backlog);

            moved.ShouldBeTrue();
            Positions(_a0, _a1, _a2).ShouldBe([1, 2, 0]);
            Positions(_b0, _b1, _b2).ShouldBe([0, 1, 2], "board B's backlog must not change");
        }

        [Fact]
        public async Task PositionCard_MoveDownInBacklog_ReordersOnlyThatBoard()
        {
            var moved = await _service.PositionCard(_a0, newListId: null, newPosition: 2, CardStatus.Backlog);

            moved.ShouldBeTrue();
            Positions(_a0, _a1, _a2).ShouldBe([2, 0, 1]);
            Positions(_b0, _b1, _b2).ShouldBe([0, 1, 2], "board B's backlog must not change");
        }

        private static Card BacklogCard(string title, IssueType issueType, short position) => new()
        {
            Id = Guid.NewGuid(),
            Title = title,
            IssueTypeId = issueType.Id,
            Status = CardStatus.Backlog,
            Position = position,
        };

        private static short[] Positions(params Card[] cards) => [.. cards.Select(card => card.Position)];
    }
}
