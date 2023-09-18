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
        /// Returns workspaces created by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<List<Workspace>> GetUserOwnedWorkspaces(string userId);
    }
}
