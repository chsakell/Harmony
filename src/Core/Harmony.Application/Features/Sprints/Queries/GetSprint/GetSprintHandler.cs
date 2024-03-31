using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Specifications.Sprints;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Sprints.Queries.GetSprint
{
    public class GetSprintHandler : IRequestHandler<GetSprintQuery, 
            IResult<SprintDto>>
    {
        private readonly ISprintRepository _sprintRepository;
        private readonly IBoardService _boardService;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<GetSprintHandler> _localizer;

        public GetSprintHandler(ISprintRepository sprintRepository,
            IBoardService boardService,
            IMapper mapper,
            IStringLocalizer<GetSprintHandler> localizer)
        {
            _sprintRepository = sprintRepository;
            _boardService = boardService;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<IResult<SprintDto>> Handle(GetSprintQuery request, CancellationToken cancellationToken)
        {
            var filter = new SprintFilterSpecification()
            {
                SprintId = request.SprintId,
                IncludeRetrospective = true
            };

            filter.Build();

            var sprint = await _sprintRepository
                .Entities.Specify(filter)
                .FirstOrDefaultAsync();

            var result = _mapper.Map<SprintDto>(sprint);

            return Result<SprintDto>.Success(result);
        }
    }
}
