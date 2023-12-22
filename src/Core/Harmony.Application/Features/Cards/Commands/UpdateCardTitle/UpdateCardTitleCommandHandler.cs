using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Search;
using AutoMapper;
using Harmony.Application.DTO.Search;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardTitle;

public class UpdateCardTitleCommandHandler : IRequestHandler<UpdateCardTitleCommand, Result<bool>>
{
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly IHubClientNotifierService _hubClientNotifierService;
    private readonly ISearchService _searchService;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer<UpdateCardTitleCommandHandler> _localizer;

	public UpdateCardTitleCommandHandler(ICardRepository cardRepository,
		ICurrentUserService currentUserService,
        ICardActivityService cardActivityService,
        IHubClientNotifierService hubClientNotifierService,
		ISearchService searchService,
		IMapper mapper,
        IStringLocalizer<UpdateCardTitleCommandHandler> localizer)
	{
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardActivityService = cardActivityService;
        _hubClientNotifierService = hubClientNotifierService;
        _searchService = searchService;
        _mapper = mapper;
        _localizer = localizer;
	}
	public async Task<Result<bool>> Handle(UpdateCardTitleCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<bool>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);

		card.Title = request.Title;

		// commit all the changes
		var updateResult = await _cardRepository.Update(card);

		if (updateResult > 0)
		{
			await _cardActivityService.CreateActivity(card.Id, userId,
				CardActivityType.CardTitleUpdated, card.DateUpdated.Value, card.Title);

			var searchableCard = _mapper.Map<SearchableCard>(card);

			//await _searchService.UpdateCard(request.BoardId, searchableCard);
			await _hubClientNotifierService.UpdateCardTitle(request.BoardId, card.Id, card.Title);

			return await Result<bool>.SuccessAsync(true, _localizer["Title updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
