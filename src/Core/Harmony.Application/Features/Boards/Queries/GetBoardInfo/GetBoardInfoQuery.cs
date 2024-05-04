using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetBoardInfo
{
    /// <summary>
    /// Query for getting a board's details
    /// </summary>
    public class GetBoardInfoQuery : IRequest<Result<BoardInfo>>
    {
        public Guid BoardId { get; set; }

        public GetBoardInfoQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}