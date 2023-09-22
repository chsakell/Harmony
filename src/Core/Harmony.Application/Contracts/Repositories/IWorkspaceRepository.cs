using Harmony.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IWorkspaceRepository
    {
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
    }
}
