using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Queries.GetAllForUser
{
    public class GetAllForUserBoardsQuery : IRequest<IResult<List<GetAllForUserBoardResponse>>>
    {
        public Guid WorkspaceId { get; set; }

        public GetAllForUserBoardsQuery(Guid workspaceId)
        {
            WorkspaceId = workspaceId;
        }
    }
}