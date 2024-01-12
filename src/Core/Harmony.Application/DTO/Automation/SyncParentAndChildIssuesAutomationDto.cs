using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Automation
{
    public class SyncParentAndChildIssuesAutomationDto : AutomationDto
    {
        public Guid FromStatus { get; set; }
        public Guid ToStatus { get; set; }
    }
}
