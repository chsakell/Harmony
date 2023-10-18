using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class GetWorkspaceUsersQuery : PagedRequest, IRequest<PaginatedResult<UserWorkspaceResponse>>
    {
        public Guid WorkspaceId { get; set; }
        public bool MembersOnly { get; set; }

        public GetWorkspaceUsersQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }

        public string SearchTerm { get; set; }

        public GetWorkspaceUsersQuery(Guid workspaceId, int pageNumber, int pageSize, 
            string searchTerm, string orderBy, bool membersOnly)
        {
            WorkspaceId = workspaceId;
            PageNumber = pageNumber;
            PageSize = pageSize;
            SearchTerm = searchTerm;

            if (!string.IsNullOrWhiteSpace(orderBy))
            {
                OrderBy = orderBy.Split(',');
            }

            MembersOnly = membersOnly;
        }
    }
}