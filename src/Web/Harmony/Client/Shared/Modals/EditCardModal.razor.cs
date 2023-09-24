using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardModal
    {
        private LoadCardResponse _card = new();
        private bool _processing;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        [Parameter] public Guid CardId { get; set; }
        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected async override Task OnInitializedAsync()
        {
            var loadCardResult = await _cardManager.LoadCardAsync(new LoadCardQuery(CardId));

            if (loadCardResult.Succeeded)
            {
                _card = loadCardResult.Data;
            }
        }

        private async Task SubmitAsync()
        {
            _processing = true;
            //var response = await _boardManager.CreateAsync(_createBoardModel);

            //if (response.Succeeded)
            //{
            //    _snackBar.Add(response.Messages[0], Severity.Success);
            //    MudDialog.Close();
            //}
            //else
            //{
            //    foreach (var message in response.Messages)
            //    {
            //        _snackBar.Add(message, Severity.Error);
            //    }
            //}

            _processing = false;
        }
    }
}
