using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Messaging;
using MediatR;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using Harmony.Application.Constants;

namespace Harmony.Application.Features.Cards.Commands.MoveCard;

public class MoveCardCommandHandler : IRequestHandler<MoveCardCommand, Result<CardDto>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IBoardService _boardService;
    private readonly IStringLocalizer<MoveCardCommandHandler> _localizer;
    private readonly IMapper _mapper;

	public MoveCardCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		INotificationsPublisher notificationsPublisher,
		ICurrentUserService currentUserService,
		IBoardService boardService,
		IStringLocalizer<MoveCardCommandHandler> localizer,
        IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
        _notificationsPublisher = notificationsPublisher;
        _currentUserService = currentUserService;
        _boardService = boardService;
        _localizer = localizer;
        _mapper = mapper;
	}
	public async Task<Result<CardDto>> Handle(MoveCardCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<CardDto>.FailAsync(_localizer["Login required to complete this operator"]);
		}

        var card = await _cardRepository.GetWithBoardList(request.CardId);
        var isChildIssue = card.ParentCardId.HasValue;
        var previousBoardListId = card.BoardListId;
		var previousPosition = card.Position;

        var boardId = card.BoardList?.BoardId;

        
        if(!request.Position.HasValue && request.ListId.HasValue)
        {
            if (isChildIssue)
            {
                card.BoardListId = request.ListId.Value;
                var updateResult = await _cardRepository.Update(card);

                await _cardRepository.LoadBoardListEntryAsync(card);

                if(updateResult > 0)
                {
                    var cardMovedNotification = new CardMovedMessage()
                    {
                        BoardId = request.BoardId,
                        CardId = request.CardId,
                        ParentCardId = card.ParentCardId,
                        FromPosition = previousPosition,
                        ToPosition = request.Position,
                        MovedFromListId = previousBoardListId.Value,
                        MovedToListId = card.BoardListId.Value,
                        IsCompleted = card.BoardList.CardStatus == BoardListCardStatus.DONE,
                        UpdateId = request.UpdateId,
                    };

                    _notificationsPublisher.PublishMessage(cardMovedNotification,
                        NotificationType.CardMoved, routingKey: BrokerConstants.RoutingKeys.Automation);

                    var result = _mapper.Map<CardDto>(card);

                    return await Result<CardDto>.SuccessAsync(result);
                }
            }
            else
            {
                // make this the last card in the list
                var totalCards = await _cardRepository.CountCards(request.ListId.Value);
                request.Position = (short)totalCards;
            }
        }

        // commit all the changes
        var operationCompleted = await _cardService
			.PositionCard(card, request.ListId, request.Position.Value, request.Status);

        if (operationCompleted)
		{
			await _cardRepository.LoadBoardListEntryAsync(card);

			if (card?.BoardList?.CardStatus == Domain.Enums.BoardListCardStatus.DONE)
			{
				card.DateCompleted = DateTime.Now;
				await _cardRepository.Update(card);

                _notificationsPublisher.PublishEmailNotification(new CardCompletedNotification(card.Id));
            }
			else if(card.DateCompleted.HasValue)
			{
				card.DateCompleted = null;
                await _cardRepository.Update(card);
            }

            var result = _mapper.Map<CardDto>(card);

            var board = await _boardService.GetBoardInfo(request.BoardId);

            if (previousBoardListId != request.ListId)
			{
                _notificationsPublisher
                    .PublishSearchIndexNotification(new CardListUpdatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        ListId = request.ListId?.ToString(),
                    }, board.IndexName);
            }

            if (previousBoardListId.HasValue && card.BoardListId.HasValue && card.BoardList != null)
            {
                var cardMovedNotification = new CardMovedMessage()
                {
                    BoardId = request.BoardId,
                    CardId = request.CardId,
                    ParentCardId = card.ParentCardId,
                    FromPosition = previousPosition,
                    ToPosition = request.Position,
                    MovedFromListId = previousBoardListId.Value,
                    MovedToListId = card.BoardListId.Value,
                    IsCompleted = card.BoardList.CardStatus == BoardListCardStatus.DONE,
                    UpdateId = request.UpdateId,
                };

                _notificationsPublisher.PublishMessage(cardMovedNotification,
                    NotificationType.CardMoved, routingKey: BrokerConstants.RoutingKeys.Notifications);
            }

            return await Result<CardDto>.SuccessAsync(result);
		}

		return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
	}
}
