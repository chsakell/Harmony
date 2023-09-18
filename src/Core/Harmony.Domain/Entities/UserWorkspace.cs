namespace Harmony.Domain.Entities
{
    /// <summary>
    /// Class to represent M 2 M relationship between users and workspaces
    /// (intermediate table)
    /// </summary>
    public class UserWorkspace
    {
        public string UserId { get; set; }
        public Workspace Workspace { get; set; }
        public Guid WorkspaceId { get; set; }
    }
}
