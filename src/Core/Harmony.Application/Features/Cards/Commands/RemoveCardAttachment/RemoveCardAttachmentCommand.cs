using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.RemoveCardAttachment
{
    public class RemoveCardAttachmentCommand : IRequest<Result<RemoveCardAttachmentResponse>>
    {
        public RemoveCardAttachmentCommand(int cardId, Guid attachmentId)
        {
            CardId = cardId;
            AttachmentId = attachmentId;
        }

        public int CardId { get; set; }
        public Guid AttachmentId { get; set; }
    }
}
