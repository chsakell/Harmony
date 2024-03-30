using Harmony.Application.Specifications.Base;
using Harmony.Domain.Entities;

namespace Harmony.Application.Specifications.Cards
{
    public class RetrospectiveFilterSpecification : HarmonySpecification<Retrospective>
    {
        public Guid? SprintId { get; set; }
        public Guid BoardId { get; set; }

        public RetrospectiveFilterSpecification(Guid boardId)
        {
            BoardId = boardId;
        }

        public void Build()
        {
            Criteria = retro => retro.BoardId == BoardId;

            if (SprintId.HasValue)
            {
                And(retro => retro.SprintId == SprintId);
            }
        }
    }
}
