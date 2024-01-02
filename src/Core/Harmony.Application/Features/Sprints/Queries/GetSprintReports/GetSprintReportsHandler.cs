using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
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
        private readonly ISprintService _sprintService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<GetSprintReportsHandler> _localizer;

        public GetSprintReportsHandler(ISprintRepository sprintRepository,
            IBoardService boardService,
            ICardRepository cardRepository,
            ISprintService sprintService,
            IMapper mapper,
            IStringLocalizer<GetSprintReportsHandler> localizer)
        {
            _sprintRepository = sprintRepository;
            _boardService = boardService;
            _cardRepository = cardRepository;
            _sprintService = sprintService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IResult<GetSprintReportsResponse>> Handle(GetSprintReportsQuery request, CancellationToken cancellationToken)
        {
            var sprintReports = await _sprintService.GetSprintReports(request.SprintId);

            return Result<GetSprintReportsResponse>.Success(sprintReports);
        }
    }
}
