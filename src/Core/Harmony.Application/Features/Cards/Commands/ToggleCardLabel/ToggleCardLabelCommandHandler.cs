using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Entities;

namespace Harmony.Application.Features.Cards.Commands.ToggleCardLabel;

public class ToggleCardLabelCommandHandler : IRequestHandler<ToggleCardLabelCommand, Result<LabelDto>>
{
	private readonly ICardService _cardService;
    private readonly ICardLabelRepository _cardLabelRepository;
    private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly IStringLocalizer<ToggleCardLabelCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public ToggleCardLabelCommandHandler(ICardService cardService,
		ICardLabelRepository cardLabelRepository,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		IStringLocalizer<ToggleCardLabelCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
        _cardLabelRepository = cardLabelRepository;
        _cardRepository = cardRepository;
		_currentUserService = currentUserService;
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

		int dbResult = 0;
		LabelDto labelDto = null;

		if(cardLabel == null)
		{
			var label = await _cardLabelRepository.GetLabel(request.LabelId);

            dbResult = await _cardLabelRepository.CreateCardLabelAsync(new CardLabel()
			{
				CardId = request.CardId,
				LabelId = request.LabelId
			});

			labelDto = new LabelDto()
			{
				Id = label.Id,
				Colour = label.Colour,
				IsChecked = true,
				Title = label.Title
			};
        }
        else
        {
            dbResult = await _cardLabelRepository.DeleteCardLabel(cardLabel);
        }

		if (dbResult > 0)
		{
			return await Result<LabelDto>.SuccessAsync(labelDto, _localizer["Card label updated"]);
		}

		return await Result<LabelDto>.FailAsync(_localizer["Operation failed"]);
	}
}
