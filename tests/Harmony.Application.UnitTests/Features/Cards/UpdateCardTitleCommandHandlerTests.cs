// Reference handler test: copy this pattern for new handler tests (roadmap R-05, R-15, R-16).
// - Build the handler directly with NSubstitute doubles; no MediatR pipeline, no DI container.
// - Stub only what the handler reads; assert the result and every outgoing call that matters.
// - The localizer echoes its key, so failure messages are readable in test output.

using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Models;
using Harmony.Application.Notifications;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Features.Cards
{
    public class UpdateCardTitleCommandHandlerTests
    {
        private const string UserId = "user-1";
        private const string NewTitle = "Fix login on Safari";
        private const string IndexName = "board-index";

        private static readonly DateTime DateUpdated = new(2026, 3, 5, 10, 30, 0, DateTimeKind.Utc);

        private readonly Guid _boardId = Guid.NewGuid();
        private readonly Guid _cardId = Guid.NewGuid();

        private readonly ICardRepository _cardRepository = Substitute.For<ICardRepository>();
        private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
        private readonly ICardActivityService _cardActivityService = Substitute.For<ICardActivityService>();
        private readonly ISearchService _searchService = Substitute.For<ISearchService>();
        private readonly INotificationsPublisher _notificationsPublisher = Substitute.For<INotificationsPublisher>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();
        private readonly IBoardService _boardService = Substitute.For<IBoardService>();
        private readonly IStringLocalizer<UpdateCardTitleCommandHandler> _localizer =
            Substitute.For<IStringLocalizer<UpdateCardTitleCommandHandler>>();

        private readonly UpdateCardTitleCommandHandler _handler;

        public UpdateCardTitleCommandHandlerTests()
        {
            _localizer[Arg.Any<string>()].Returns(call => new LocalizedString(call.Arg<string>(), call.Arg<string>()));

            _currentUserService.UserId.Returns(UserId);

            // The persistence layer sets DateUpdated in production; the handler reads it with .Value.
            _cardRepository.Get(_cardId).Returns(new Card { Id = _cardId, Title = "Old title", DateUpdated = DateUpdated });

            _boardService.GetBoardInfo(_boardId).Returns(new BoardInfo { Id = _boardId, IndexName = IndexName });

            _handler = new UpdateCardTitleCommandHandler(_cardRepository, _currentUserService, _cardActivityService,
                _searchService, _notificationsPublisher, _mapper, _boardService, _localizer);
        }

        [Theory]
        [Trait("Requirement", "FR-018")]
        [InlineData(null)]
        [InlineData("")]
        public async Task Handle_UserNotSignedIn_ReturnsFailedAndDoesNotSave(string? userId)
        {
            _currentUserService.UserId.Returns(userId);

            var result = await _handler.Handle(NewCommand(), TestContext.Current.CancellationToken);

            result.Succeeded.ShouldBeFalse();
            await _cardRepository.DidNotReceive().Update(Arg.Any<Card>());
            await _cardActivityService.DidNotReceiveWithAnyArgs()
                .CreateActivity(default, default!, default, default, default, default);
        }

        [Fact]
        [Trait("Requirement", "FR-019")]
        public async Task Handle_TitleSaved_ReturnsSuccessRecordsActivityAndPublishes()
        {
            _cardRepository.Update(Arg.Any<Card>()).Returns(1);

            var result = await _handler.Handle(NewCommand(), TestContext.Current.CancellationToken);

            result.Succeeded.ShouldBeTrue();
            await _cardRepository.Received(1).Update(Arg.Is<Card>(card => card.Id == _cardId && card.Title == NewTitle));
            await _cardActivityService.Received(1)
                .CreateActivity(_cardId, UserId, CardActivityType.CardTitleUpdated, DateUpdated, NewTitle);
            await _notificationsPublisher.Received(1).PublishSearchIndexNotificationAsync(
                Arg.Is<CardTitleUpdatedIndexNotification>(notification =>
                    notification.ObjectID == _cardId.ToString() && notification.Title == NewTitle),
                IndexName);
            await _notificationsPublisher.Received(1).PublishMessageAsync(
                Arg.Is<CardTitleChangedMessage>(message =>
                    message.BoardId == _boardId && message.CardId == _cardId && message.Title == NewTitle),
                NotificationType.CardTitleChanged,
                BrokerConstants.RoutingKeys.SignalR);
        }

        [Fact]
        [Trait("Requirement", "FR-020")]
        public async Task Handle_NothingSaved_ReturnsFailedAndPublishesNothing()
        {
            _cardRepository.Update(Arg.Any<Card>()).Returns(0);

            var result = await _handler.Handle(NewCommand(), TestContext.Current.CancellationToken);

            result.Succeeded.ShouldBeFalse();
            await _cardActivityService.DidNotReceiveWithAnyArgs()
                .CreateActivity(default, default!, default, default, default, default);
            _notificationsPublisher.ReceivedCalls().ShouldBeEmpty();
        }

        private UpdateCardTitleCommand NewCommand() => new(_cardId, NewTitle) { BoardId = _boardId };
    }
}
