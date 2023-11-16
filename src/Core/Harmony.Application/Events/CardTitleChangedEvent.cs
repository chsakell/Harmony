namespace Harmony.Application.Events
{
    public class CardTitleChangedEvent
    {
        public int CardId { get; set; }
        public string Title { get; set; }

        public CardTitleChangedEvent(int cardId, string title)
        {
            CardId = cardId;
            Title = title;
        }
    }
}
