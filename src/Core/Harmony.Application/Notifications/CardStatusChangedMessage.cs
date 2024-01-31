using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications
{
    public class CardStatusChangedMessage
    {
        public CardStatusChangedMessage(Guid boardId, Guid cardId, CardStatus status)
        {
            BoardId = boardId;
            CardId = cardId;
            Status = status;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public CardStatus Status { get; set; }
    }
}
