using Harmony.Domain.Enums;

namespace Harmony.Application.DTO
{
    public class BoardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public Guid WorkspaceId { get; set; }
        public BoardVisibility Visibility { get; set; }
        public BoardType Type { get; set; }
        public string Key { get; set; }
        public List<BoardListDto> Lists { get; set; }
    }
}
