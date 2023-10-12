using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardDates;

public class UpdateCardDatesCommandHandler : IRequestHandler<UpdateCardDatesCommand, Result<bool>>
{
	private readonly ICardService _cardService;
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
    private readonly ICardActivityService _cardActivityService;
    private readonly IStringLocalizer<UpdateCardDatesCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public UpdateCardDatesCommandHandler(ICardService cardService,
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		ICardActivityService cardActivityService,
		IStringLocalizer<UpdateCardDatesCommandHandler> localizer,
		IMapper mapper)
	{
		_cardService = cardService;
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
        _cardActivityService = cardActivityService;
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

		// commit all the changes
		var updateResult = await _cardRepository.Update(card);

		if (updateResult > 0)
		{
            await _cardActivityService.CreateActivity(card.Id, userId,
                CardActivityType.CardDatesUpdated, card.DateUpdated.Value);

            return await Result<bool>.SuccessAsync(true, _localizer["Dates updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
