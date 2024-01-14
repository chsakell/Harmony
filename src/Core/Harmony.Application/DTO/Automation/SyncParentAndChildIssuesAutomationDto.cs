namespace Harmony.Application.DTO.Automation
{
    public class SyncParentAndChildIssuesAutomationDto : AutomationDto
    {
        public IEnumerable<string> FromStatuses { get; set; }
        public IEnumerable<string> ToStatuses { get; set; }
    }
}
