namespace Harmony.Domain.Automation
{
    public class SyncParentAndChildIssuesAutomation : Automation
    {
        public Guid FromStatus { get; set; }
        public Guid ToStatus { get; set; }
    }
}
