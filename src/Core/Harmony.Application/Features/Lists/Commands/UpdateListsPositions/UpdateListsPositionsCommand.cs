using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Lists.Commands.UpdateListsPositions
{
    public class UpdateListsPositionsCommand : IRequest<Result<UpdateListsPositionsResponse>>
    {
        public Guid BoardId { get; set; }
        public Dictionary<Guid, short> ListPositions { get; set; }

        public UpdateListsPositionsCommand(Guid boardId, Dictionary<Guid, short> listPositions)
        {
            BoardId = boardId;
            ListPositions = listPositions;
        }
    }
}
