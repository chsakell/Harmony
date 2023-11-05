using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.Get
{
    /// <summary>
    /// Query for getting a board
    /// </summary>
    public class GetBoardQuery : IRequest<IResult<GetBoardResponse>>
    {
        public Guid BoardId { get; set; }
        public int MaxCardsPerList { get; set; }

        public GetBoardQuery(Guid boardId, int maxCardsPerList)
        {
            BoardId = boardId;
            MaxCardsPerList = maxCardsPerList;
        }
    }
}