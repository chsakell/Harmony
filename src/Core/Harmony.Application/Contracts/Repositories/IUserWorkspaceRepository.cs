using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IUserWorkspaceRepository
    {
        /// <summary>
        /// Create a user workspace
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        Task<int> CreateAsync(UserWorkspace userWorkspace);
        Task<int> CountWorkspaceUsers(Guid workspaceId);
        Task<List<string>> GetWorkspaceUsers(Guid workspaceId, int pageNumber, int pageSize);
        Task<List<string>> SearchWorkspaceUsers(Guid workspaceId, List<string> userIds);
        Task<int> RemoveAsync(UserWorkspace userWorkspace);
        Task<UserWorkspace?> GetUserWorkspace(Guid workspaceId, string userId);
    }
}
