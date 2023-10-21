using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListTitle;

public class UpdateListTitleCommand : IRequest<Result<UpdateListTitleResponse>>
{
    public Guid BoardId { get; set; }
    public Guid ListId { get; set; }
    public string Title { get; set; }

    public UpdateListTitleCommand(Guid boardId, Guid listId, string title)
    {
        BoardId = boardId;
        ListId = listId;
        Title = title;
    }
}
