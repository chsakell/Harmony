using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardStatus;

public class UpdateCardStatusCommandHandler : IRequestHandler<UpdateCardStatusCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IStringLocalizer<UpdateCardStatusCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardStatusCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		INotificationsPublisher notificationsPublisher,
		IStringLocalizer<UpdateCardStatusCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _notificationsPublisher = notificationsPublisher;
        _localizer = localizer;
		_mapper = mapper;
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

        // commit all the changes
		var updateResult = await _cardRepository.Update(card);

		if (updateResult > 0)
		{
            _notificationsPublisher
                    .PublishSearchIndexNotification(new CardStatusUpdatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        BoardId = request.BoardId,
                        Status = card.Status.ToString()
                    });

            return await Result<bool>.SuccessAsync(true, _localizer["Status updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
