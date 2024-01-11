using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Automation;
using Harmony.Domain.Enums;

namespace Harmony.Infrastructure.Seed
{
    public class MongoDbSeeder : IDatabaseSeeder
    {
        private readonly IAutomationRepository _automationRepository;

        public MongoDbSeeder(IAutomationRepository automationRepository) {
            _automationRepository = automationRepository;
        }

        public int Order => 5;

        public async Task Initialize()
        {
            var templates = await _automationRepository.GetTemplates();

            if(!templates.Any())
            {
                var automationTemplate = new AutomationTemplate()
                {
                    Title = "Auto-close parent issue when subtasks are done",
                    Type = AutomationType.AutoCloseParentIsssueWhenSubTasksAreDone,
                    Description = @"Keep sub-tasks and their parent in sync. When the last sub-task moves to 'done', usually you will want the parent / epic to move to 'done' also. This automation rule solves that problem.
                                    <br><br>It is a common rule that most customers use to ensure the parent and child issue always stay in sync. "
                };

                await _automationRepository.CreateTemplate(automationTemplate);
            }
        }
    }
}
