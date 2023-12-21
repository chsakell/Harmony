using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using MediatR;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Domain.Entities;
using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Features.Cards.Commands.MoveCard;

public class MoveCardCommandHandler : IRequestHandler<MoveCardCommand, Result<CardDto>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly ICurrentUserService _currentUserService;
	private readonly IStringLocalizer<MoveCardCommandHandler> _localizer;
    private readonly IHubClientNotifierService _hubClientNotifierService;
    private readonly IMapper _mapper;

	public MoveCardCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		INotificationsPublisher notificationsPublisher,
		ICurrentUserService currentUserService,
		IStringLocalizer<MoveCardCommandHandler> localizer,
        IHubClientNotifierService hubClientNotifierService,
        IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
        _notificationsPublisher = notificationsPublisher;
        _currentUserService = currentUserService;
		_localizer = localizer;
        _hubClientNotifierService = hubClientNotifierService;
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

		var previousBoardListId = card.BoardListId;
		var previousPosition = card.Position;

        var boardId = card.BoardList?.BoardId;

        // commit all the changes
        var operationCompleted = await _cardService
			.PositionCard(card, request.ListId, request.Position, request.Status);

        if (operationCompleted)
		{
			await _cardRepository.LoadBoardListEntryAsync(card);

			if (card.BoardList.CardStatus == Domain.Enums.BoardListCardStatus.DONE)
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

            if (request.ListId.HasValue && boardId.HasValue)
            {
                await _hubClientNotifierService
                        .UpdateCardPosition(boardId.Value, request.CardId, 
						previousBoardListId.Value,request.ListId.Value, 
						previousPosition, request.Position, request.UpdateId);
            }

            return await Result<CardDto>.SuccessAsync(result);
		}

		return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
	}
}
