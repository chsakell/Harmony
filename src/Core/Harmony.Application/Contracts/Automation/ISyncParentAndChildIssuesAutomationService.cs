using Harmony.Application.DTO.Automation;
using Harmony.Application.Notifications;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.Contracts.Automation
{
    public interface ISyncParentAndChildIssuesAutomationService
    {
        Task Process(SyncParentAndChildIssuesAutomationDto automation, CardMovedNotification notification);
    }
}
