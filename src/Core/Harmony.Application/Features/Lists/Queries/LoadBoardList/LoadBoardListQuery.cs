using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Queries.LoadBoardList
{
    /// <summary>
    /// Query for returning list cards
    /// </summary>
    public class LoadBoardListQuery : IRequest<IResult<List<CardDto>>>
    {
        public Guid BoardId { get; set; }
        public Guid BoardListId { get; set; }
        public int PageSize { get; set; } = 5;
        public int Page { get; set; } = 1;

        public LoadBoardListQuery(Guid boardId, Guid boardListId, int page, int maxCardsPerList)
        {
            BoardId = boardId;
            BoardListId = boardListId;
            Page = page;
            PageSize = maxCardsPerList;
        }
    }
}