using AutoMapper;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO.Search;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Search.Queries.GlobalSearch
{
    /// <summary>
    /// Handler for loading card
    /// </summary>
    public class GlobalSearchHandler : IRequestHandler<GlobalSearchQuery, IResult<List<SearchableCard>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ISearchService _searchService;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<GlobalSearchHandler> _localizer;
        private readonly IMapper _mapper;

        public GlobalSearchHandler(ICurrentUserService currentUserService,
            IUserService userService,
            ISearchService searchService,
            IBoardService boardService,
            IStringLocalizer<GlobalSearchHandler> localizer,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _userService = userService;
            _searchService = searchService;
            _boardService = boardService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<List<SearchableCard>>> Handle(GlobalSearchQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<SearchableCard>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _boardService.GetUserBoards(null, userId);

            if(!userBoards.Any())
            {
                return await Result<List<SearchableCard>>
                    .FailAsync(_localizer["There are no boards that you can access at the moment"]);
            }

            var boardIds = userBoards.Select(b => b.Id).ToList();

            var result = await _searchService.Search(boardIds, request.Term);

            return await Result<List<SearchableCard>>.SuccessAsync(result);
        }
    }
}
