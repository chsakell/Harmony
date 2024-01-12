using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Automation
{
    public interface IAutomationDto
    {
        AutomationType Type { get; set; }
        Guid BoardId { get; set; }
    }
}
