using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    public class Retrospective : AuditableEntity<Guid>
    {
        public string Name { get; set; }
        public string UserId { get; set; }
        public Guid BoardId { get; set; }
        public Board Board { get; set; }
        public Guid? SprintId { get; set; }
        public Sprint Sprint { get; set; }
        public RetrospectiveType Type { get; set; }
        public bool HideCardsInitially { get; set; }
        public bool DisableVotingInitially { get; set; }
        public bool ShowCardsAuthor { get; set; }
    }
}
