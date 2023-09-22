using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceQuery : IRequest<IResult<List<LoadWorkspaceResponse>>>
    {
        public Guid WorkspaceId { get; set; }

        public LoadWorkspaceQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }
    }
}