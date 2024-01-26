using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Automations.Queries.GetAutomations
{
    /// <summary>
    /// Handler for returning card labels
    /// </summary>
    public class GetAutomationsHandler : IRequestHandler<GetAutomationsQuery, IResult<IEnumerable<IAutomationDto>>>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly IMapper _mapper;

        public GetAutomationsHandler(
            IAutomationRepository automationRepository,
            IMapper mapper)
        {
            _automationRepository = automationRepository;
            _mapper = mapper;
        }

        public async Task<IResult<IEnumerable<IAutomationDto>>> Handle(GetAutomationsQuery request, CancellationToken cancellationToken)
        {
            var automations = await _automationRepository
                    .GetAutomations<SyncParentAndChildIssuesAutomationDto>(request.AutomationType, request.BoardId);

            return await Result<IEnumerable<SyncParentAndChildIssuesAutomationDto>>.SuccessAsync(automations);
        }
    }
}
