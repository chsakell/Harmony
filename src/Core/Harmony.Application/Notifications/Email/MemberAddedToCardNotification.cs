using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public class MemberAddedToCardNotification : BaseEmailNotification
    {
        public MemberAddedToCardNotification(Guid boardId, Guid cardId, string userId, string cardUrl)
        {
            BoardId = boardId;
            CardId = cardId;
            UserId = userId;
            CardUrl = cardUrl;
        }

        public override EmailNotificationType Type => EmailNotificationType.MemberAddedToCard;

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public string UserId { get; set; }
        public string CardUrl { get; set; }
    }
}
