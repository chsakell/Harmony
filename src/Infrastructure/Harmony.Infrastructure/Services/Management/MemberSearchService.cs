using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Services.Management
{
    public class MemberSearchService : IMemberSearchService
    {
        private readonly UserManager<HarmonyUser> _userManager;
        private readonly IUserWorkspaceRepository _userWorkspaceRepository;

        public MemberSearchService(UserManager<HarmonyUser> userManager,
            IUserWorkspaceRepository userWorkspaceRepository)
        {
            _userManager = userManager;
            _userWorkspaceRepository = userWorkspaceRepository;
        }


        public async Task<List<UserWorkspaceResponse>> SearchWorkspaceUsers(Guid workspaceId, bool onlyMembers, string term, int pageNumber, int pageSize)
        {
            IQueryable<UserWorkspaceResponse> query = null;

            if (onlyMembers)
            {
                query = from user in _userManager.Users
                        join userWorkspace in _userWorkspaceRepository.Entities on user.Id equals userWorkspace.UserId
                        where userWorkspace.WorkspaceId == workspaceId && user.IsActive &&
                        (string.IsNullOrEmpty(term) ? true :
                            (user.FirstName.Contains(term) || user.LastName.Contains(term) ||
                             user.UserName.Contains(term) || user.Email.Contains(term)))
                        select new UserWorkspaceResponse()
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            ProfilePicture = user.ProfilePicture,
                            IsMember = true
                        };
            }
            else
            {
                query = from user in _userManager.Users
                        join userWorkspace in _userWorkspaceRepository.Entities
                            on user.Id equals userWorkspace.UserId into temp
                        from res in temp.DefaultIfEmpty()
                        where user.IsActive &&
                        (string.IsNullOrEmpty(term) ? true :
                            (user.FirstName.Contains(term) || user.LastName.Contains(term) ||
                             user.UserName.Contains(term) || user.Email.Contains(term)))
                        select new UserWorkspaceResponse()
                        {
                            Id = user.Id,
                            UserName = user.UserName,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            ProfilePicture = user.ProfilePicture,
                            IsMember = res.WorkspaceId == workspaceId
                        };
            }


            var result = await query.Skip((pageNumber - 1) * pageSize)
                                    .Take(pageSize).ToListAsync();

            return result;
        }
    }
}
