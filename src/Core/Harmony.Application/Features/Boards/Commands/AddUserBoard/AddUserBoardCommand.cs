using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Boards.Commands.AddUserBoard
{
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
