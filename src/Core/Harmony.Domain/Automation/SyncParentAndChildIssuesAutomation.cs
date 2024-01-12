using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Automation
{
    public class SyncParentAndChildIssuesAutomation : Automation
    {
        public Guid FromStatus { get; set; }
        public Guid ToStatus { get; set; }
    }
}
