using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.DeleteLink
{
    public class DeleteLinkCommand : BaseBoardCommand, IRequest<Result<List<Guid>>>
    {
        public Guid LinkId { get; set; }

        public DeleteLinkCommand(Guid boardId, Guid linkId)
        {
            BoardId = boardId;
            LinkId = linkId;
        }
    }
}
