using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class LinkDto
    {
        public Guid Id { get; set; }
        public Guid SourceCardId { get; set; }
        public string SourceCardTitle { get; set; }
        public string SourceCardSerialKey { get; set; }
        public Guid TargetCardId { get; set; }
        public string TargetCardTitle { get; set; }
        public string TargetCardSerialKey { get; set; }
        public string UserId { get; set; }
        public LinkType Type { get; set; }
    }
}
