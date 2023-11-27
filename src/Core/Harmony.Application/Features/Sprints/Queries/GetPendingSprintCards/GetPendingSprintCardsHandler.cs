using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Sprints.Queries.GetPendingSprintCards
{
    public class GetPendingSprintCardsHandler : IRequestHandler<GetPendingSprintCardsQuery, 
            IResult<GetPendingSprintCardsResponse>>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardService _boardService;
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<GetPendingSprintCardsHandler> _localizer;

        public GetPendingSprintCardsHandler(ISprintRepository sprintRepository,
            IBoardService boardService,
            ICardRepository cardRepository,
            IMapper mapper,
            IStringLocalizer<GetPendingSprintCardsHandler> localizer)
        {
            _sprintRepository = sprintRepository;
            _boardService = boardService;
            _cardRepository = cardRepository;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IResult<GetPendingSprintCardsResponse>> Handle(GetPendingSprintCardsQuery request, CancellationToken cancellationToken)
        {
            var pendingSprintCards = await _cardRepository.GetPendingSprintCards(request.SprintId);

            var nonCompletedSprints = await _sprintRepository.GetNonCompletedSprints(request.BoardId);

            var result = new GetPendingSprintCardsResponse()
            {
                PendingCards = _mapper.Map<List<CardDto>>(pendingSprintCards),
                AvailableSprints = _mapper.Map<List<SprintDto>>(nonCompletedSprints.Where(s => s.Id != request.SprintId))
            };

            return Result<GetPendingSprintCardsResponse>.Success(result);
        }
    }
}
