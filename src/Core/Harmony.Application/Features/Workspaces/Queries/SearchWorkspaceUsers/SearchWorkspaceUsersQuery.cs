using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.SearchWorkspaceUsers
{
    public class SearchWorkspaceUsersQuery : IRequest<IResult<List<SearchWorkspaceUserResponse>>>
    {
        public Guid WorkspaceId { get; set; }
        public string SearchTerm { get; set; }

        public SearchWorkspaceUsersQuery(Guid workspaceId, string searchTerm)
        {
            WorkspaceId = workspaceId;
            SearchTerm = searchTerm;
        }
    }
}