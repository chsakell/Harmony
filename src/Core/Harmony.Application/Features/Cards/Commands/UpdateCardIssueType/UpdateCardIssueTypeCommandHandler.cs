using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.DTO;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardIssueType;

public class UpdateCardIssueTypeCommandHandler : IRequestHandler<UpdateCardIssueTypeCommand, Result<bool>>
{
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly IBoardService _boardService;
    private readonly IMapper _mapper;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IStringLocalizer<UpdateCardIssueTypeCommandHandler> _localizer;

	public UpdateCardIssueTypeCommandHandler(
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		IBoardService boardService,
		IMapper mapper, INotificationsPublisher notificationsPublisher,
		IStringLocalizer<UpdateCardIssueTypeCommandHandler> localizer)
	{
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _boardService = boardService;
        _mapper = mapper;
        _notificationsPublisher = notificationsPublisher;
        _localizer = localizer;
	}
	public async Task<Result<bool>> Handle(UpdateCardIssueTypeCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);

		card.IssueTypeId = request.IssueTypeId;

		// commit all the changes
		var updateResult = await _cardRepository.Update(card);
        if (updateResult > 0)
		{
			await _cardRepository.LoadIssueEntryAsync(card);
			var issueType = _mapper.Map<IssueTypeDto>(card.IssueType);

            var board = await _boardService.GetBoardInfo(request.BoardId);

            _notificationsPublisher
                    .PublishSearchIndexNotification(new CardIssueTypeUpdatedIndexNotification()
                    {
                        ObjectID = card.Id.ToString(),
                        IssueType = issueType.Summary
                    }, board.IndexName);

            var message = new CardIssueTypeChangedMessage(request.BoardId, request.CardId, issueType);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardIssueTypeChanged, routingKey: BrokerConstants.RoutingKeys.SignalR);

            return await Result<bool>.SuccessAsync(true, _localizer["Issue type updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
