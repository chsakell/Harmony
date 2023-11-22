using Harmony.Application.DTO;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.GetSprints
{
    public class GetSprintCardResponse
    {
        public Guid? CardId { get; set; }
        public string CardTitle { get; set; }
        public DateTime? CardStartDate { get; set; }
        public DateTime? CardDueDate { get; set; }
        public string CardSerialKey { get; set; }
        public IssueTypeDto? CardIssueType { get; set; }
        public Guid SprintId { get; set; }
        public string Sprint { get; set; }
        public SprintStatus SprintStatus { get; set; }
        public DateTime SprintStartDate { get; set; }
        public DateTime SprintEndDate { get; set; }
        public bool IsCompleted { get; set; }
    }
}
