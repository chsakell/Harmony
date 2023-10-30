using Harmony.Application.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
