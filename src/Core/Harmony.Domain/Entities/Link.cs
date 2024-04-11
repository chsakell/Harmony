
using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    public class Link : AuditableEntity<Guid>
    {
        public Guid SourceCardId { get; set; }
        public Card SourceCard { get; set; }
        public Guid TargetCardId { get; set; }
        public string UserId { get; set; }
        public LinkType Type { get; set; }
    }
}
