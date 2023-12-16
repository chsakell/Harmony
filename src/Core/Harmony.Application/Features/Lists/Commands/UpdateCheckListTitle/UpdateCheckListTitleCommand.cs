using Harmony.Application.Models;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateCheckListTitle;

public class UpdateCheckListTitleCommand : BaseBoardCommand, IRequest<Result<bool>>
{
    public Guid ListId { get; set; }
    public string Title { get; set; }

    public UpdateCheckListTitleCommand(Guid listId, string title)
    {
        ListId = listId;
        Title = title;
    }
}
