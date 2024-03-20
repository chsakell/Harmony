using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Specifications.Sprints
{
    public class SprintFilterSpecification : HarmonySpecification<Sprint>
    {
        public SprintStatus? Status { get; set; }
        public SprintFilterSpecification(Guid? sprintId = null, Guid? boardId = null)
        {
            if (sprintId.HasValue)
            {
                Criteria = And(sprint => sprint.Id == sprintId);
            }

            if (boardId.HasValue)
            {
                Criteria = And(sprint => sprint.BoardId == boardId);
            }

            if(Status.HasValue)
            {
                Criteria = And(sprint => sprint.Status == sprint.Status);
            }
        }
    }
}
