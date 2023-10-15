using Harmony.Shared.Wrapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Features.Boards.Commands.RemoveUserBoard
{
    public class RemoveUserBoardResponse 
    {
        public RemoveUserBoardResponse(Guid boardId, string userId)
        {
            BoardId = boardId;
            UserId = userId;
        }

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
    }
}
