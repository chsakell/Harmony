namespace Harmony.Application.Events
{
    public class AttachmentRemovedEvent
    {
        public Guid CardId { get; set; }
        public Guid AttachmentId { get; set; }

        public AttachmentRemovedEvent(Guid cardId, Guid attachmentId)
        {
            CardId = cardId;
            AttachmentId = attachmentId;
        }
    }
}
