using Harmony.Application.DTO;

namespace Harmony.Application.Events
{
    public class WorkspaceAddedEvent
    {
        public WorkspaceDto Workspace { get; set; }

        public WorkspaceAddedEvent(WorkspaceDto workspace)
        {
            Workspace = workspace;
        }
    }
}
