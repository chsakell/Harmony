using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Cards.Commands.CreateChecklist;

public class CreateChecklistCommand : IRequest<Result<CheckListDto>>
{
    public Guid CardId { get; set; }
    public string Title { get; set; }
    public byte Position { get; set; }

    public CreateChecklistCommand(Guid cardId, string title, byte position)
    {
        CardId = cardId;
        Title = title;
        Position = position;
    }
}
