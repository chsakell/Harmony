using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

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