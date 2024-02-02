using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text.Json;

namespace Harmony.Client.Shared.Components.Automation
{
    public partial class SmartAutoAssign
    {
        [Parameter]
        public AutomationType AutomationType { get; set; }

        [Parameter] public Guid BoardId { get; set; }

        private bool _loaded = false;
        private bool _creating = false;
        private bool _updating = false;
        private bool _toggling = false;
        private bool _removing = false;

        private SmartAutoAssignAutomationDto _selectedAutomationModel;
        private SmartAutoAssignAutomationDto _newAutomationModel;

        private List<SmartAutoAssignAutomationDto>? _automations = new List<SmartAutoAssignAutomationDto>();

        protected override async Task OnInitializedAsync()
        {
            _newAutomationModel =
            new SmartAutoAssignAutomationDto()
            {
                Type = AutomationType,
                BoardId = BoardId.ToString(),
                Enabled = true
            };

            var automationsResult = await _automationManager
                .GetAutomations<SmartAutoAssignAutomationDto>(BoardId, AutomationType);

            if (automationsResult.Succeeded)
            {
                _automations = automationsResult.Data;

                if (_automations.Any())
                {
                    _selectedAutomationModel = _automations.First();
                }
                else
                {
                    _selectedAutomationModel = _newAutomationModel;
                }
            }

            SetDescription();

            _loaded = true;
        }

        private void SetDescription()
        {
            _selectedAutomationModel.Description = $"Smart auto sync";
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private async Task SubmitAsync()
        {
            _creating = true;
            var isNewRule = string.IsNullOrEmpty(_selectedAutomationModel.Id);

            var response = await _automationManager.CreateAutomation
                (new CreateAutomationCommand(JsonSerializer.Serialize(_selectedAutomationModel),
                AutomationType.SmartAutoAssign));

            if (response.Succeeded)
            {
                if (isNewRule)
                {
                    _selectedAutomationModel.Id = response.Data;
                    _automations.Add(_selectedAutomationModel);
                }

                _newAutomationModel = new SmartAutoAssignAutomationDto()
                {
                    Type = AutomationType,
                    BoardId = BoardId.ToString(),
                    Enabled = true
                };
            }

            _creating = false;

            DisplayMessage(response);
        }

        private async Task ToggleStatus()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want " +
                $"to {(_selectedAutomationModel.Enabled ? "disable" : "enable" )} this rule?"},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Warning }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var toggleResult = await _automationManager.ToggleAutomation(new ToggleAutomationCommand(_selectedAutomationModel.Id, !_selectedAutomationModel.Enabled));

                if (toggleResult.Succeeded)
                {
                    _selectedAutomationModel.Enabled = !_selectedAutomationModel.Enabled;
                }

                DisplayMessage(toggleResult);
            }
        }

        private async Task Remove()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want " +
                $"to remove this rule?"},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var removeResult = await _automationManager.RemoveAutomation(_selectedAutomationModel.Id);

                if (removeResult.Succeeded)
                {
                    _automations.Remove(_selectedAutomationModel);

                    _selectedAutomationModel = _automations.Any() ? _automations.First() : _newAutomationModel;
                }

                DisplayMessage(removeResult);
            }
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

        Func<SmartAutoAssignAutomationDto, string> converter = p =>
        {
            return p?.Name;
        };

        Func<SmartAutoAssignOption, string> optionConverter = p =>
        {
            return p switch
            {
                SmartAutoAssignOption.IssueCreator => "Issue creator",
                SmartAutoAssignOption.SpecificUser => "Specify user",
                _ => "Assign the issue to"
            };
        };

        Func<AutomationTriggerSchedule, string> triggerScheduleConverter = p =>
        {
            return p switch
            {
                AutomationTriggerSchedule.Instantly => "Instantly",
                AutomationTriggerSchedule.After_5_Minutes => "After 5 minutes",
                AutomationTriggerSchedule.After_15_Minutes => "After 15 minutes",
                AutomationTriggerSchedule.After_30_Minutes => "After 30 minutes",
                AutomationTriggerSchedule.After_1_Hour => "After 1 hour",
                AutomationTriggerSchedule.After_3_Hours => "After 3 hours",
                AutomationTriggerSchedule.After_6_Hours => "After 6 hours",
                AutomationTriggerSchedule.After_1_Day => "After 1 day",
                _ => "Run trigger at"
            };
        };
    }
}
