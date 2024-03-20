using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.GetSprintsDetails
{
    public class SprintDetails
    {
        public Guid Id { get; set; }
        public Guid BoardId { get; set; }
        public string Name { get; set; }
        public string Goal { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public SprintStatus Status { get; set; }
        public int TotalCards { get; set; }
        public int StoryPoints { get; set; }
    }
}
