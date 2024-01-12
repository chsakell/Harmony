using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Automation
{
    public class AutomationTemplateDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Summary { get; set; }
        public AutomationType Type { get; set; }
        public bool Enabled { get; set; }
    }
}
