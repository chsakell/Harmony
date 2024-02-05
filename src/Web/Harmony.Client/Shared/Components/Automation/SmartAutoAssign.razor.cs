using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Automations.Commands.ToggleAutomation;
using Harmony.Application.Features.Boards.Queries.SearchBoardUsers;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums.Automations;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Text;
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
        
        private bool _searching;
        private SearchBoardUserResponse _selectedUser;

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

                    if(_selectedAutomationModel.Option == SmartAutoAssignOption.SpecificUser)
                    {
                        _selectedUser = new SearchBoardUserResponse()
                        {
                            Id = _selectedAutomationModel.UserId,
                            FirstName = _selectedAutomationModel.FirstName,
                            LastName = _selectedAutomationModel.LastName,
                            UserName = _selectedAutomationModel.UserName,
                        };
                    }
                }
                else
                {
                    _selectedAutomationModel = _newAutomationModel;
                }
            }

            SetDescription();

            _loaded = true;
        }

        private void SetFromParentIfSubtask(bool setFromParentIfSubtask)
        {
            _selectedAutomationModel.SetFromParentIfSubtask = setFromParentIfSubtask;

            SetDescription();
        }

        private void SetAssignIfNoneAssigned(bool assignIfNoneAssigned)
        {
            _selectedAutomationModel.AssignIfNoneAssigned = assignIfNoneAssigned;

            SetDescription();
        }

        private void SetUser(SearchBoardUserResponse user)
        {
            _selectedUser = user;

            SetDescription();
        }

        private void SetOption(SmartAutoAssignOption option)
        {
            _selectedAutomationModel.Option = option;

            SetDescription();
        }

        private void SetDescription()
        {
            var specificUserText = string.IsNullOrEmpty(_selectedUser?.Id) ? "The selected user" : _selectedUser.FullName;
            var issueCreatorText = "- The user that creates the issue";
            var overrideIfSubtaskText = "- Subtasks will be assigned with parent's assignee if any.";
            var assignOnlyIfNotAlreadyAssignedText = "- Assignement will be skipped if already someone else assigned";
            var builder = new StringBuilder();
            if(_selectedAutomationModel.Option == SmartAutoAssignOption.SpecificUser)
            {
                builder.Append(specificUserText);
            }
            else
            {
                builder.Append(issueCreatorText);
            }
            builder.AppendLine(" will be the assignee for new issues.");

            if(_selectedAutomationModel.SetFromParentIfSubtask)
            {
                builder.AppendLine(overrideIfSubtaskText);
            }

            if(_selectedAutomationModel.AssignIfNoneAssigned)
            {
                builder.AppendLine(assignOnlyIfNotAlreadyAssignedText);
            }

            _selectedAutomationModel.Description = builder.ToString();
        }

        private async Task<IEnumerable<SearchBoardUserResponse>> SearchUsers(string value)
        {
            if (string.IsNullOrEmpty(value) || value.Length < 4 || _searching)
            {
                return Enumerable.Empty<SearchBoardUserResponse>();
            }

            _searching = true;
            var searchResult = await _boardManager.SearchBoardMembersAsync(BoardId.ToString(), value);

            if (searchResult.Succeeded)
            {
                _searching = false;
                return searchResult.Data;
            }

            _searching = false;
            return Enumerable.Empty<SearchBoardUserResponse>();
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private async Task SubmitAsync()
        {
            _creating = true;

            if(_selectedAutomationModel.Option == SmartAutoAssignOption.SpecificUser)
            {
                if(_selectedUser == null)
                {
                    DisplayMessage(Result.Fail("Specify the user to be assigned to new issues."));
                    _creating = false;
                    return;
                }

                _selectedAutomationModel.UserId = _selectedUser.Id;
                _selectedAutomationModel.FirstName = _selectedUser.FirstName;
                _selectedAutomationModel.LastName = _selectedUser.LastName;
                _selectedAutomationModel.UserName = _selectedUser.UserName;
            }
            else
            {
                _selectedAutomationModel.UserId = null;
                _selectedAutomationModel.FirstName = null;
                _selectedAutomationModel.LastName = null;
                _selectedAutomationModel.UserName = null;
            }

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
