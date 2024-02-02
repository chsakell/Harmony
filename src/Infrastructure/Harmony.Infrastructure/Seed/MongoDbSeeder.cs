using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;
using Harmony.Domain.Enums.Automations;

namespace Harmony.Infrastructure.Seed
{
    public class MongoDbSeeder : IDatabaseSeeder
    {
        private readonly IAutomationRepository _automationRepository;

        public MongoDbSeeder(IAutomationRepository automationRepository)
        {
            _automationRepository = automationRepository;
        }

        public int Order => 5;

        public async Task Initialize()
        {
            var templates = await _automationRepository.GetTemplates();

            if (!templates.Any())
            {
                templates.AddRange(new List<AutomationTemplate>()
                {
                    new AutomationTemplate()
                    {
                        Name = "Sync parent and child tasks",
                        Summary = "If the sub-task moves to 'In Progress' then move the parent issue to 'In Progress' also. Always keep your parent and child tasks in sync.",
                        Type = AutomationType.SyncParentAndChildIssues,
                        Enabled = true,
                        NotificationTypes = new List<NotificationType>() { NotificationType.CardMoved }
                    },
                    new AutomationTemplate()
                    {
                        Name = "Smart auto-assign",
                        Summary = "When an issue is created without an assignee, automatically assign it. Go one better and assign it based on skillset or in a balanced workload.",
                        Type = AutomationType.SmartAutoAssign,
                        Enabled = true,
                        NotificationTypes = new List<NotificationType>() { NotificationType.CardCreated }
                    },
                    new AutomationTemplate()
                    {
                        Name = "Auto-create sub-tasks",
                        Summary = "Whenever a Jira issue is created, automatically create 5 sub-tasks with certain fields populated. Start simple and add depth as you go.",
                        Type = AutomationType.AutoCreateSubtasks,
                        NotificationTypes = new List<NotificationType>() { NotificationType.CardCreated }
                    },
                    new AutomationTemplate()
                    {
                        Name = "Sum up story points",
                        Summary = "When a new sub-task is created, sum up its story points to the parent",
                        Type = AutomationType.SumUpStorePoints,
                        NotificationTypes = new List<NotificationType>() { NotificationType.SubTaskStoryPoints }
                    }
                });


                await _automationRepository.CreateTemplates(templates);
            }
        }
    }
}
