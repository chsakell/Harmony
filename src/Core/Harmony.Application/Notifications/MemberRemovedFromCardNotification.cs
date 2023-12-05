using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications
{
    public class MemberRemovedFromCardNotification : BaseNotification
    {
        public MemberRemovedFromCardNotification(Guid boardId, Guid cardId, string userId, string cardUrl)
        {
            BoardId = boardId;
            CardId = cardId;
            UserId = userId;
            CardUrl = cardUrl;
        }

        public override NotificationType Type => NotificationType.MemberRemovedFromCard;

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public string UserId { get; set; }
        public string CardUrl { get; set; }
    }
}
