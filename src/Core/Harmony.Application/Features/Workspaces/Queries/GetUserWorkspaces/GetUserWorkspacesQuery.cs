using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetAllForUser
{
    public class GetUserWorkspacesQuery : IRequest<IResult<List<WorkspaceDto>>>
    {
    }
}