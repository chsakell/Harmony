namespace Harmony.Application.Notifications
{
    public class CardItemAddedMessage
    {
        public CardItemAddedMessage(Guid boardId, Guid cardId)
        {
            BoardId = boardId;
            CardId = cardId;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
    }
}
