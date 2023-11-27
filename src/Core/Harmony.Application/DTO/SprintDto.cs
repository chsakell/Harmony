using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class SprintDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }
        public Guid BoardId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SprintStatus Status { get; set; }
    }
}
