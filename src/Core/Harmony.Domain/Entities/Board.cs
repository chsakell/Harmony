using Harmony.Domain.Enums;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Represents a user's board
    /// </summary>
    public class Board : AuditableEntity<Guid>
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string UserId { get; set; }
        public Workspace Workspace { get; set; }
        public Guid WorkspaceId { get; set; }
        public BoardVisibility Visibility { get; set; }
        public List<BoardList> Lists { get; set; }
        public List<UserBoard> Users { get; set; }
        public List<Label> Labels { get; set; }
        public List<IssueType> IssueTypes { get; set; }
        public BoardType Type { get; set; }
        public string Key {  get; set; }
        public List<Sprint> Sprints { get; set; }
    }
}
