using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Sprints.Queries.GetSprintReports
{
    public class GetSprintReportsHandler : IRequestHandler<GetSprintReportsQuery, 
            IResult<GetSprintReportsResponse>>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardService _boardService;
        private readonly ICardRepository _cardRepository;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<GetSprintReportsHandler> _localizer;

        public GetSprintReportsHandler(ISprintRepository sprintRepository,
            IBoardService boardService,
            ICardRepository cardRepository,
            IMapper mapper,
            IStringLocalizer<GetSprintReportsHandler> localizer)
        {
            _sprintRepository = sprintRepository;
            _boardService = boardService;
            _cardRepository = cardRepository;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IResult<GetSprintReportsResponse>> Handle(GetSprintReportsQuery request, CancellationToken cancellationToken)
        {

            var nonCompletedSprints = await _sprintRepository.GetNonCompletedSprints(request.BoardId);

            var result = new GetSprintReportsResponse()
            {
                
            };

            return Result<GetSprintReportsResponse>.Success(result);
        }
    }
}
