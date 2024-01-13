using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;

namespace Harmony.Client.Shared.Components.Automation
{
    public partial class SyncParentAndChildIssues
    {
        [Parameter]
        public AutomationType AutomationType { get; set; }

        [Parameter] public Guid BoardId { get; set; }

        private bool _loaded = false;

        private SyncParentAndChildIssuesAutomationDto _automationModel;

        private List<SyncParentAndChildIssuesAutomationDto>? _automations = new List<SyncParentAndChildIssuesAutomationDto>();

        protected override async Task OnInitializedAsync()
        {
            _automationModel =
            new SyncParentAndChildIssuesAutomationDto()
            {
                Type = AutomationType,
                BoardId = BoardId.ToString(),
            };

            var automationsResult = await _automationManager
                .GetAutomations<SyncParentAndChildIssuesAutomationDto>(BoardId, AutomationType);

            if(automationsResult.Succeeded)
            {
                _automations = automationsResult.Data;
            }

            _loaded = true;
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private async Task SubmitAsync()
        {
            var response = await _automationManager.CreateAutomation
                (new CreateAutomationCommand(JsonSerializer.Serialize(_automationModel), 
                AutomationType.SyncParentAndChildIssues));

            DisplayMessage(response);
        }

        private void DisplayMessage(IResult result)
        {
            if (result == null)
            {
                return;
            }

            var severity = result.Succeeded ? Severity.Success : Severity.Error;

            foreach (var message in result.Messages)
            {
                _snackBar.Add(message, severity);
            }
        }
    }
}
