using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    /// <summary>
    /// Handler for returning card labels
    /// </summary>
    public class GetAutomationTemplatesHandler : IRequestHandler<GetAutomationTemplatesQuery, IResult<List<AutomationTemplateDto>>>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public GetAutomationTemplatesHandler(
            IAutomationRepository automationRepository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _automationRepository = automationRepository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<IResult<List<AutomationTemplateDto>>> Handle(GetAutomationTemplatesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<AutomationTemplateDto>>.FailAsync("Login required to complete this operator");
            }

            var templates = await _automationRepository.GetTemplates();

            var result = _mapper.Map<List<AutomationTemplateDto>>(templates);

            return await Result<List<AutomationTemplateDto>>.SuccessAsync(result);
        }
    }
}
