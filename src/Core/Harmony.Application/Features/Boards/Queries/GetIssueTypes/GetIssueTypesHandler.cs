using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Workspaces.Queries.GetIssueTypes;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetIssueTypes
{
    public class GetIssueTypesHandler : IRequestHandler<GetIssueTypesQuery, IResult<List<IssueTypeDto>>>
    {
        private readonly IIssueTypeRepository _issueTypeRepository;
        private readonly IStringLocalizer<GetIssueTypesHandler> _localizer;
        private readonly IMemoryCache _memoryCache;
        private readonly IMapper _mapper;

        public GetIssueTypesHandler(IIssueTypeRepository issueTypeRepository,
            IStringLocalizer<GetIssueTypesHandler> localizer,
            IMemoryCache cache, IMapper mapper)
        {
            _issueTypeRepository = issueTypeRepository;
            _localizer = localizer;
            _memoryCache = cache;
            _mapper = mapper;
        }

        public async Task<IResult<List<IssueTypeDto>>> Handle(GetIssueTypesQuery request, CancellationToken cancellationToken)
        {
            var issueTypes = await GetIssueTypes(request.BoardId);

            var result = _mapper.Map<List<IssueTypeDto>>(issueTypes);

            return await Result<List<IssueTypeDto>>.SuccessAsync(request.NormalOnly ? result.Normal() : result);
        }

        private async Task<List<IssueType>> GetIssueTypes(Guid boardId)
        {
            return await _memoryCache.GetOrCreateAsync(CacheKeys.BoardIssueTypes(boardId),
            async cacheEntry =>
            {
                cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30);

                return await _issueTypeRepository.GetIssueTypes(boardId);
            });
        }
    }
}
