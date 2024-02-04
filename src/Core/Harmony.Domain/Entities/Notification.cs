using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }
        public Guid? CardId { get; set; }
        public string UserId { get; set; }
        public Guid? BoardId { get; set; }
        public Guid? WorkspaceId { get; set; }
        public EmailNotificationType Type { get; set; }
        public string JobId { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
