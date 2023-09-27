using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist;

public class CreateCheckListCommand : IRequest<Result<CheckListDto>>
{
    public Guid CardId { get; set; }

    [Required]
    public string Title { get; set; }

    public CreateCheckListCommand()
    {
        
    }

    public CreateCheckListCommand(Guid cardId, string title)
    {
        CardId = cardId;
        Title = title;
    }
}
