using Harmony.Domain.Enums.Automations;

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
