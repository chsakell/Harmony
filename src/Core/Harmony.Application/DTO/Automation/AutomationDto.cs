using Harmony.Domain.Enums.Automations;
using System.ComponentModel.DataAnnotations;

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
        public virtual bool SingleRuleOnly { get; set; }
    }
}
