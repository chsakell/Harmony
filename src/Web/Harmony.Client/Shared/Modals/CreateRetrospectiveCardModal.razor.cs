﻿using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.CreateCard;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CreateRetrospectiveCardModal
    {
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateCardCommand CreateCardCommandModel { get; set; }

        [Parameter]
        public BoardType BoardType { get; set; }

        [Parameter]
        public string ListTitle { get; set; }

        private List<IssueTypeDto> _issueTypes = new List<IssueTypeDto>();

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var issueTypesResult = await _boardManager.GetIssueTypesAsync(CreateCardCommandModel.BoardId.ToString());

            if(issueTypesResult.Succeeded)
            {
                _issueTypes = issueTypesResult.Data;
            }
        }

        private async Task SubmitAsync()
        {
            _processing = true;
            
            var result = await _cardManager.CreateCardAsync(CreateCardCommandModel);

            MudDialog.Close(result.Data);

            if (!result.Succeeded)
            {
                DisplayMessage(result);
            }

            _processing = false;
        }

        private Func<IssueTypeDto, string> convertFunc = type =>
        {
            if(type.Id == Guid.Empty)
            {
                return "Select issue type";
            }

            return type.Summary;
        };

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
