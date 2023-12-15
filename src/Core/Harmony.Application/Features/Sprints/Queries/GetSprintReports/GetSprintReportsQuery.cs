using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Sprints.Queries.GetSprintReports
{
    public class GetSprintReportsQuery : IRequest<IResult<GetSprintReportsResponse>>
    {
        public GetSprintReportsQuery(Guid boardId, Guid sprintId)
        {
            BoardId = boardId;
            SprintId = sprintId;
        }

        public Guid BoardId { get; set; }
        public Guid SprintId { get; set; }
    }
}