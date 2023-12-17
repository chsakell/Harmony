using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Cards.Commands.UpdateCardIssueType;

public class UpdateCardIssueTypeCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public UpdateCardIssueTypeCommand(Guid cardId, Guid issueTypeId)
    {
        CardId = cardId;
        IssueTypeId = issueTypeId;
    }

    public Guid CardId { get; set; }
    public Guid IssueTypeId { get; set; }
}