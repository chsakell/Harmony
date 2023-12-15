using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Requests;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Sprints.Queries.GetSprintReports
{
    public class GetSprintReportsQuery : IRequest<IResult<GetSprintReportsResponse>>
    {
        public GetSprintReportsQuery(Guid sprintId)
        {
            SprintId = sprintId;
        }
        public Guid SprintId { get; set; }
    }
}