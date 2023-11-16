namespace Harmony.Application.Events
{
    public class CardItemAddedEvent
    {
        public int CardId { get; set; }

        public CardItemAddedEvent(int cardId)
        {
            CardId = cardId;
        }
    }
}
