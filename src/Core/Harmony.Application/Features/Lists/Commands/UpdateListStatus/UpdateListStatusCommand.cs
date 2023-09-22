using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.ArchiveList;

public class UpdateListStatusCommand : IRequest<Result<bool>>
{
    public Guid ListId { get; set; }
    public BoardListStatus Status { get; set; }

    public UpdateListStatusCommand(Guid listId, BoardListStatus status)
    {
        ListId = listId;
        Status = status;
    }
}
