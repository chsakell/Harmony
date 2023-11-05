using Harmony.Application.Contracts.Repositories;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers
{
    public class SearchWorkspaceUsersUsersHandler : IRequestHandler<SearchWorkspaceUsersQuery, IResult<List<SearchWorkspaceUserResponse>>>
    {
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;

        public SearchWorkspaceUsersUsersHandler(IUserWorkspaceRepository userWorkspaceRepository)
        {
            _userWorkspaceRepository = userWorkspaceRepository;
        }

        public async Task<IResult<List<SearchWorkspaceUserResponse>>> Handle(SearchWorkspaceUsersQuery request, CancellationToken cancellationToken)
        {
            List<SearchWorkspaceUserResponse> searchResult = new List<SearchWorkspaceUserResponse>();

            var workspaceUsers = await _userWorkspaceRepository
                .SearchWorkspaceUsers(request.WorkspaceId, request.SearchTerm);

            foreach ( var workspaceUser in workspaceUsers )
            {
                var user = new SearchWorkspaceUserResponse()
                {
                    Id = workspaceUser.Id,
                    FirstName = workspaceUser.FirstName,
                    LastName = workspaceUser.LastName,
                    Email = workspaceUser.Email,
                    ProfilePicture   = workspaceUser.ProfilePicture,
                    EmailConfirmed = workspaceUser.EmailConfirmed,
                    UserName = workspaceUser.UserName,
                };
                
                searchResult.Add(user);
            }

            return await Result<List<SearchWorkspaceUserResponse>>.SuccessAsync(searchResult);
        }
    }
}
