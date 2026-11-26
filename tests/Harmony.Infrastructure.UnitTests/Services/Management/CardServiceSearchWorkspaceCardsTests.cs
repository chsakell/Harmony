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
    /// The search behind "link an issue" must return the non-archived cards of the current workspace that match
    /// the term, minus the card given to skip, whether or not a card is skipped. Two workspaces, each with matching
    /// and non-matching cards, and one archived card in the searched workspace.
    /// </summary>
    [Trait("Bug", "17")]
    public class CardServiceSearchWorkspaceCardsTests
    {
        private readonly Guid _workspaceA = Guid.NewGuid();
        private readonly Guid _workspaceB = Guid.NewGuid();

        private readonly Card _loginA;
        private readonly Card _logoutA;
        private readonly Card _paymentsA;
        private readonly Card _archivedLoginA;
        private readonly Card _loginB;

        private readonly CardService _service;

        public CardServiceSearchWorkspaceCardsTests()
        {
            var boardA = new Board { Id = Guid.NewGuid(), Title = "Board A", Key = "A", WorkspaceId = _workspaceA };
            var boardB = new Board { Id = Guid.NewGuid(), Title = "Board B", Key = "B", WorkspaceId = _workspaceB };

            var storyA = new IssueType { Id = Guid.NewGuid(), Summary = "Story", BoardId = boardA.Id };
            var storyB = new IssueType { Id = Guid.NewGuid(), Summary = "Story", BoardId = boardB.Id };

            _loginA = NewCard("Fix login", storyA, position: 0);
            _logoutA = NewCard("Fix login timeout", storyA, position: 1);
            _paymentsA = NewCard("Payments page", storyA, position: 2);
            _archivedLoginA = NewCard("Old login", storyA, position: 3, CardStatus.Archived);
            _loginB = NewCard("Fix login on B", storyB, position: 0);

            var cards = new[] { _loginA, _logoutA, _paymentsA, _archivedLoginA, _loginB };

            var cardRepository = Substitute.For<ICardRepository>();
            cardRepository.Entities.Returns(_ => cards.AsAsyncQueryable());

            var issueTypeRepository = Substitute.For<IIssueTypeRepository>();
            issueTypeRepository.Entities.Returns(_ => new[] { storyA, storyB }.AsAsyncQueryable());

            var boardRepository = Substitute.For<IBoardRepository>();
            boardRepository.Entities.Returns(_ => new[] { boardA, boardB }.AsAsyncQueryable());

            _service = new CardService(
                cardRepository,
                boardRepository,
                issueTypeRepository,
                Substitute.For<IBoardListRepository>(),
                Options.Create(new AppEndpointConfiguration()),
                Substitute.For<IMediator>());
        }

        [Fact]
        public async Task SearchWorkspaceCards_NoCardSkipped_ReturnsMatchingCardsOfThatWorkspace()
        {
            var result = await _service.SearchWorkspaceCards(_workspaceA, "login", pageNumber: 1, pageSize: 10);

            result.Select(card => card.Id).ShouldBe([_loginA.Id, _logoutA.Id]);
        }

        [Fact]
        public async Task SearchWorkspaceCards_NoTermNoCardSkipped_ReturnsAllNonArchivedCardsOfThatWorkspace()
        {
            var result = await _service.SearchWorkspaceCards(_workspaceA, term: null!, pageNumber: 1, pageSize: 10);

            result.Select(card => card.Id).ShouldBe([_loginA.Id, _logoutA.Id, _paymentsA.Id]);
        }

        [Fact]
        public async Task SearchWorkspaceCards_CardSkipped_ReturnsMatchingCardsOfThatWorkspaceExceptSkipped()
        {
            var result = await _service.SearchWorkspaceCards(_workspaceA, "login", pageNumber: 1, pageSize: 10,
                skipCardId: _loginA.Id);

            result.Select(card => card.Id).ShouldBe([_logoutA.Id]);
        }

        [Fact]
        public async Task CountWorkspaceCards_NoCardSkipped_CountsMatchingCardsOfThatWorkspace()
        {
            var count = await _service.CountWorkspaceCards(_workspaceA, "login");

            count.ShouldBe(2);
        }

        [Fact]
        public async Task CountWorkspaceCards_CardSkipped_CountsMatchingCardsOfThatWorkspaceExceptSkipped()
        {
            var count = await _service.CountWorkspaceCards(_workspaceA, "login", skipCardId: _loginA.Id);

            count.ShouldBe(1);
        }

        private static Card NewCard(string title, IssueType issueType, short position,
            CardStatus status = CardStatus.Active) => new()
            {
                Id = Guid.NewGuid(),
                Title = title,
                IssueTypeId = issueType.Id,
                Status = status,
                Position = position,
            };
    }
}
