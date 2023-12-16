using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.RemoveCardAttachment
{
    public class RemoveCardAttachmentCommand : BaseBoardCommand, IRequest<Result<RemoveCardAttachmentResponse>>
    {
        public RemoveCardAttachmentCommand(Guid cardId, Guid attachmentId)
        {
            CardId = cardId;
            AttachmentId = attachmentId;
        }

        public Guid CardId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}
