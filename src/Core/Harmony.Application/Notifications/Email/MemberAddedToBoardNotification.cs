using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public class MemberAddedToBoardNotification : BaseEmailNotification
    {
        public MemberAddedToBoardNotification(Guid boardId, string userId, string boardUrl)
        {
            BoardId = boardId;
            UserId = userId;
            BoardUrl = boardUrl;
        }

        public override EmailNotificationType Type => EmailNotificationType.MemberAddedToBoard;

        public Guid BoardId { get; set; }
        public string UserId { get; set; }
        public string BoardUrl { get; set; }
    }
}
