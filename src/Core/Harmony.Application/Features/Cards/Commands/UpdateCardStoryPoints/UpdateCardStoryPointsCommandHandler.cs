using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Contracts.Services.Hubs;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardStoryPoints;

public class UpdateCardStoryPointsCommandHandler : IRequestHandler<UpdateCardStoryPointsCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly IHubClientNotifierService _hubClientNotifierService;
    private readonly IStringLocalizer<UpdateCardStoryPointsCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardStoryPointsCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardActivityService cardActivityService,
		IHubClientNotifierService hubClientNotifierService,
		IStringLocalizer<UpdateCardStoryPointsCommandHandler> localizer,
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
            return await Result<bool>.SuccessAsync(true, _localizer["Story points updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
