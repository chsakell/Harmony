using Harmony.Application.DTO;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;

namespace Harmony.Application.Models
{
    public class BoardInfo
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public List<BoardListDto> Lists { get; set; } = new List<BoardListDto>();
        public List<IssueTypeDto> IssueTypes { get; set; } = new List<IssueTypeDto>();
        public BoardType Type { get; set; }
        public string Key { get; set; }
        public string IndexName { get; set; }
        public List<SprintDto> ActiveSprints { get; set; } = new List<SprintDto> { };
    }
}
