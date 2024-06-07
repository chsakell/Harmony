using AutoMapper;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    /// <summary>
    /// Handler for getting a board's users
    /// </summary>
    public class GetBoardUsersHandler : IRequestHandler<GetBoardUsersQuery, Result<List<UserBoardResponse>>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardRepository _boardRepository;
        private readonly IUserService _userService;
        private readonly ICacheService _cacheService;
        private readonly IStringLocalizer<GetBoardUsersHandler> _localizer;
        private readonly IMapper _mapper;

        public GetBoardUsersHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IBoardRepository boardRepository,
            IUserService userService,
            ICacheService cacheService,
            IStringLocalizer<GetBoardUsersHandler> localizer,
            IMapper mapper)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _boardRepository = boardRepository;
            _userService = userService;
            _cacheService = cacheService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<Result<List<UserBoardResponse>>> Handle(GetBoardUsersQuery request, CancellationToken cancellationToken)
        {
            var boardMembers = await _cacheService.GetOrCreateAsync(
            CacheKeys.BoardMembers(request.BoardId),
                async () => await _userBoardRepository.GetBoardAccessMembers(request.BoardId),
                TimeSpan.FromMinutes(5));

            return await Result<List<UserBoardResponse>>.SuccessAsync(boardMembers);
        }
    }
}
