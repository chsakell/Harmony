using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications
{
    public class MemberRemovedFromWorkspaceNotification : BaseNotification
    {
        public MemberRemovedFromWorkspaceNotification(Guid workspaceId, string userId, string workspaceUrl)
        {
            WorkspaceId = workspaceId;
            UserId = userId;
            WorkspaceUrl = workspaceUrl;
        }

        public override NotificationType Type => NotificationType.MemberRemovedFromWorkspace;

        public Guid WorkspaceId { get; set; }
        public string UserId { get; set; }
        public string WorkspaceUrl { get; set; }
    }
}
