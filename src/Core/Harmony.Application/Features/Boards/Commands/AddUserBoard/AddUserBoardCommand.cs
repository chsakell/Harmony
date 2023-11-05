using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Commands.AddUserBoard
{
    /// <summary>
    /// Command to add a user to a board
    /// </summary>
    public class AddUserBoardCommand : IRequest<Result<UserBoardResponse>>
    {
        public AddUserBoardCommand(Guid boardId, string userId, UserBoardAccess access)
        {
            BoardId = boardId;
            UserId = userId;
            Access = access;
        }

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public UserBoardAccess Access { get; set; }
    }
}
