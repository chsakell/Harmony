using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Boards.Commands.UpdateUserBoardAccess
{
    /// <summary>
    /// Command to assign access level to a board member
    /// </summary>
    public class UpdateUserBoardAccessCommand : IRequest<Result<UpdateUserBoardAccessResponse>>
    {
        public UpdateUserBoardAccessCommand(Guid boardId, string userId, UserBoardAccess access)
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
