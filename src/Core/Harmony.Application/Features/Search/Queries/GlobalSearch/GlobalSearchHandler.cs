using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Search.Queries.GlobalSearch
{
    /// <summary>
    /// Handler for loading card
    /// </summary>
    public class GlobalSearchHandler : IRequestHandler<GlobalSearchQuery, IResult<GlobalSearchResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ISearchService _searchService;
        private readonly IStringLocalizer<GlobalSearchHandler> _localizer;
        private readonly IMapper _mapper;

        public GlobalSearchHandler(ICurrentUserService currentUserService,
            IUserService userService,
            ISearchService searchService,
            IStringLocalizer<GlobalSearchHandler> localizer,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _userService = userService;
            _searchService = searchService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<GlobalSearchResponse>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<GlobalSearchResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var result = await _searchService.SearchBoard(request.BoardId, request.Term);

            var test = new GlobalSearchResponse();

            return await Result<GlobalSearchResponse>.SuccessAsync(test);
        }
    }
}
