using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspace
{
    public class GetWorkspaceQuery : IRequest<IResult<WorkspaceDto>>
    {
        public Guid WorkspaceId { get; set; }

        public GetWorkspaceQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }
    }
}