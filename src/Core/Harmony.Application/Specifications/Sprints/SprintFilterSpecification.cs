using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Specifications.Sprints
{
    public class SprintFilterSpecification : HarmonySpecification<Sprint>
    {
        public SprintStatus? Status { get; set; }
        public Guid? SprintId { get; set; }
        public Guid? BoardId { get; set; }

        public bool IncludeRetrospective { get; set; }

        public void Build()
        {
            if(IncludeRetrospective)
            {
                Includes.Add(sprint => sprint.Retrospective);
            }

            if (SprintId.HasValue)
            {
                Criteria = And(sprint => sprint.Id == SprintId);
            }

            if (BoardId.HasValue)
            {
                Criteria = And(sprint => sprint.BoardId == BoardId);
            }

            if (Status.HasValue)
            {
                Criteria = And(sprint => sprint.Status == sprint.Status);
            }
        }
    }
}
