using Harmony.Domain.Enums;

namespace Harmony.Application.Events
{
    public class BoardCreatedEvent
    {
        public BoardCreatedEvent(string workspaceId, Guid boardId, string title, string description, BoardVisibility visibility)
        {
            WorkspaceId = workspaceId;
            BoardId = boardId;
            Title = title;
            Description = description;
            Visibility = visibility;
        }

        public string WorkspaceId { get; set; }
        public Guid BoardId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public BoardVisibility Visibility { get; set; }
    }
}
