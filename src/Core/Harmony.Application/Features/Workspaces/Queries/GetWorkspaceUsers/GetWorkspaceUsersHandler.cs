using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class GetWorkspaceUsersHandler : IRequestHandler<GetWorkspaceUsersQuery, PaginatedResult<UserWorkspaceResponse>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IUserService _userService;
        private readonly IMemberSearchService _memberSearchService;
        private readonly IStringLocalizer<GetWorkspaceUsersHandler> _localizer;
        private readonly IMapper _mapper;

        public GetWorkspaceUsersHandler(IUserWorkspaceRepository userWorkspaceRepository,
            ICurrentUserService currentUserService,
            IUserService userService,
            IMemberSearchService memberSearchService,
            IStringLocalizer<GetWorkspaceUsersHandler> localizer,
            IMapper mapper)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
            _currentUserService = currentUserService;
            _userService = userService;
            _memberSearchService = memberSearchService;
            _localizer = localizer;
            _mapper = mapper;
        }

        public async Task<PaginatedResult<UserWorkspaceResponse>> Handle(GetWorkspaceUsersQuery request, CancellationToken cancellationToken)
        {
            request.PageNumber = request.PageNumber == 0 ? 1 : request.PageNumber;
            request.PageSize = request.PageSize == 0 ? 10 : request.PageSize;
            
            var totalWorkspaceUsers = request.MembersOnly ? 
                await _userWorkspaceRepository.CountWorkspaceUsers(request.WorkspaceId) :
                await _userService.GetCountAsync();

            var result = await _memberSearchService.SearchWorkspaceUsers(request.WorkspaceId,
                request.MembersOnly, request.SearchTerm, request.PageNumber, request.PageSize);

            return PaginatedResult<UserWorkspaceResponse>
                .Success(result, totalWorkspaceUsers, request.PageNumber, request.PageSize);
        }
    }
}
