using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications
{
    public class MemberRemovedFromBoardNotification : BaseNotification
    {
        public MemberRemovedFromBoardNotification(Guid boardId, string userId, string boardUrl)
        {
            BoardId = boardId;
            UserId = userId;
            BoardUrl = boardUrl;
        }

        public override NotificationType Type => NotificationType.MemberRemovedFromBoard;

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string BoardUrl { get; set; }
    }
}
