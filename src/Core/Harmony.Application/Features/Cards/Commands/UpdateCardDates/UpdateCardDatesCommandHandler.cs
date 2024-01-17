using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Helpers;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDates;

public class UpdateCardDatesCommandHandler : IRequestHandler<UpdateCardDatesCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IBoardService _boardService;
    private readonly IStringLocalizer<UpdateCardDatesCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardDatesCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardActivityService cardActivityService,
		INotificationsPublisher notificationsPublisher,
		IBoardService boardService,
		IStringLocalizer<UpdateCardDatesCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardActivityService = cardActivityService;
        _notificationsPublisher = notificationsPublisher;
        _boardService = boardService;
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

			_notificationsPublisher.PublishEmailNotification(new CardDueTimeUpdatedNotification(card.Id));

            var board = await _boardService.GetBoardInfo(request.BoardId);

            DateTime sTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long? unixTime = card.DueDate.HasValue ? (long)(card.DueDate.Value - sTime).TotalSeconds : null;

            _notificationsPublisher
                    .PublishSearchIndexNotification(new CardDueDateUpdatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        DueDate = unixTime
                    }, board.IndexName);

            var message = new CardDatesChangedMessage(request.BoardId, request.CardId, card.StartDate, card.DueDate);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardDatesChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

            return await Result<bool>.SuccessAsync(true, _localizer["Dates updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
