using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateSprintIssue;

public class CreateSprintIssueCommand : IRequest<Result<CardDto>>
{
    public CreateSprintIssueCommand(Guid boardId, Guid sprintId)
    {
        BoardId = boardId;
        SprintId = sprintId;
    }

    public Guid BoardId { get; set; }
    public Guid SprintId { get; set; }

    [Required]
    public string Title { get; set; }

    [Required]
    public IssueTypeDto IssueType { get; set; }
    public Guid BoardListId { get; set; }
}
