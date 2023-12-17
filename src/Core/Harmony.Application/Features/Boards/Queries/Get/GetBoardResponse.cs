using Harmony.Application.DTO;
using Harmony.Domain.Enums;

namespace Harmony.Application.Features.Boards.Queries.Get
{
    /// <summary>
    /// Response for getting a board
    /// </summary>
    public class GetBoardResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Key { get; set; }
        public string UserId { get; set; }
        public Guid WorkspaceId { get; set; }
		public List<BoardListDto> Lists { get; set; }
        public BoardType Type { get; set; }
        public List<SprintDto> ActiveSprints { get; set; } = new List<SprintDto>();
        public List<IssueTypeDto> IssueTypes { get; set; }
    }
}
