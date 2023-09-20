using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony.Domain.Entities;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;


namespace Harmony.Application.Features.Cards.Commands.MoveCard;

public class MoveCardCommandHandler : IRequestHandler<MoveCardCommand, Result<CardDto>>
{
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly IStringLocalizer<MoveCardCommandHandler> _localizer;
	private readonly IMapper _mapper;

	public MoveCardCommandHandler(ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		IStringLocalizer<MoveCardCommandHandler> localizer,
		IMapper mapper)
	{
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
		_localizer = localizer;
		_mapper = mapper;
	}
	public async Task<Result<CardDto>> Handle(MoveCardCommand request, CancellationToken cancellationToken)
	{
		var userId = _currentUserService.UserId;

		if (string.IsNullOrEmpty(userId))
		{
			return await Result<CardDto>.FailAsync(_localizer["Login required to complete this operator"]);
		}

		var card = await _cardRepository.Get(request.CardId);


		card.BoardListId = request.ListId;

		// needs swapping
		if(card.Position != request.Position)
		{
			var currentPositionCard = await _cardRepository.GetByPosition(request.ListId, request.Position);
			if (currentPositionCard != null)
			{
				currentPositionCard.Position = card.Position;
				_cardRepository.UpdateEntry(currentPositionCard);
			}
		}

		card.Position = request.Position;

		// commit all the changes
		var dbResult = await _cardRepository.Update(card);

		if (dbResult > 0)
		{
			var result = _mapper.Map<CardDto>(card);
			return await Result<CardDto>.SuccessAsync(result, _localizer["Card Moved"]);
		}

		return await Result<CardDto>.FailAsync(_localizer["Operation failed"]);
	}
}
