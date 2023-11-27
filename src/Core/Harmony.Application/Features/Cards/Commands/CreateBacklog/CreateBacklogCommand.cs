using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateBacklog;

public class CreateBacklogCommand : IRequest<Result<CardDto>>
{
    [Required]
    public string Title { get; set; }
    public Guid BoardId { get; set; }

    [Required]
    public IssueTypeDto IssueType { get; set; }

    public CreateBacklogCommand(string title, Guid boardId)
    {
        Title = title;
        BoardId = boardId;
    }
}
