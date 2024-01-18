using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Domain.Entities;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

public class ToggleCardLabelCommandHandler : IRequestHandler<ToggleCardLabelCommand, Result<LabelDto>>
{
    private readonly ICardLabelRepository _cardLabelRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly INotificationsPublisher _notificationsPublisher;
    private readonly IStringLocalizer<ToggleCardLabelCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public ToggleCardLabelCommandHandler(ICardLabelRepository cardLabelRepository,
		ICurrentUserService currentUserService,
		INotificationsPublisher notificationsPublisher,
		IStringLocalizer<ToggleCardLabelCommandHandler> localizer,
		IMapper mapper)
	{
        _cardLabelRepository = cardLabelRepository;
		_currentUserService = currentUserService;
        _notificationsPublisher = notificationsPublisher;
        _localizer = localizer;
		_mapper = mapper;
	}
	public async Task<Result<LabelDto>> Handle(ToggleCardLabelCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<LabelDto>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var cardLabel = await _cardLabelRepository.GetLabel(request.CardId, request.LabelId);
        var label = await _cardLabelRepository.GetLabel(request.LabelId);

        int dbResult = 0;
		LabelDto labelDto = new LabelDto()
        {
            Id = label.Id,
            Colour = label.Colour,
            Title = label.Title
        };

		if(cardLabel == null)
		{
            dbResult = await _cardLabelRepository.CreateCardLabelAsync(new CardLabel()
			{
				CardId = request.CardId,
				LabelId = request.LabelId
			});

			labelDto.IsChecked = true;
        }
        else
        {
            dbResult = await _cardLabelRepository.DeleteCardLabel(cardLabel);
        }

        if (dbResult > 0)
		{
            var message = new CardLabelToggledMessage(request.BoardId, request.CardId, labelDto);

            _notificationsPublisher.PublishMessage(message,
                NotificationType.CardLabelToggled, routingKey: BrokerConstants.RoutingKeys.SignalR);

            return await Result<LabelDto>.SuccessAsync(labelDto, _localizer["Card label updated"]);
		}

		return await Result<LabelDto>.FailAsync(_localizer["Operation failed"]);
	}
}
