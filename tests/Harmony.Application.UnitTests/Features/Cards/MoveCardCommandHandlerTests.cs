using AutoMapper;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Models;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Features.Cards
{
    /// <summary>
    /// The backlog page's up and down arrows send a move without a board ID (#15). The board is only needed to
    /// update the search index when a card changes list, so a backlog reorder must not depend on it.
    /// </summary>
    [Trait("Bug", "15")]
    public class MoveCardCommandHandlerTests
    {
        private const string UserId = "user-1";
        private const string IndexName = "board-index";

        private readonly Guid _boardId = Guid.NewGuid();
        private readonly Guid _cardId = Guid.NewGuid();

        private readonly ICardService _cardService = Substitute.For<ICardService>();
        private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
        private readonly INotificationsPublisher _notificationsPublisher = Substitute.For<INotificationsPublisher>();
        private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
        private readonly IBoardService _boardService = Substitute.For<IBoardService>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IStringLocalizer<MoveCardCommandHandler> _localizer =
            Substitute.For<IStringLocalizer<MoveCardCommandHandler>>();

        private readonly MoveCardCommandHandler _handler;

        public MoveCardCommandHandlerTests()
        {
            _localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));
            _currentUserService.UserId.Returns(UserId);

            _cardService.PositionCard(Arg.Any<Card>(), Arg.Any<Guid?>(), Arg.Any<short>(), Arg.Any<CardStatus>())
                .Returns(true);

            // Like BoardService.GetBoardInfo, throw for a board that does not exist, such as Guid.Empty.
            var boardInfo = new BoardInfo { Id = _boardId, IndexName = IndexName };
            _boardService.GetBoardInfo(Arg.Any<Guid>()).Returns(call => call.Arg<Guid>() == _boardId
                ? Task.FromResult<BoardInfo?>(boardInfo)
                : throw new NullReferenceException("No board with this ID"));

            _handler = new MoveCardCommandHandler(_cardService, _cardRepository, _notificationsPublisher,
                _currentUserService, _boardService, _localizer, _mapper);
        }

        [Fact]
        public async Task Handle_ReorderInBacklogWithoutBoardId_ReturnsSuccess()
        {
            var card = new Card { Id = _cardId, Status = CardStatus.Backlog, Position = 2 };
            _cardRepository.GetWithBoardList(_cardId).Returns(card);

            // As sent by the backlog page: no list, no board ID.
            var command = new MoveCardCommand(_cardId, listId: null, position: 1, CardStatus.Backlog, Guid.NewGuid());

            var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

            result.Succeeded.ShouldBeTrue();
            await _cardService.Received(1).PositionCard(card, null, 1, CardStatus.Backlog);
        }

        [Fact]
        public async Task Handle_CardMovedToAnotherList_UpdatesTheBoardsSearchIndex()
        {
            var fromList = new BoardList { Id = Guid.NewGuid(), BoardId = _boardId, CardStatus = BoardListCardStatus.TODO };
            var toListId = Guid.NewGuid();
            var card = new Card
            {
                Id = _cardId,
                Status = CardStatus.Active,
                Position = 0,
                BoardList = fromList,
                BoardListId = fromList.Id,
            };
            _cardRepository.GetWithBoardList(_cardId).Returns(card);

            var command = new MoveCardCommand(_cardId, toListId, position: 0, CardStatus.Active, Guid.NewGuid())
            {
                BoardId = _boardId,
            };

            var result = await _handler.Handle(command, TestContext.Current.CancellationToken);

            result.Succeeded.ShouldBeTrue();
            await _notificationsPublisher.Received(1).PublishSearchIndexNotificationAsync(
                Arg.Is<CardListUpdatedIndexNotification>(notification =>
                    notification.ObjectID == _cardId.ToString() && notification.ListId == toListId.ToString()),
                IndexName);
        }
    }
}
