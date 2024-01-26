using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardStoryPoints;

public class UpdateCardStoryPointsCommandHandler : IRequestHandler<UpdateCardStoryPointsCommand, Result<bool>>
{
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IStringLocalizer<UpdateCardStoryPointsCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardStoryPointsCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		INotificationsPublisher notificationsPublisher,
		IStringLocalizer<UpdateCardStoryPointsCommandHandler> localizer,
		IMapper mapper)
	{
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _notificationsPublisher = notificationsPublisher;
        _localizer = localizer;
		_mapper = mapper;
	}
	public async Task<Result<bool>> Handle(UpdateCardStoryPointsCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);

		card.StoryPoints = request.StoryPoints;

		// commit all the changes
		var updateResult = await _cardRepository.Update(card);

        if (updateResult > 0)
        {
            var message = new CardStoryPointsChangedMessage(request.BoardId, card.Id, card.StoryPoints);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardStoryPointsChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

            return await Result<bool>.SuccessAsync(true, _localizer["Story points updated"]);
        }

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
