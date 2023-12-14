namespace Harmony.Application.Events
{
    public class CardStoryPointsChangedEvent
    {
        public CardStoryPointsChangedEvent(Guid cardId, short? storyPoints)
        {
            CardId = cardId;
            StoryPoints = storyPoints;
        }

        public Guid CardId { get; set; }
        public short? StoryPoints { get; set; }
    }
}
