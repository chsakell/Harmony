
using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    public class Attachment : AuditableEntity<Guid>
    {
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public Card Card { get; set; }
        public Guid CardId { get; set; }
        public AttachmentType Type { get; set; }
        public string UserId { get; set; }
    }
}
