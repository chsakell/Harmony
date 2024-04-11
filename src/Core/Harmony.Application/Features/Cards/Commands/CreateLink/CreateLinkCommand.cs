using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.AddUserCard;
using Harmony.Application.Models;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateLink
{
    public class CreateLinkCommand : BaseBoardCommand, IRequest<Result<LinkDetailsDto>>
    {
        public CreateLinkCommand(Guid boardId, Guid sourceCardId)
        {
            BoardId = boardId;
            SourceCardId = sourceCardId;
        }

        public Guid SourceCardId { get; set; }

        [Required(ErrorMessage = "Linked issue is required")]
        public Guid? TargetCardId { get; set; }
        public LinkType Type { get; set; }
    }
}
