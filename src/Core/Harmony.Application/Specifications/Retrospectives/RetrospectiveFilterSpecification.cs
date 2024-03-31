using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;

namespace Harmony.Application.Specifications.Cards
{
    public class RetrospectiveFilterSpecification : HarmonySpecification<Retrospective>
    {
        public Guid? SprintId { get; set; }
        public Guid? ParentBoardId { get; set; }

        public void Build()
        {
            if(ParentBoardId.HasValue)
            {
                And(retro => retro.ParentBoardId == ParentBoardId);
            }

            if (SprintId.HasValue)
            {
                And(retro => retro.SprintId == SprintId);
            }
        }
    }
}
