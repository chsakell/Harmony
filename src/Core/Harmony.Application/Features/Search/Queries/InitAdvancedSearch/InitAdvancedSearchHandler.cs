using AutoMapper;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Search.Queries.InitAdvancedSearch
{
    /// <summary>
    /// Handler for initializing advanced search
    /// </summary>
    public class InitAdvancedSearchHandler : IRequestHandler<InitAdvancedSearchQuery, IResult<InitAdvancedSearchResponse>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly ISearchService _searchService;
        private readonly IBoardService _boardService;
        private readonly IStringLocalizer<InitAdvancedSearchHandler> _localizer;
        private readonly IMapper _mapper;

        public InitAdvancedSearchHandler(ICurrentUserService currentUserService,
            IUserService userService,
            ISearchService searchService,
            IBoardService boardService,
            IStringLocalizer<InitAdvancedSearchHandler> localizer,
            IMapper mapper)
        {
            _currentUserService = currentUserService;
            _userService = userService;
            _searchService = searchService;
            _boardService = boardService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<IResult<InitAdvancedSearchResponse>> Handle(InitAdvancedSearchQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<InitAdvancedSearchResponse>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userBoards = await _boardService.GetUserBoards(null, userId);
            var boards = _mapper.Map<List<BoardDto>>(userBoards);

            var result = new InitAdvancedSearchResponse()
            {
                Boards = boards
            };

            return await Result<InitAdvancedSearchResponse>.SuccessAsync(result);
        }
    }
}
