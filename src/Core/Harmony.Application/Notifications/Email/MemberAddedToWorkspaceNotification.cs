using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.Email
{
    public class MemberAddedToWorkspaceNotification : BaseEmailNotification
    {
        public MemberAddedToWorkspaceNotification(Guid workspaceId, string userId, string workspaceUrl)
        {
            WorkspaceId = workspaceId;
            UserId = userId;
            WorkspaceUrl = workspaceUrl;
        }

        public override EmailNotificationType Type => EmailNotificationType.MemberAddedToWorkspace;

        public Guid WorkspaceId { get; set; }
        public string UserId { get; set; }
        public string WorkspaceUrl { get; set; }
    }
}
