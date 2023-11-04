using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.Get
{
    public class GetBoardQuery : IRequest<IResult<GetBoardResponse>>
    {
        public Guid BoardId { get; set; }
        public int MaxCardsPerList { get; set; } = 3;

        public GetBoardQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}