using Harmony.Application.DTO;

namespace Harmony.Application.Features.Boards.Queries.Get
{
    public class GetBoardResponse
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public Guid WorkspaceId { get; set; }
		public List<BoardListDto> Lists { get; set; }
	}
}
