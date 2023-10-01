using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers
{
    public class GetWorkspaceUsersQuery : IRequest<IResult<List<UserWorkspaceResponse>>>
    {
        public Guid WorkspaceId { get; set; }

        public GetWorkspaceUsersQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }
    }
}