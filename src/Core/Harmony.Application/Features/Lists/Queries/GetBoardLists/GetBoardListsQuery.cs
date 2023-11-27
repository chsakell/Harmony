using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Queries.GetBoardLists
{
    /// <summary>
    /// Query for returning board lists
    /// </summary>
    public class GetBoardListsQuery : IRequest<IResult<List<GetBoardListResponse>>>
    {
        public Guid BoardId { get; set; }

        public GetBoardListsQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}