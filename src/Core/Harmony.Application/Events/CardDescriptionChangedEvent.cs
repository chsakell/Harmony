namespace Harmony.Application.Events
{
    public class CardDescriptionChangedEvent
    {
        public int CardId { get; set; }
        public string Description { get; set; }

        public CardDescriptionChangedEvent(int cardId, string description)
        {
            CardId = cardId;
            Description = description;
        }
    }
}
