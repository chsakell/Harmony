using Harmony.Application.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Notifications
{
    public class MemberAddedToBoardNotification : BaseNotification
    {
        public MemberAddedToBoardNotification(Guid boardId, string userId, string boardUrl)
        {
            BoardId = boardId;
            UserId = userId;
            BoardUrl = boardUrl;
        }

        public override NotificationType Type => NotificationType.MemberAddedToBoard;

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string BoardUrl { get; set; }
    }
}
