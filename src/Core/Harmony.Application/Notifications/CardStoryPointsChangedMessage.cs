namespace Harmony.Application.Notifications
{
    public class CardStoryPointsChangedMessage
    {
        public CardStoryPointsChangedMessage(
            Guid boardId, Guid cardId,
            short? storyPoints, Guid? parentCardId)
        {
            BoardId = boardId;
            CardId = cardId;
            StoryPoints = storyPoints;
            ParentCardId = parentCardId;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public Guid? ParentCardId { get; set; }
        public short? StoryPoints { get; set; }
    }
}
