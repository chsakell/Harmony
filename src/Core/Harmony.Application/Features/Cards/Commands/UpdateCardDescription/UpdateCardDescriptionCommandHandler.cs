using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDescription;

public class UpdateCardDescriptionCommandHandler : IRequestHandler<UpdateCardDescriptionCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly IBoardService _boardService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IStringLocalizer<UpdateCardDescriptionCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardDescriptionCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardActivityService cardActivityService,
		IBoardService boardService,
		INotificationsPublisher notificationsPublisher,
		IStringLocalizer<UpdateCardDescriptionCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardActivityService = cardActivityService;
        _boardService = boardService;
        _notificationsPublisher = notificationsPublisher;
        _localizer = localizer;
		_mapper = mapper;
	}
	public async Task<Result<bool>> Handle(UpdateCardDescriptionCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);

		card.Description = request.Description;

		// commit all the changes
		var updateResult = await _cardRepository.Update(card);
        if (updateResult > 0)
        {
            await _cardActivityService.CreateActivity(card.Id, userId,
                CardActivityType.CardDescriptionUpdated, card.DateUpdated.Value);

            var board = await _boardService.GetBoardInfo(request.BoardId);

            _notificationsPublisher
                    .PublishSearchIndexNotification(new CardDescriptionUpdatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        Description = card.Description
                    }, board.IndexName);

            var message = new CardDescriptionChangedMessage(request.BoardId, request.CardId, card.Description);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardDescriptionChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

            return await Result<bool>.SuccessAsync(true, _localizer["Description updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
