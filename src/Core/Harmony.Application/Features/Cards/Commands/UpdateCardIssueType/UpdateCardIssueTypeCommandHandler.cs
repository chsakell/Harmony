using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardIssueType;

public class UpdateCardIssueTypeCommandHandler : IRequestHandler<UpdateCardIssueTypeCommand, Result<bool>>
{
	private readonly ICardRepository _cardRepository;
	private readonly ICurrentUserService _currentUserService;
	private readonly IStringLocalizer<UpdateCardIssueTypeCommandHandler> _localizer;

	public UpdateCardIssueTypeCommandHandler(
		ICardRepository cardRepository,
		ICurrentUserService currentUserService,
		IStringLocalizer<UpdateCardIssueTypeCommandHandler> localizer)
	{
		_cardRepository = cardRepository;
		_currentUserService = currentUserService;
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
			
			return await Result<bool>.SuccessAsync(true, _localizer["Issue type updated"]);
		}

		return await Result<bool>.FailAsync(_localizer["Operation failed"]);
	}
}
