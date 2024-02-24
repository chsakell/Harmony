using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Enums.Automations;
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
            return
                request.AutomationType switch
                {
                    AutomationType.SyncParentAndChildIssues => await 
                    GetAutomations<SyncParentAndChildIssuesAutomationDto>(request),
                    AutomationType.SmartAutoAssign => await GetAutomations<SmartAutoAssignAutomationDto>(request),
                        _ => throw new NotImplementedException(),
                };
        }

        private async Task<IResult<IEnumerable<T>>> GetAutomations<T>(GetAutomationsQuery request)
        {
            var automations = await _automationRepository
                    .GetAutomations<T>(request.AutomationType, request.BoardId);
            return await Result<IEnumerable<T>>.SuccessAsync(automations);
        }
    }
}
