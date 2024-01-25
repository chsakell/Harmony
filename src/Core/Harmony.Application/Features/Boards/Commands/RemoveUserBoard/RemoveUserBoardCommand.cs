using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Commands.RemoveUserBoard
{
    /// <summary>
    /// Command to remove a member for a board
    /// </summary>
    public class RemoveUserBoardCommand : IRequest<Result<RemoveUserBoardResponse>>
    {
        public RemoveUserBoardCommand(Guid boardId, string userId)
        {
            BoardId = boardId;
            UserId = userId;
        }

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
    }
}
