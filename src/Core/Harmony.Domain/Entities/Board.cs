using Harmony.Domain.Contracts;
using Harmony.Domain.Enums;
using Harmony.Domain.Extensions;

namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Represents a user's board
    /// </summary>
    public class Board : AuditableEntity<Guid>, IHashable
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
        public Retrospective Retrospective { get; set; }
        public Guid? RetrospectiveId { get; set; }
        public List<Retrospective> Retrospectives { get; set; }

        public Dictionary<string, string> ConvertToDictionary()
        {
            return this.ToDictionary();
        }
    }
}
