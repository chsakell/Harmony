using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetSprintItemResponse
    {
        public Guid? CardId { get; set; }
        public string CardTitle { get; set; }
        public DateTime? CardStartDate { get; set; }
        public DateTime? CardDueDate { get; set; }
        public string CardSerialKey { get; set; }
        public IssueTypeDto? CardIssueType { get; set; }
        public string Sprint { get; set; }
        public Guid SprintId { get; set; }
        public DateTime SprintStartDate { get; set; }
        public DateTime SprintEndDate { get; set; }
    }
}
