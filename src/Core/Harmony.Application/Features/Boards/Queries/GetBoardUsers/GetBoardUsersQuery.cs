using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Queries.GetBoardUsers
{
    public class GetBoardUsersQuery : IRequest<Result<List<UserBoardResponse>>>
    {
        public Guid BoardId { get; set; }

        public GetBoardUsersQuery(Guid boardId)
        {
            BoardId = boardId;
        }
    }
}