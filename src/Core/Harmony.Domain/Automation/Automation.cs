using Harmony.Domain.Enums.Automations;

namespace Harmony.Domain.Automation
{
    public interface IAutomation 
    {
        AutomationType Type { get; set; }
        Guid BoardId { get; set; }
    }

    public abstract class Automation : IAutomation
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AutomationType Type { get; set; }
        public Guid BoardId { get; set; }
        public bool Enabled { get; set; }
        public virtual bool SingleRuleOnly { get; set; }
    }
}
