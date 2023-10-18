namespace Harmony.Application.Events
{
    public class CardItemAddedEvent
    {
        public Guid CardId { get; set; }

        public CardItemAddedEvent(Guid cardId)
        {
            CardId = cardId;
        }
    }
}
