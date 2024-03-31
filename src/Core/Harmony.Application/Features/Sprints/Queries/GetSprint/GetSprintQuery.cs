using Harmony.Application.DTO;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Sprints.Queries.GetSprint
{
    public class GetSprintQuery : IRequest<IResult<SprintDto>>
    {
        public GetSprintQuery(Guid sprintId)
        {
            SprintId = sprintId;
        }
        public Guid SprintId { get; set; }
    }
}