using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    /// <summary>
    /// Handler for returning card labels
    /// </summary>
    public class GetAutomationTemplatesHandler : IRequestHandler<GetAutomationTemplatesQuery, IResult<List<AutomationTemplateDto>>>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly ICacheService _cacheService;
        private readonly IMapper _mapper;

        public GetAutomationTemplatesHandler(
            IAutomationRepository automationRepository,
            ICacheService cacheService,
            IMapper mapper)
        {
            _automationRepository = automationRepository;
            _cacheService = cacheService;
            _mapper = mapper;
        }

        public async Task<IResult<List<AutomationTemplateDto>>> Handle(GetAutomationTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templates = await _cacheService.GetOrCreateAsync(CacheKeys.AutomationTemplates,
            async () =>
            {
                return await _automationRepository.GetTemplates();
            }, TimeSpan.FromMinutes(10));

            var result = _mapper.Map<List<AutomationTemplateDto>>(templates);

            return await Result<List<AutomationTemplateDto>>.SuccessAsync(result);
        }
    }
}
