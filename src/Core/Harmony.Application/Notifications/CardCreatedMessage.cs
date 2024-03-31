using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class CardCreatedMessage
    {
        public CardCreatedMessage(Guid boardId, CardDto card, string userId)
        {
            BoardId = boardId;
            Card = card;
            UserId = userId;
        }

        public Guid BoardId { get; set; }
        public CardDto Card { get; set; }
        public string UserId { get; set; }
    }
}
