namespace Harmony.Application.DTO.Automation
{
    public class SumUpStoryPointsAutomationDto : AutomationDto
    {
        public IEnumerable<string> IssueTypes { get; set; }
        public override bool SingleRuleOnly { get; set; } = true;
    }
}
