using Harmony.Domain.Entities;

namespace Harmony.Application.Models
{
    public class BoardInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<BoardList> Lists { get; set; }
        public string Key { get; set; }
        public string IndexName { get; set; }
    }
}
