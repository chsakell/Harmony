using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Queries.Get
{
    public class GetBoardQuery : IRequest<IResult<GetBoardResponse>>
    {
        public Guid BoardId { get; set; }

        public GetBoardQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}