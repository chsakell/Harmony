using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.DTO.Automation;
using Harmony.Domain.Automation;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Automations.Queries.GetAutomations
{
    /// <summary>
    /// Handler for returning card labels
    /// </summary>
    public class GetAutomationsHandler : IRequestHandler<GetAutomationsQuery, IResult<IEnumerable<IAutomationDto>>>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetAutomationsHandler(
            IAutomationRepository automationRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _automationRepository = automationRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<IResult<IEnumerable<IAutomationDto>>> Handle(GetAutomationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<IAutomationDto>>.FailAsync("Login required to complete this operator");
            }

            var automations = await _automationRepository
                    .GetAutomations<SyncParentAndChildIssuesAutomationDto>(request.AutomationType, request.BoardId);

            return await Result<IEnumerable<IAutomationDto>>.SuccessAsync(automations);
        }
    }
}
