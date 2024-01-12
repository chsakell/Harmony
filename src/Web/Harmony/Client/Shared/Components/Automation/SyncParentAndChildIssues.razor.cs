using Harmony.Application.DTO.Automation;
using Harmony.Application.Requests.Identity;
using Harmony.Domain.Enums;
using Microsoft.AspNetCore.Components;

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
                BoardId = BoardId,
            };

            var automationsResult = await _automationManager
                .GetAutomations<SyncParentAndChildIssuesAutomationDto>(BoardId, AutomationType);

            if(automationsResult.Succeeded)
            {

            }

            _loaded = true;
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private async Task SubmitAsync()
        {
            //var response = await _userManager.RegisterUserAsync(_registerUserModel);
            //if (response.Succeeded)
            //{
            //    _snackBar.Add(response.Messages[0], Severity.Success);
            //    _navigationManager.NavigateTo("/login");
            //    _registerUserModel = new RegisterRequest();
            //}
            //else
            //{
            //    foreach (var message in response.Messages)
            //    {
            //        _snackBar.Add(message, Severity.Error);
            //    }
            //}
        }
    }
}
