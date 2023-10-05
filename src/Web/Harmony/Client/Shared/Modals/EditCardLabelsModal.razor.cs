using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Queries.GetLabels;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardLabelsModal
    {
        private List<LabelDto> _cardLabels = new();
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid CardId { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var cardLabelsResponse = await _cardManager
                .GetCardLabelsAsync(new GetCardLabelsQuery(CardId));

            if(cardLabelsResponse.Succeeded)
            {
                _cardLabels = cardLabelsResponse.Data;
            }
        }

        private async Task UpdateLabelTitle(LabelDto label, string title)
        {
            var result = await _labelManager
                .UpdateLabelTitle(new UpdateLabelTitleCommand(label.Id, title));

            if(result.Succeeded)
            {
                label.Title = title;
            }

            DisplayMessage(result);
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
