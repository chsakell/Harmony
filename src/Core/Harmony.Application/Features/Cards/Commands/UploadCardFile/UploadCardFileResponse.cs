using Harmony.Application.DTO;

namespace Harmony.Application.Features.Cards.Commands.UploadCardFile
{
    public class UploadCardFileResponse
    {
        public UploadCardFileResponse(int cardId, AttachmentDto attachment)
        {
            CardId = cardId;
            Attachment = attachment;
        }

        public int CardId { get; set; }
        public AttachmentDto Attachment { get; set; }
    }
}
