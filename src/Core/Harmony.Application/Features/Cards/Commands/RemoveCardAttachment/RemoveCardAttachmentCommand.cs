using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.RemoveCardAttachment
{
    public class RemoveCardAttachmentCommand : IRequest<Result<RemoveCardAttachmentResponse>>
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
