using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class AttachmentRemovedEvent
    {
        public int CardId { get; set; }
        public Guid AttachmentId { get; set; }

        public AttachmentRemovedEvent(int cardId, Guid attachmentId)
        {
            CardId = cardId;
            AttachmentId = attachmentId;
        }
    }
}
