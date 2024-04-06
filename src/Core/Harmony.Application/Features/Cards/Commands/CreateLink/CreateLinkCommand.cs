using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Application.Models;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.CreateLink
{
    public class CreateLinkCommand : BaseBoardCommand, IRequest<Result<LinkDto>>
    {
        public CreateLinkCommand(Guid sourceCardId, Guid targetCardId, LinkType type)
        {
            SourceCardId = sourceCardId;
            TargetCardId = targetCardId;
            Type = type;
        }

        public Guid SourceCardId { get; set; }
        public Guid TargetCardId { get; set; }
        public LinkType Type { get; set; }
    }
}
