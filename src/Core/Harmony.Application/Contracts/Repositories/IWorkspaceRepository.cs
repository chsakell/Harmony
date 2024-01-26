using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access Workspaces
    /// </summary>
    public interface IWorkspaceRepository
    {
        IQueryable<Workspace> Entities { get; }
        Task<Workspace> GetAsync(Guid workspaceId);
        Task AddAsync(Workspace workspace);
        /// <summary>
        /// Create a workspace
        /// </summary>
        /// <param name="workspace"></param>
        /// <returns></returns>
        Task<int> CreateAsync(Workspace workspace);

        /// <summary>
        /// Returns workspaces created by userId plus have access to
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Workspace>> GetAllForUser(string userId);

        Task<List<Board>> LoadWorkspace(string userId, Guid workspaceId);
        Task<List<Board>> GetWorkspaceBoards(Guid workspaceId);
    }
}
