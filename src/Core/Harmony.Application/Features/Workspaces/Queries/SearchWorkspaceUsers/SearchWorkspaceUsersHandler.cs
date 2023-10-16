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
                    ProfilePictureDataUrl   = workspaceUser.ProfilePictureDataUrl,
                    EmailConfirmed = workspaceUser.EmailConfirmed,
                    UserName = workspaceUser.UserName,
                };
                
                searchResult.Add(user);
            }

            return await Result<List<SearchWorkspaceUserResponse>>.SuccessAsync(searchResult);
        }
    }
}
