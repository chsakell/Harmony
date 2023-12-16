using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Contracts.Services.Hubs;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDescription;

public class UpdateCardDescriptionCommandHandler : IRequestHandler<UpdateCardDescriptionCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly IHubClientNotifierService _hubClientNotifierService;
    private readonly IStringLocalizer<UpdateCardDescriptionCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardDescriptionCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardActivityService cardActivityService,
		IHubClientNotifierService hubClientNotifierService,
		IStringLocalizer<UpdateCardDescriptionCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardActivityService = cardActivityService;
        _hubClientNotifierService = hubClientNotifierService;
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

            await _hubClientNotifierService.UpdateCardDescription(request.BoardId, card.Id, card.Description);

            return await Result<bool>.SuccessAsync(true, _localizer["Description updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
