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

        Task<List<string>> GetWorkspaceUsers(Guid workspaceId);
    }
}
