using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Helpers;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Application.Notifications.Email;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDates;

public class UpdateCardDatesCommandHandler : IRequestHandler<UpdateCardDatesCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly IHubClientNotifierService _hubClientNotifierService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IStringLocalizer<UpdateCardDatesCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardDatesCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardActivityService cardActivityService,
		IHubClientNotifierService hubClientNotifierService,
		INotificationsPublisher notificationsPublisher,
		IStringLocalizer<UpdateCardDatesCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardActivityService = cardActivityService;
        _hubClientNotifierService = hubClientNotifierService;
        _notificationsPublisher = notificationsPublisher;
        _localizer = localizer;
		_mapper = mapper;
	}
	public async Task<Result<bool>> Handle(UpdateCardDatesCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);

		card.StartDate = request.StartDate;
		card.DueDate = request.DueDate;
		card.DueDateReminderType = request.DueDateReminderType;

		// commit all the changes
		var updateResult = await _cardRepository.Update(card);

		if (updateResult > 0)
		{
            await _cardActivityService.CreateActivity(card.Id, userId,
                CardActivityType.CardDatesUpdated, card.DateUpdated.Value,
                CardHelper.DisplayDates(card.StartDate, card.DueDate));

            await _hubClientNotifierService.UpdateCardDates(request.BoardId, card.Id, card.StartDate, card.DueDate);

			_notificationsPublisher.PublishEmailNotification(new CardDueTimeUpdatedNotification(card.Id));

            return await Result<bool>.SuccessAsync(true, _localizer["Dates updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
