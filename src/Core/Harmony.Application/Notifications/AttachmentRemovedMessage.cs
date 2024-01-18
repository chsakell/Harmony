namespace Harmony.Application.Notifications
{
    public class AttachmentRemovedMessage
    {
        public AttachmentRemovedMessage(Guid boardId, Guid cardId, Guid attachmentId)
        {
            BoardId = boardId;
            CardId = cardId;
            AttachmentId = attachmentId;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}
