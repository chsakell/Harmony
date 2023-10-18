using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class CardActivityDto
    {
        public Guid Id { get; set; }
        public string Activity { get; set; }
        public string Actor { get; set; }
        public Guid CardId { get; set; }
        public CardActivityType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public string Url { get; set; }
    }
}
