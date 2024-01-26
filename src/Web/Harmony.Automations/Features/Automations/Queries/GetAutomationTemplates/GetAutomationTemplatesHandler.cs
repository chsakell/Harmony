using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace Harmony.Application.Features.Cards.Queries.GetLabels
{
    /// <summary>
    /// Handler for returning card labels
    /// </summary>
    public class GetAutomationTemplatesHandler : IRequestHandler<GetAutomationTemplatesQuery, IResult<List<AutomationTemplateDto>>>
    {
        private readonly IAutomationRepository _automationRepository;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public GetAutomationTemplatesHandler(
            IAutomationRepository automationRepository,
            IMemoryCache cache,
            IMapper mapper)
        {
            _automationRepository = automationRepository;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<IResult<List<AutomationTemplateDto>>> Handle(GetAutomationTemplatesQuery request, CancellationToken cancellationToken)
        {
            var templates = await _cache.GetOrCreateAsync(CacheKeys.AutomationTemplates,
            async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);

                return await _automationRepository.GetTemplates(); ;
            });

            var result = _mapper.Map<List<AutomationTemplateDto>>(templates);

            return await Result<List<AutomationTemplateDto>>.SuccessAsync(result);
        }
    }
}
