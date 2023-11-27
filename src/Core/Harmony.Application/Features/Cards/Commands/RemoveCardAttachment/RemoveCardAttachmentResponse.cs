namespace Harmony.Application.Features.Cards.Commands.RemoveCardAttachment
{
    public class RemoveCardAttachmentResponse
    {
        public RemoveCardAttachmentResponse(Guid cardId, Guid attachmentId)
        {
            CardId = cardId;
            AttachmentId = attachmentId;
        }

        public Guid CardId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}
