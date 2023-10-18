using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Entities;
using Harmony.Application.Contracts.Services.Hubs;

namespace Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

public class ToggleCardLabelCommandHandler : IRequestHandler<ToggleCardLabelCommand, Result<LabelDto>>
{
	private readonly ICardService _cardService;
    private readonly ICardLabelRepository _cardLabelRepository;
    private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly IHubClientNotifierService _hubClientNotifierService;
    private readonly IStringLocalizer<ToggleCardLabelCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public ToggleCardLabelCommandHandler(ICardService cardService,
		ICardLabelRepository cardLabelRepository,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		IHubClientNotifierService hubClientNotifierService,
		IStringLocalizer<ToggleCardLabelCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
        _cardLabelRepository = cardLabelRepository;
        _cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _hubClientNotifierService = hubClientNotifierService;
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
            var boardId = await _cardRepository.GetBoardId(request.CardId);
			await _hubClientNotifierService.ToggleCardLabel(boardId, request.CardId, labelDto);

            return await Result<LabelDto>.SuccessAsync(labelDto, _localizer["Card label updated"]);
		}

		return await Result<LabelDto>.FailAsync(_localizer["Operation failed"]);
	}
}
