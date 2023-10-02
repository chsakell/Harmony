using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;
using Harmony.Application.Extensions;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class GetWorkspaceUsersHandler : IRequestHandler<GetWorkspaceUsersQuery, PaginatedResult<UserWorkspaceResponse>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly IStringLocalizer<GetWorkspaceUsersHandler> _localizer;
        private readonly IMapper _mapper;

        public GetWorkspaceUsersHandler(IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IUserService userService,
            IStringLocalizer<GetWorkspaceUsersHandler> localizer,
            IMapper mapper)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _userService = userService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<UserWorkspaceResponse>> Handle(GetWorkspaceUsersQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            Result<List<UserResponse>> users = null;
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;
            var totalWorkspaceUsers = await _userWorkspaceRepository.CountWorkspaceUsers(request.WorkspaceId);
            List<string> userIds = null;

            if (request.MembersOnly)
            {
                userIds = await _userWorkspaceRepository
                    .GetWorkspaceUsers(request.WorkspaceId, request.PageNumber, request.PageSize);

                users = await _userService.GetAllAsync(userIds);
            }
            else
            {
                users = await _userService.Search(request.SearchTerm, 
                            request.PageNumber, request.PageSize);

                userIds = await _userWorkspaceRepository
                    .SearchWorkspaceUsers(request.WorkspaceId, users.Data.Select(u => u.Id).ToList());
            }

            var workspaceUsers = _mapper.Map<List<UserWorkspaceResponse>>(users.Data);

            var result = PaginatedResult<UserWorkspaceResponse>
                .Success(workspaceUsers, totalWorkspaceUsers, request.PageNumber, request.PageSize);

            foreach (var user in workspaceUsers )
            {
                if(userIds.Contains(user.Id))
                {
                    user.IsMember = true;
                }
            }

            return result;
        }
    }
}
