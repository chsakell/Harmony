using Harmony.Application.DTO;

namespace Harmony.Application.Notifications
{
    public class AttachmentAddedMessage
    {
        public AttachmentAddedMessage(Guid boardId, Guid cardId, AttachmentDto attachment)
        {
            BoardId = boardId;
            CardId = cardId;
            Attachment = attachment;
        }

        public Guid BoardId { get; set; }
        public Guid CardId { get; set; }
        public AttachmentDto Attachment { get; set; }
    }
}
