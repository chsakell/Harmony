using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListTitle;

public class UpdateListTitleCommand : IRequest<Result<bool>>
{
    public Guid ListId { get; set; }
    public string Title { get; set; }

    public UpdateListTitleCommand(Guid listId, string title)
    {
        ListId = listId;
        Title = title;
    }
}
