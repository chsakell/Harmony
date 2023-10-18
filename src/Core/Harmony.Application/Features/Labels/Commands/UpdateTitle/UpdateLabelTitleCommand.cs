using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Labels.Commands.UpdateTitle;

public class UpdateLabelTitleCommand : IRequest<IResult>
{
    public Guid LabelId { get; set; }
    public string Title { get; set; }

    public UpdateLabelTitleCommand(Guid labelId, string title)
    {
        LabelId = labelId;
        Title = title;
    }
}
