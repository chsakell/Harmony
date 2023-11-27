using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class BoardCreatedEvent
    {
        public BoardCreatedEvent(string workspaceId, BoardDto board)
        {
            WorkspaceId = workspaceId;
            Board = board;
        }

        public string WorkspaceId { get; set; }
        public BoardDto Board { get; set; }
    }
}
