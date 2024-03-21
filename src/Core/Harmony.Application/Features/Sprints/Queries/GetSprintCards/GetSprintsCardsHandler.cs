using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Application.Specifications.Cards;
using Harmony.Application.Specifications.Sprints;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Sprints.Queries.GetSprintCards
{
    public class GetSprintCardsQueryHandler : IRequestHandler<GetSprintCardsQuery,
            PaginatedResult<CardDto>>
    {
        private readonly IBoardService _boardService;
        private readonly ISprintRepository _sprintRepository;
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly ISprintService _sprintService;
        private readonly IStringLocalizer<GetSprintReportsHandler> _localizer;

        public GetSprintCardsQueryHandler(IBoardService boardService,
            ISprintRepository sprintRepository,
            ICardRepository cardRepository,
            IMapper mapper,
            ISprintService sprintService,
            IStringLocalizer<GetSprintReportsHandler> localizer)
        {
            _boardService = boardService;
            _sprintRepository = sprintRepository;
            _cardRepository = cardRepository;
            _mapper = mapper;
            _sprintService = sprintService;
            _localizer = localizer;
        }

        public async Task<PaginatedResult<CardDto>> Handle(GetSprintCardsQuery request,
            CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;

            var includes = new CardIncludes()
            {
                IssueType = true,
                BoardList = true
            };

            var filter = new CardFilterSpecification(
                sprintId: request.SprintId, 
                includes, 
                status: request.Status, title: request.SearchTerm);

            var cards = await _cardRepository
                .Entities.Specify(filter)
                .Skip((request.PageNumber - 1) * request.PageSize)
                                    .Take(request.PageSize)
                .ToListAsync();

            var result = _mapper.Map<List<CardDto>>(cards);

            return PaginatedResult<CardDto>
                .Success(result, cards.Count, request.PageNumber, request.PageSize);
        }
    }
}
