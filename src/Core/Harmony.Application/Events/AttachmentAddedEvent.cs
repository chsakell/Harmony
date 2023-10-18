using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class AttachmentAddedEvent
    {
        public Guid CardId { get; set; }
        public AttachmentDto Attachment { get; set; }

        public AttachmentAddedEvent(Guid cardId, AttachmentDto attachment)
        {
            CardId = cardId;
            Attachment = attachment;
        }
    }
}
