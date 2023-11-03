using Harmony.Application.DTO;

namespace Harmony.Application.Features.Cards.Commands.UploadCardFile
{
    public class UploadCardFileResponse
    {
        public UploadCardFileResponse(Guid cardId, AttachmentDto attachment)
        {
            CardId = cardId;
            Attachment = attachment;
        }

        public Guid CardId { get; set; }
        public AttachmentDto Attachment { get; set; }
    }
}
