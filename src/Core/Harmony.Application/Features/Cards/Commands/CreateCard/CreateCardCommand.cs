using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateCard;

public class CreateCardCommand : IRequest<Result<CardDto>>
{
    [Required]
    public string Title { get; set; }
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }

    [Required]
    public IssueTypeDto IssueType { get; set; }
    public Guid? SprintId  { get; set; }

    public CreateCardCommand(string title, Guid boardId, Guid listId)
    {
        Title = title;
        BoardId = boardId;
        ListId = listId;
    }
}
