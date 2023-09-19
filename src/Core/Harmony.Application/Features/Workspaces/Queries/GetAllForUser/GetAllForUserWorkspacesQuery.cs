using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Workspaces.Queries.GetAllForUser
{
    public class GetAllForUserWorkspacesQuery : IRequest<IResult<List<GetAllForUserWorkspaceResponse>>>
    {
    }
}