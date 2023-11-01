using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceQuery : IRequest<IResult<LoadWorkspaceResponse>>
    {
        public Guid WorkspaceId { get; set; }

        public LoadWorkspaceQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }
    }
}