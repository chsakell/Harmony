using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Client.Shared.Components;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardModal
    {
        private LoadCardResponse _card = new();
        private bool _loading = true;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        private PostTextEditor _textEditor;
        public string NewListName { get; set; }
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

            _loading = false;
        }

        private async Task SaveDescription()
        {
            _loading = true;
            var cardDescription = await _textEditor.GetHTML();

            var response = await _cardManager
                .UpdateDescriptionAsync(new UpdateCardDescriptionCommand(CardId, cardDescription));

            DisplayMessage(response);

            _loading = false;
        }

        private async Task AddCheckList()
        {
            var position = (byte)_card.CheckLists.Count;

            var response = await _checkListManager
                .CreateCheckListAsync(new CreateChecklistCommand(CardId, NewListName, position));

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
