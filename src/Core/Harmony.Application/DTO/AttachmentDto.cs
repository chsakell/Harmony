using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class AttachmentDto
    {
        public Guid Id { get; set; }
        public string FileName { get; set; }
        public string OriginalFileName { get; set; }
        public AttachmentType Type { get; set; }
        public string Url { get; set; }
        public DateTime DateCreated { get; set; }
    }
}
