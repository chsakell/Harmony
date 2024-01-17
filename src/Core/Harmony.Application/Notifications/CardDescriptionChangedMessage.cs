namespace Harmony.Application.Notifications
{
    public class CardDescriptionChangedMessage
    {
        public CardDescriptionChangedMessage(Guid boardId, Guid cardId, string description)
        {
            BoardId = boardId;
            CardId = cardId;
            Description = description;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public string Description { get; set; }
    }
}
