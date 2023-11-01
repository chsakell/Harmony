using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.DeleteChecklist;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Cards.Commands.UploadFile;
using Harmony.Application.Features.Cards.Queries.GetActivity;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Lists.Commands.UpdateCheckListTitle;
using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;
using Harmony.Application.Features.Lists.Commands.UpdateListTitle;
using Harmony.Application.Helpers;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardModal
    {
        private EditableCardModel _card = new();
        private bool _loading = true;

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter] public Guid CardId { get; set; }
        [Parameter] public Guid BoardId { get; set; }

        private async Task UploadFiles(IReadOnlyList<IBrowserFile> files)
        {
            const long maxAllowedImageSize = 10000000;

            foreach (var file in files)
            {
                _card.UploadingAttachment = true;

                var extension = Path.GetExtension(file.Name);
                var fileName = file.Name;
                var buffer = new byte[file.Size];
                await file.OpenReadStream(maxAllowedImageSize).ReadAsync(buffer);
                var request = new UploadFileCommand
                {
                    Data = buffer,
                    FileName = fileName,
                    Extension = extension,
                    CardId = CardId,
                    Type = FileHelper.GetAttachmentType(extension)
                };

                var result = await _fileManager.UploadFile(request);

                if (result.Succeeded)
                {
                    _card.Attachments.Add(result.Data.Attachment);
                }

                DisplayMessage(result);

                _card.UploadingAttachment = false;
            }
        }

        private async Task DeleteCheckList(Guid checkListId)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to delete this checklist? " +
                $"You won't be able to restore its items" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var command = new DeleteCheckListCommand(checkListId);
                var result = await _checkListManager.DeleteCheckListAsync(command);

                if (result.Succeeded && result.Data)
                {
                    var checkList = _card.CheckLists.FirstOrDefault(x => x.Id == checkListId);

                    if (checkList != null)
                    {
                        _card.CheckLists.Remove(checkList);
                    }
                }

                DisplayMessage(result);
            }
        }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected async override Task OnInitializedAsync()
        {
            _loading = true;

            var loadCardResult = await _cardManager.LoadCardAsync(new LoadCardQuery(CardId));

            if (loadCardResult.Succeeded)
            {
                _card = _mapper.Map<EditableCardModel>(loadCardResult.Data);
            }

            _loading = false;

            _hubSubscriptionManager.OnCardLabelRemoved += OnCardLabelRemoved;
            _hubSubscriptionManager.OnCardMemberAdded += OnCardMemberAdded;
            _hubSubscriptionManager.OnCardLabelToggled += OnCardLabelToggled;
            _hubSubscriptionManager.OnCardDatesChanged += OnCardDatesChanged;
        }

        private void OnCardLabelToggled(object? sender, CardLabelToggledEvent e)
        {
            if(e.Label.IsChecked)
            {
                _card.Labels.Add(e.Label);
            }
            else
            {
                var label = _card.Labels.FirstOrDefault(l => l.Id == e.Label.Id);
                if (label != null)
                {
                    _card.Labels.Remove(label);
                }
            }

            StateHasChanged();
        }

        private void OnCardMemberAdded(object? sender, CardMemberAddedEvent e)
        {
            _card.Members.Add(e.Member);
            StateHasChanged();
        }

        private void OnCardLabelRemoved(object? sender, CardLabelRemovedEvent e)
        {
            var label = _card.Labels.FirstOrDefault(l => l.Id == e.CardLabelId);

            if (label != null)
            {
                _card.Labels.Remove(label);
            }
        }

        private void OnCardDatesChanged(object? sender, CardDatesChangedEvent e)
        {
            _card.StartDate = e.StartDate;
            _card.DueDate = e.DueDate;

            StateHasChanged();
        }

        private async Task SaveDescription(string cardDescription)
        {
            if (cardDescription.Equals("<p> </p>") || cardDescription.Equals("<p><br></p>"))
            {
                cardDescription = null;
            }

            var response = await _cardManager
                .UpdateDescriptionAsync(new UpdateCardDescriptionCommand(CardId, cardDescription));

            _card.Description = cardDescription;

            DisplayMessage(response);
        }

        private async Task SaveCheckListTitle(Guid checkListId, string title)
        {
            var response = await _checkListManager
                .UpdateTitleAsync(new UpdateCheckListTitleCommand(checkListId, title));

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

            if (result.Data is CheckListDto checkList)
            {
                var checkListAdded = _mapper.Map<EditableCheckListModel>(checkList);
                _card.CheckLists.Add(checkListAdded);
            }
        }

        private async Task AddCardMembers()
        {
            var parameters = new DialogParameters<CardMembersModal>
            {
                { c => c.CardId, CardId }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CardMembersModal>(_localizer["Add card member"], parameters, options);

            var result = await dialog.Result;
        }

        private async Task AddCheckListItem(EditableCheckListItemModel checkListItem)
        {
            var response = await _checkListManager
                .CreateCheckListItemAsync(new CreateCheckListItemCommand(checkListItem.CheckListId,
                checkListItem.Description, checkListItem.DueDate, CardId));

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
                UpdateListItemCheckedCommand(item.Id, !item.IsChecked, CardId));

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

        private async Task ArchiveCard()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to archive this card?" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var command = new UpdateCardStatusCommand(CardId, Domain.Enums.CardStatus.Archived);
                var result = await _cardManager
                    .UpdateStatusAsync(command);

                if (result.Succeeded && result.Data)
                {
                    MudDialog.Close(command);
                }

                DisplayMessage(result);
            }
        }

        private async Task EditLabels()
        {
            var parameters = new DialogParameters<EditCardLabelsModal>
            {
                { c => c.CardId, CardId },
                { c => c.BoardId,  BoardId }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<EditCardLabelsModal>(_localizer["Edit labels"], parameters, options);

            var result = await dialog.Result;
        }

        private async Task EditDates()
        {
            var parameters = new DialogParameters<EditCardDatesModal>
            {
                { c => c.CardId, CardId },
                { c => c.StartDate, _card.StartDate },
                { c => c.DueDate, _card.DueDate }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<EditCardDatesModal>(_localizer["Edit dates"], parameters, options);

            var result = await dialog.Result;
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


        private async Task LoadCardActivity(bool newVal)
        {
            if (newVal)
            {
                var activityResult = await _cardManager.GetCardActivityAsync(new GetCardActivityQuery(CardId));

                if(activityResult.Succeeded)
                {
                    _card.Activities.Clear();
                    _card.Activities.AddRange(activityResult.Data);
                }
            }
            //else
            //{
            //    // Reset after a while to prevent sudden collapse.
            //    Task.Delay(350).ContinueWith(t => _panelContent = null).AndForget();
            //}
        }
    }
}
