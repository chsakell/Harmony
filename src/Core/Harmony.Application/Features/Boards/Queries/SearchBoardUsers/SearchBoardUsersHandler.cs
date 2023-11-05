using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Boards.Queries.SearchBoardUsers
{
    /// <summary>
    /// Handler for searching a board's users
    /// </summary>
    public class SearchBoardUsersUsersHandler : IRequestHandler<SearchBoardUsersQuery, Result<List<SearchBoardUserResponse>>>
    {
        private readonly IUserBoardRepository _userBoardRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IBoardRepository _boardRepository;
        private readonly IUserService _userService;
        private readonly IStringLocalizer<SearchBoardUsersUsersHandler> _localizer;
        private readonly IMapper _mapper;

        public SearchBoardUsersUsersHandler(IUserBoardRepository userBoardRepository,
            ICurrentUserService currentUserService,
            IBoardRepository boardRepository,
            IUserService userService,
            IStringLocalizer<SearchBoardUsersUsersHandler> localizer,
            IMapper mapper)
        {
            _userBoardRepository = userBoardRepository;
            _currentUserService = currentUserService;
            _boardRepository = boardRepository;
            _userService = userService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<Result<List<SearchBoardUserResponse>>> Handle(SearchBoardUsersQuery request, CancellationToken cancellationToken)
        {
            List<SearchBoardUserResponse> searchResult = new List<SearchBoardUserResponse>();
            var users = (await _userService.Search(request.SearchTerm)).Data;

            var boardMembers = await _userBoardRepository.GetBoardAccessMembers(request.BoardId);

            foreach(var user in users)
            {
                var userResponse = new SearchBoardUserResponse()
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    ProfilePicture = user.ProfilePicture,
                };

                var boardMember = boardMembers.FirstOrDefault(boardMember => boardMember.Id == user.Id);

                if(boardMember != null)
                {
                    userResponse.Access = boardMember.Access;
                    userResponse.IsMember = true;
                }

                searchResult.Add(userResponse);
            }

            return await Result<List<SearchBoardUserResponse>>.SuccessAsync(searchResult);
        }
    }
}
