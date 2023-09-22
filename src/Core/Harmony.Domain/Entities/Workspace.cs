namespace Harmony.Domain.Entities
{
    public class Workspace : AuditableEntity<Guid>
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<Board> Boards { get; set; }
        public string UserId { get; set; }
        public List<UserWorkspace> Users { get; set; }
        public bool IsPublic { get; set; }
    }
}
