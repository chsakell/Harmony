using Harmony.Domain.Enums.Automations;

namespace Harmony.Application.DTO.Automation
{
    public class SmartAutoAssignAutomationDto : AutomationDto
    {
        public AutomationTriggerSchedule RunTriggerAt { get; set; }
        public bool OverrideAndCopyFromParentIfSubtask { get; set; }
        public SmartAutoAssignOption Option { get; set; }
        public string UserId { get; set; }
    }
}
