using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetWorkspaceBoards;

public class GetWorkspaceBoardsQuery : IRequest<IResult<List<GetWorkspaceBoardResponse>>>
{
    public Guid WorkspaceId { get; set; }

    public GetWorkspaceBoardsQuery(Guid workspaceId)
    {
        WorkspaceId = workspaceId;
    }
}