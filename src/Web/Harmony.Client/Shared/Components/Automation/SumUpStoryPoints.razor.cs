using Harmony.Application.DTO;
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
    public partial class SumUpStoryPoints
    {
        [Parameter]
        public AutomationType AutomationType { get; set; }

        [Parameter] public Guid BoardId { get; set; }

        private bool _loaded = false;
        private bool _creating = false;
        private bool _updating = false;
        private bool _toggling = false;
        private bool _removing = false;

        private SumUpStoryPointsAutomationDto _selectedAutomationModel;
        private SumUpStoryPointsAutomationDto _newAutomationModel;

        private List<SumUpStoryPointsAutomationDto>? _automations = new List<SumUpStoryPointsAutomationDto>();

        private List<IssueTypeDto> _issueTypes;
        private IssueTypeDto _selectedIssueType;
        private IEnumerable<IssueTypeDto> _selectedIssueTypes { get; set; } = Enumerable.Empty<IssueTypeDto>();

        protected override async Task OnInitializedAsync()
        {
            _newAutomationModel =
            new SumUpStoryPointsAutomationDto()
            {
                Type = AutomationType,
                BoardId = BoardId.ToString(),
                Enabled = true
            };

            var automationsResult = await _automationManager
                .GetAutomations<SumUpStoryPointsAutomationDto>(BoardId, AutomationType);

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

            await LoadIssueTypes();

            SelectAutomationIssueTypes(_selectedAutomationModel);

            _loaded = true;
        }

        private async Task LoadIssueTypes()
        {
            if (_issueTypes != null)
            {
                return;
            }

            var issueTypesResult = await _boardManager.GetIssueTypesAsync(BoardId.ToString());

            if (issueTypesResult.Succeeded)
            {
                _issueTypes = issueTypesResult.Data;
            }
        }

        private void SetFromIssueTypes(IEnumerable<IssueTypeDto> issueTypes)
        {
            _selectedIssueTypes = issueTypes;

            SetDescription();
        }

        private void SetDescription()
        {
            var selectedIssueTypes = string.Join(",", _selectedIssueTypes.Select(i => i.Summary));
            var selectedIssueTypesText = string.IsNullOrEmpty(selectedIssueTypes) ? "any issue type" : 
                $"[{selectedIssueTypes}] issues";

            _selectedAutomationModel.Description = $"- When subtasks story points change for {selectedIssueTypesText}, " +
                $"parent's story points will be equal to their total sum {Environment.NewLine}" +
                $"- You cannot directly change parent's story points when there are subtasks";
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private async Task SubmitAsync()
        {
            _creating = true;
            var isNewRule = string.IsNullOrEmpty(_selectedAutomationModel.Id);

            _selectedAutomationModel.IssueTypes = _selectedIssueTypes?.Select(s => s.Id.ToString()) ?? Enumerable.Empty<string>();

            var response = await _automationManager.CreateAutomation
                (new CreateAutomationCommand(JsonSerializer.Serialize(_selectedAutomationModel),
                AutomationType.SumUpStoryPoints));

            if (response.Succeeded)
            {
                if (isNewRule)
                {
                    _selectedAutomationModel.Id = response.Data;
                    _automations.Add(_selectedAutomationModel);
                }

                _newAutomationModel = new SumUpStoryPointsAutomationDto()
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
                { x => x.ContentText, $"Are you sure you want to remove this rule?"},
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
                    SelectAutomationIssueTypes(_selectedAutomationModel);
                }

                DisplayMessage(removeResult);
            }
        }

        private void SelectAutomationIssueTypes(SumUpStoryPointsAutomationDto automation)
        {
            _selectedAutomationModel = automation;

            if (string.IsNullOrEmpty(automation.Id))
            {
                _selectedIssueTypes = Enumerable.Empty<IssueTypeDto>();
            }
            else
            {
                _selectedIssueTypes = _issueTypes.Where(l => _selectedAutomationModel
                    .IssueTypes.ToList().Contains(l.Id.ToString())).ToList();
            }

            SetDescription();
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

        Func<IssueTypeDto, string> converter = p =>
        {
            return p?.Summary;
        };

        Func<SumUpStoryPointsAutomationDto, string> automationConverter = p =>
        {
            return p?.Name;
        };
    }
}
