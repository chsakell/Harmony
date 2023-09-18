using Harmony.Application.Features.Workspaces.Queries.GetAll;
using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Workspaces.Queries.GetUserOwned
{
    public class GetUserOwnedWorkspacesQuery : IRequest<IResult<List<GetUserOwnedWorkspacesResponse>>>
    {
    }
}