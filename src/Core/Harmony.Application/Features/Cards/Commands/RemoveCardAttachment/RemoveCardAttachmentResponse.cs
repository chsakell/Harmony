using Harmony.Application.DTO;

namespace Harmony.Application.Features.Cards.Commands.RemoveCardAttachment
{
    public class RemoveCardAttachmentResponse
    {
        public RemoveCardAttachmentResponse(int cardId, Guid attachmentId)
        {
            CardId = cardId;
            AttachmentId = attachmentId;
        }

        public int CardId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}
