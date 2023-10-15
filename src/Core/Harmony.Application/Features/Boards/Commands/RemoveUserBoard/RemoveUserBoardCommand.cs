using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Boards.Commands.RemoveUserBoard
{
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
