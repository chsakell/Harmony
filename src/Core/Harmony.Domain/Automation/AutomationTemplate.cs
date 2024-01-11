using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Domain.Automation
{
    public class AutomationTemplate
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public AutomationType Type { get; set; }
    }
}
