using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.GetArchivedItems
{
    public class GetArchivedItemResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public string BoardKey { get; set; }
        public string SerialKey { get; set; }
        public IssueTypeDto IssueType { get; set; }
        public short Position { get; set; }
        public short? StoryPoints { get; set; }
        public BoardType BoardType { get; set; }
    }
}
