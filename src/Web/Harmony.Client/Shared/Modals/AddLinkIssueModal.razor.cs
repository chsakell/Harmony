using Harmony.Application.Extensions;
using Harmony.Application.Features.Cards.Commands.CreateChildIssue;
using Harmony.Application.Features.Cards.Commands.CreateLink;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class AddLinkIssueModal
    {
        private bool _processing;

        private GetBoardListResponse _selectedBoardList;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateLinkCommand CreateLinkCommandModel { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            //CreateLinkCommandModel.ListId = _selectedBoardList.Id;

            //var result = await _cardManager
            //    .CreateChildIssueAsync(CreateLinkCommandModel);

            //MudDialog.Close(result.Data);

            //if (!result.Succeeded)
            //{
            //    DisplayMessage(result);
            //}

            _processing = false;
        }

        Func<LinkType, string> converter = type =>
        {
            return type.GetDescription();
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
