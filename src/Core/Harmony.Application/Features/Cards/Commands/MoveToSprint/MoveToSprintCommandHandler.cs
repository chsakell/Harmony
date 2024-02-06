using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;

namespace Harmony.Application.Features.Cards.Commands.MoveToSprint
{
    /// <summary>
    /// Handler for moving cards from backlog to sprints
    /// </summary>
    public class MoveToSprintCommandHandler : IRequestHandler<MoveToSprintCommand, IResult<List<CardDto>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardService _cardService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<MoveToSprintCommandHandler> _localizer;

        public MoveToSprintCommandHandler(ICurrentUserService currentUserService,
            ICardService cardService,
            IMapper mapper, IStringLocalizer<MoveToSprintCommandHandler> localizer)
        {
            _currentUserService = currentUserService;
            _cardService = cardService;
            _mapper = mapper;
            _localizer = localizer;
        }
        public async Task<IResult<List<CardDto>>> Handle(MoveToSprintCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<CardDto>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var operationResult = await _cardService.MoveCardsToSprint(request.BoardId, request.Cards, request.SprintId, request.BoardListId);

            if (operationResult.Succeeded)
            {
                var result = _mapper.Map<List<CardDto>>(operationResult.Data);

                return await Result<List<CardDto>>.SuccessAsync(result, "Cards moved successfully");
            }

            return await Result<List<CardDto>>.FailAsync(_localizer["Operation failed"]);
        }
    }
}
