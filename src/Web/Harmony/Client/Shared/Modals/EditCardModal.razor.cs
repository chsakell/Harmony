using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Shared.Components;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardModal
    {
        private EditableCardModel _card = new();
        private bool _loading = true;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        public string NewListName { get; set; }
        public bool NewListOpen { get; set; }
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
                _card = _mapper.Map<EditableCardModel>(loadCardResult.Data);
            }

            _loading = false;
        }

        private async Task SaveDescription(string cardDescription)
        {
            if(cardDescription.Equals("<p> </p>") || cardDescription.Equals("<p><br></p>"))
            {
                cardDescription = null;
            }

            _loading = true;

            var response = await _cardManager
                .UpdateDescriptionAsync(new UpdateCardDescriptionCommand(CardId, cardDescription));

            _card.Description = cardDescription;

            DisplayMessage(response);

            _loading = false;
        }

        private async Task SaveCheckListTitle(Guid checkListId, string title)
        {
            var response = await _checkListManager
                .UpdateTitleAsync(new UpdateListTitleCommand(checkListId, title));

            var checkList = _card.CheckLists.FirstOrDefault(x => x.Id == checkListId);
            checkList.Title = title;

            DisplayMessage(response);
        }

        private async Task SaveCheckListItemDescription(Guid checkListItemId, string description)
        {
            var response = await _checkListItemManager
                .UpdateListItemDescriptionAsync(new UpdateListItemDescriptionCommand(checkListItemId, description));

            var checkListItem = _card.CheckLists.SelectMany(list => list.Items)
                .FirstOrDefault(x => x.Id == checkListItemId);

            checkListItem.Description = description;

            DisplayMessage(response);
        }

        private async Task SaveTitle(string newTitle)
        {
            var result = await _cardManager.UpdateTitleAsync(new UpdateCardTitleCommand(CardId, newTitle));

            if (result.Succeeded)
            {
                _card.Title = newTitle;
            }

            DisplayMessage(result);
        }

        private async Task AddCheckList()
        {
            var parameters = new DialogParameters<CreateCheckListModal>
            {
                { c => c.CardId, CardId }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateCheckListModal>(_localizer["Create check list"], parameters, options);
            var result = await dialog.Result;
            if (!result.Cancelled)
            {
                // TODO update workspace list or navigate to it
            }
        }

        private async Task AddCheckListItem(EditableCheckListItemModel checkListItem)
        {
            var response = await _checkListManager
                .CreateCheckListItemAsync(new CreateCheckListItemCommand(checkListItem.CheckListId,
                checkListItem.Description, checkListItem.DueDate));

            var list = _card.CheckLists.FirstOrDefault(list => list.Id == checkListItem.CheckListId);
            if (list != null)
            {
                list.NewItem = null;

                if (response.Succeeded)
                {
                    var itemAdded = _mapper.Map<EditableCheckListItemModel>(response.Data);
                    list.Items.Add(itemAdded);
                }
            }

            DisplayMessage(response);
        }

        private async Task ToggleListItemChecked(EditableCheckListItemModel item)
        {
            var response = await _checkListItemManager
                .UpdateListItemCheckedAsync(new 
                UpdateListItemCheckedCommand(item.Id, !item.IsChecked));


            DisplayMessage(response);
        }

        private async Task ListItemDueDateChanged(EditableCheckListItemModel item)
        {
            item.DatePicker.Close();

            var response = await _checkListItemManager
                .UpdateListItemDueDateAsync(new
                UpdateListItemDueDateCommand(item.Id, item.DueDate));


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
