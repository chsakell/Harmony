
using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Cards.Commands.UploadFile
{
    public class UploadFileResponse
    {
        public UploadFileResponse(Guid cardId, AttachmentDto attachment)
        {
            CardId = cardId;
            Attachment = attachment;
        }

        public Guid CardId { get; set; }
        public AttachmentDto Attachment { get; set; }
    }
}
