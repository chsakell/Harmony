using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateCard;

public class CreateCardCommand : IRequest<Result<CardDto>>
{
    [Required]
    public string Name { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }

    public CreateCardCommand(string name, Guid boardId, Guid listId)
    {
        Name = name;
        BoardId = boardId;
        ListId = listId;
    }
}
