using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class AttachmentAddedEvent
    {
        public int CardId { get; set; }
        public AttachmentDto Attachment { get; set; }

        public AttachmentAddedEvent(int cardId, AttachmentDto attachment)
        {
            CardId = cardId;
            Attachment = attachment;
        }
    }
}
