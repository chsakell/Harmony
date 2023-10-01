using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class GetWorkspaceUsersHandler : IRequestHandler<GetWorkspaceUsersQuery, IResult<List<UserResponse>>>
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

        public async Task<IResult<List<UserResponse>>> Handle(GetWorkspaceUsersQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {
                return await Result<List<UserResponse>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userIds = await _userWorkspaceRepository.GetWorkspaceUsers(request.WorkspaceId);

            var users = await _userService.GetAllAsync(userIds);

            return await Result<List<UserResponse>>.SuccessAsync(users.Data);
        }
    }
}
