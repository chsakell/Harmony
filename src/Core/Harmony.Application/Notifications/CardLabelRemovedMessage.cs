namespace Harmony.Application.Notifications
{
    public class CardLabelRemovedMessage
    {
        public CardLabelRemovedMessage(Guid boardId, Guid cardLabelId)
        {
            BoardId = boardId;
            CardLabelId = cardLabelId;
        }

        public Guid BoardId { get; set; }
        public Guid CardLabelId { get; set; }
    }
}
