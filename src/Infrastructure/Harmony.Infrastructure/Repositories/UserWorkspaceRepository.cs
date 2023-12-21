using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Domain.Entities;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Infrastructure.Repositories
{
    public class UserWorkspaceRepository : IUserWorkspaceRepository
    {
        private readonly HarmonyContext _context;

        public UserWorkspaceRepository(HarmonyContext context)
        {
            _context = context;
        }

        public IQueryable<UserWorkspace> Entities => _context.Set<UserWorkspace>();

        public async Task<List<string>> GetWorkspaceUsers(Guid workspaceId, int pageNumber, int pageSize)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId)
                .Select(userWorkspace => userWorkspace.UserId)
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> CountWorkspaceUsers(Guid workspaceId)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId)
                .CountAsync();
        }

        public async Task AddAsync(UserWorkspace userWorkspaceRepository)
        {
            await _context.UserWorkspaces.AddAsync(userWorkspaceRepository);
        }

        public async Task<int> CreateAsync(UserWorkspace userWorkspaceRepository)
        {
            await _context.UserWorkspaces.AddAsync(userWorkspaceRepository);

            return await _context.SaveChangesAsync();
        }

        public async Task<List<string>> FindWorkspaceUsers(Guid workspaceId, List<string> userIds)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId
                    && userIds.Contains(userWorkspace.UserId))
                .Select(userworkspace => userworkspace.UserId)
                .ToListAsync();
        }

        public async Task<List<UserWorkspaceResponse>> SearchWorkspaceUsers(Guid workspaceId, string term, int pageNumber, int pageSize)
        {
            var workspaceUsers = await (from userWorkspace in _context.UserWorkspaces
                          join user in _context.Users
                          on userWorkspace.UserId equals user.Id
                          where userWorkspace.WorkspaceId == workspaceId 
                            && string.IsNullOrEmpty(term) ? true : (user.UserName.Contains(term) || user.FirstName.Contains(term) ||
                                user.LastName.Contains(term) || user.Email.Contains(term))
                          select new UserWorkspaceResponse
                          {
                              Id = user.Id,
                              UserName = user.UserName,
                              FirstName = user.FirstName,
                              LastName = user.LastName,
                              Email = user.Email,
                              EmailConfirmed = user.EmailConfirmed,
                              IsActive = user.IsActive,
                              IsMember = true,
                              PhoneNumber = user.PhoneNumber
                          })
                          .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                          .Distinct()
                          .ToListAsync();

            return workspaceUsers;
        }

        public async Task<List<UserWorkspaceResponse>> SearchWorkspaceUsers(Guid workspaceId, string term)
        {
            var workspaceUsers = await (from userWorkspace in _context.UserWorkspaces
                                        join user in _context.Users
                                        on userWorkspace.UserId equals user.Id
                                        where userWorkspace.WorkspaceId == workspaceId
                                          && string.IsNullOrEmpty(term) ? true : (user.UserName.Contains(term) || user.FirstName.Contains(term) ||
                                              user.LastName.Contains(term) || user.Email.Contains(term))
                                        select new UserWorkspaceResponse
                                        {
                                            Id = user.Id,
                                            UserName = user.UserName,
                                            FirstName = user.FirstName,
                                            LastName = user.LastName,
                                            Email = user.Email,
                                            EmailConfirmed = user.EmailConfirmed,
                                            IsActive = user.IsActive,
                                            IsMember = true,
                                            PhoneNumber = user.PhoneNumber,
                                            ProfilePicture = user.ProfilePicture
                                        })
                          .ToListAsync();

            return workspaceUsers;
        }

        public async Task<int> CountWorkspaceUsers(Guid workspaceId, string term, int pageNumber, int pageSize)
        {
            var totalWorkspaceUsers = await (from userWorkspace in _context.UserWorkspaces
                                        join user in _context.Users
                                        on userWorkspace.UserId equals user.Id
                                        where userWorkspace.WorkspaceId == workspaceId
                                          && string.IsNullOrEmpty(term) ? true : (user.UserName.Contains(term) || user.FirstName.Contains(term) ||
                                              user.LastName.Contains(term) || user.Email.Contains(term))
                                        select new UserWorkspaceResponse
                                        {
                                            Id = user.Id,
                                            UserName = user.UserName,
                                            FirstName = user.FirstName,
                                            LastName = user.LastName,
                                            Email = user.Email,
                                            EmailConfirmed = user.EmailConfirmed,
                                            IsActive = user.IsActive,
                                            IsMember = true,
                                            PhoneNumber = user.PhoneNumber
                                        })
                          .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                          .Distinct()
                          .CountAsync();

            return totalWorkspaceUsers;
        }

        public async Task<UserWorkspace?> GetUserWorkspace(Guid workspaceId, string userId)
        {
            return await _context.UserWorkspaces
                .Where(userWorkspace => userWorkspace.WorkspaceId == workspaceId
                    && userWorkspace.UserId == userId)
                .FirstOrDefaultAsync();
        }

        public IQueryable<Board> GetUserWorkspaceBoardsQuery(Guid? workspaceId, string userId)
        {
            var query = from UserWorkspace userWorkspace in _context.UserWorkspaces
                        join workspace in _context.Workspaces
                            on userWorkspace.WorkspaceId equals workspace.Id
                        join board in _context.Boards
                            on workspace.Id equals board.WorkspaceId
                        where userWorkspace.UserId == userId
                            && (userWorkspace.WorkspaceId == (workspaceId.HasValue ? workspaceId : userWorkspace.WorkspaceId)) && 
                            (board.Visibility == Domain.Enums.BoardVisibility.Workspace || board.Visibility == Domain.Enums.BoardVisibility.Public)
                        select board;

            return query;
        }

        public async Task<int> RemoveAsync(UserWorkspace userWorkspaceRepository)
        {
            _context.UserWorkspaces.Remove(userWorkspaceRepository);

            return await _context.SaveChangesAsync();
        }
    }
}
