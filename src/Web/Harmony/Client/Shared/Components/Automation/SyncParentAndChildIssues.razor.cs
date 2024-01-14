using Harmony.Application.DTO.Automation;
using Harmony.Application.Features.Automations.Commands.CreateAutomation;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Domain.Entities;
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
        private List<GetBoardListResponse> _boardLists = new List<GetBoardListResponse>();

        private GetBoardListResponse _fromStatusBoardList;
        private IEnumerable<GetBoardListResponse> _fromStatusBoardLists { get; set; } = new List<GetBoardListResponse>() { };
        private GetBoardListResponse _toStatusBoardList;
        private IEnumerable<GetBoardListResponse> _toStatusBoardLists { get; set; } = new List<GetBoardListResponse>() { };

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

            var boardListsResult = await _boardManager
                .GetBoardListsAsync(BoardId.ToString());

            if ((boardListsResult.Succeeded))
            {
                _boardLists = boardListsResult.Data;
            }

            _loaded = true;
        }

        protected override Task OnParametersSetAsync()
        {
            return base.OnParametersSetAsync();
        }

        private async Task SubmitAsync()
        {
            _automationModel.FromStatuses = _fromStatusBoardLists?.Select(s => s.Id.ToString()) ?? Enumerable.Empty<string>();
            _automationModel.ToStatuses = _toStatusBoardLists?.Select(s => s.Id.ToString()) ?? Enumerable.Empty<string>();

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

        Func<GetBoardListResponse, string> converter = p =>
        { 
            return p?.Title ;
        };
    }
}
