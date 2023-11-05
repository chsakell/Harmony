using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    /// <summary>
    /// Query for getting a board's users
    /// </summary>
    public class GetBoardUsersQuery : IRequest<Result<List<UserBoardResponse>>>
    {
        public Guid BoardId { get; set; }

        public GetBoardUsersQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}