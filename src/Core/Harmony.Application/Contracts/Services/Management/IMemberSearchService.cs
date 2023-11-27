using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;

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
