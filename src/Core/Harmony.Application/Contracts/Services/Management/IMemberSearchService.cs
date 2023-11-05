using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Shared.Wrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Services.Management
{
    /// <summary>
    /// Service to searching users
    /// </summary>
    public interface IMemberSearchService
    {
        Task<List<UserWorkspaceResponse>> SearchWorkspaceUsers(Guid workspaceId, bool onlyMembers, string term, int pageNumber, int pageSize);
    }
}
