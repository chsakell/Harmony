using Harmony.Domain.Enums.Automations;

namespace Harmony.Domain.Automation
{
    public class SmartAutoAssignAutomation : Automation
    {
        public AutomationTriggerSchedule RunTriggerAt { get; set; }
        public bool OverrideAndCopyFromParentIfSubtask { get; set; }
        public bool  AssignIfNoneAssigned { get; set; }
        public SmartAutoAssignOption  Option { get; set; }
        public string UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public override bool SingleRuleOnly { get; set; } = true;
    }
}
