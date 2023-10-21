using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Labels.Commands.RemoveCardLabel;

public class RemoveCardLabelCommand : IRequest<IResult<bool>>
{
    public Guid LabelId { get; set; }

    public RemoveCardLabelCommand(Guid labelId)
    {
        LabelId = labelId;
    }
}
