using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.UpdateBacklog;

public class UpdateBacklogCommand : IRequest<Result<bool>>
{
    public UpdateBacklogCommand(Guid cardId, Guid boardId, string title, IssueTypeDto issueType, short? storyPoints)
    {
        CardId = cardId;
        BoardId = boardId;
        Title = title;
        IssueType = issueType;
        StoryPoints = storyPoints;
    }

    public Guid CardId { get; set; }

    public Guid BoardId { get; set; }

    [Required]
    public string Title { get; set; }
    
    [Required]
    public IssueTypeDto IssueType { get; set; }

    public short? StoryPoints { get; set; }
}
