using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.LoadWorkspace
{
    public class LoadWorkspaceQuery : IRequest<IResult<List<BoardDto>>>
    {
        public Guid WorkspaceId { get; set; }

        public LoadWorkspaceQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }
    }
}