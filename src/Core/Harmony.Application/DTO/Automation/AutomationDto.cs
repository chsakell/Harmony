using Harmony.Application.Contracts.Persistence;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Application.DTO.Automation
{
    public class AutomationDto : IAutomationDto
    {
        public string Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [MaxLength(400)]
        public string Description { get; set; }
        public AutomationType Type { get; set; }
        public string BoardId { get; set; }
        public bool Enabled { get; set; }
    }
}
