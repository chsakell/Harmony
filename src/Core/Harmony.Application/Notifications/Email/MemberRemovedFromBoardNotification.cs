using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public class MemberRemovedFromBoardNotification : BaseEmailNotification
    {
        public MemberRemovedFromBoardNotification(Guid boardId, string userId, string boardUrl)
        {
            BoardId = boardId;
            UserId = userId;
            BoardUrl = boardUrl;
        }

        public override EmailNotificationType Type => EmailNotificationType.MemberRemovedFromBoard;

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string BoardUrl { get; set; }
    }
}
