namespace Harmony.Application.Notifications
{
    public class CardStoryPointsChangedMessage
    {
        public CardStoryPointsChangedMessage(Guid boardId, Guid cardId, short? storyPoints)
        {
            BoardId = boardId;
            CardId = cardId;
            StoryPoints = storyPoints;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public short? StoryPoints { get; set; }
    }
}
