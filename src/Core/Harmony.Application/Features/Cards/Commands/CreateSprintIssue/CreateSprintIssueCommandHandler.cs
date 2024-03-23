using Harmony.Shared.Wrapper;
using MediatR;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using AutoMapper;
using Harmony.Application.Contracts.Services.Management;

namespace Harmony.Application.Features.Cards.Commands.CreateSprintIssue
{
    public class CreateSprintIssueCommandHandler : IRequestHandler<CreateSprintIssueCommand, Result<CardDto>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly ICardService _cardService;
        private readonly IMapper _mapper;

        public CreateSprintIssueCommandHandler(ICurrentUserService currentUserService,
            ICardService cardService,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _cardService = cardService;
            _mapper = mapper;
        }
        public async Task<Result<CardDto>> Handle(CreateSprintIssueCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<CardDto>.FailAsync("Login required to complete this operator");
            }

            var operationResult = await _cardService
                .AddCardToSprint(
                userId: userId,
                sprintId: request.SprintId,
                issueTypeId: request.IssueType.Id,
                boardListId: request.BoardListId,
                title: request.Title);

            if (operationResult.Succeeded)
            {
                var result = _mapper.Map<CardDto>(operationResult.Data);

                return await Result<CardDto>.SuccessAsync(result, "Card added to sprint");
            }

            return await Result<CardDto>.FailAsync("Operation failed");
        }
    }
}
