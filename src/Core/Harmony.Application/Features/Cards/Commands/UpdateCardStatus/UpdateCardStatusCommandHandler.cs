using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardStatus;

public class UpdateCardStatusCommandHandler : IRequestHandler<UpdateCardStatusCommand, Result<bool>>
{
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardService _cardService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IBoardService _boardService;
    private readonly IStringLocalizer<UpdateCardStatusCommandHandler> _localizer;

	public UpdateCardStatusCommandHandler(
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardService cardService,
		INotificationsPublisher notificationsPublisher,
		IBoardService boardService,
		IStringLocalizer<UpdateCardStatusCommandHandler> localizer)
	{
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardService = cardService;
        _notificationsPublisher = notificationsPublisher;
        _boardService = boardService;
        _localizer = localizer;
	}
	public async Task<Result<bool>> Handle(UpdateCardStatusCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);

		card.Status = request.Status;

		if (card.BoardListId.HasValue && !card.ParentCardId.HasValue)
		{
			await _cardService.ReorderCardsAfterArchive(card.BoardListId.Value, card.Position);
		}
        // commit all the changes
		var updateResult = await _cardRepository.Update(card);

		if (updateResult > 0)
		{
            var board = await _boardService.GetBoardInfo(request.BoardId);

            _notificationsPublisher
                    .PublishSearchIndexNotification(new CardStatusUpdatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        Status = card.Status.ToString()
                    }, board.IndexName);

            var message = new CardStatusChangedMessage(request.BoardId, request.CardId, card.Status);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardStatusChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

            return await Result<bool>.SuccessAsync(true, _localizer["Status updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
