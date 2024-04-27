using Harmony.Application.Notifications;
using Harmony.Application.SourceControl.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.SourceControl.Services.Hubs
{
    public interface IHubClientSourceControlNotifierService
    {
        Task BranchCreated(BranchCreatedMessage messsage);
    }
}
