using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.DTO;
using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Queries.GetAutomationTemplates;
using Harmony.Application.Models;
using Harmony.Application.Specifications.Boards;
using Harmony.Domain.Entities;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public GetAutomationTemplatesHandler(
            IAutomationRepository automationRepository,
            ICurrentUserService currentUserService,
            IMemoryCache cache,
            IMapper mapper)
        {
            _automationRepository = automationRepository;
            _currentUserService = currentUserService;
            _cache = cache;
            _mapper = mapper;
        }

        public async Task<IResult<List<AutomationTemplateDto>>> Handle(GetAutomationTemplatesQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<AutomationTemplateDto>>.FailAsync("Login required to complete this operator");
            }

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
