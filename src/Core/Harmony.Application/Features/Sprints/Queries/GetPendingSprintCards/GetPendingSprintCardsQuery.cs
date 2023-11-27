using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Workspaces.Queries.GetSprints
{
    public class GetPendingSprintCardsQuery : PagedRequest, IRequest<IResult<GetPendingSprintCardsResponse>>
    {
        public GetPendingSprintCardsQuery(Guid boardId, Guid sprintId)
        {
            BoardId = boardId;
            SprintId = sprintId;
        }

        public Guid BoardId { get; set; }
        public Guid SprintId { get; set; }
    }
}