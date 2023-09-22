using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetAllForUser
{
    public class GetAllForUserWorkspacesQuery : IRequest<IResult<List<GetAllForUserWorkspaceResponse>>>
    {
    }
}