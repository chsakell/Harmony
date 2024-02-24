using Harmony.Application.DTO;
using Harmony.Application.Events;
using Harmony.Application.Features.Cards.Commands.CreateCheckListItem;
using Harmony.Application.Features.Cards.Commands.CreateChildIssue;
using Harmony.Application.Features.Cards.Commands.DeleteChecklist;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Cards.Commands.RemoveCardAttachment;
using Harmony.Application.Features.Cards.Commands.UpdateCardDescription;
using Harmony.Application.Features.Cards.Commands.UpdateCardIssueType;
using Harmony.Application.Features.Cards.Commands.UpdateCardStatus;
using Harmony.Application.Features.Cards.Commands.UpdateCardStoryPoints;
using Harmony.Application.Features.Cards.Commands.UpdateCardTitle;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Features.Cards.Queries.GetActivity;
using Harmony.Application.Features.Cards.Queries.LoadCard;
using Harmony.Application.Features.Comments.Commands.CreateComment;
using Harmony.Application.Features.Comments.Commands.UpdateComment;
using Harmony.Application.Features.Lists.Commands.UpdateCheckListTitle;
using Harmony.Application.Features.Lists.Commands.UpdateListItemChecked;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDescription;
using Harmony.Application.Features.Lists.Commands.UpdateListItemDueDate;
using Harmony.Application.Features.Lists.Queries.GetBoardLists;
using Harmony.Application.Helpers;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Components;
using Harmony.Client.Shared.Dialogs;
using Harmony.Domain.Enums;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardModal : IDisposable
    {
        private EditableCardModel _card = new();
        private bool _loading = true;
        private CreateCommentCommand CreateCommentCommandModel = new CreateCommentCommand();

        [Inject] private IJSRuntime JSRuntime { get; set; }
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        public EditableTextEditorField _commentsTextEditor;
        private bool _historyLoaded = false;
        private bool _updatingStoryPoints;
        private GetBoardListResponse? _cardBoardList;

        [Parameter] public Guid CardId { get; set; }
        [Parameter] public Guid BoardId { get; set; }
        [Parameter] public string BoardKey { get; set; }

        public event EventHandler<EditableCardModel> OnCardUpdated;
        
        protected async override Task OnInitializedAsync()
        {
            _loading = true;

            var loadCardResult = await _cardManager.LoadCardAsync(new LoadCardQuery(CardId));

            if (loadCardResult.Succeeded)
            {
                _card = _mapper.Map<EditableCardModel>(loadCardResult.Data);
                CreateCommentCommandModel.CardId = CardId;
                CreateCommentCommandModel.BoardId = BoardId;

                if (_card.BoardList != null) 
                {
                    _cardBoardList = _card.BoardLists.FirstOrDefault(l => l.Id == _card.BoardList.Id);
                }
            }

            _loading = false;

            RegisterEvents();

            await Task.Run(() =>
            {
                var commentsTask = LoadCommentsTask();

                commentsTask.ContinueWith(res =>
                {
                    var result = res.Result;

                    if (result.Succeeded)
                    {
                        _card.Comments = result.Data;
                        
                        StateHasChanged();
                    }
                });
            });
        }

        private void RegisterEvents()
        {
            _hubSubscriptionManager.OnCardLabelRemoved += OnCardLabelRemoved;
            _hubSubscriptionManager.OnCardMemberAdded += OnCardMemberAdded;
            _hubSubscriptionManager.OnCardMemberRemoved += OnCardMemberRemoved;
            _hubSubscriptionManager.OnCardLabelToggled += OnCardLabelToggled;
            _hubSubscriptionManager.OnCardDatesChanged += OnCardDatesChanged;
            _hubSubscriptionManager.OnCardAttachmentRemoved += OnCardAttachmentRemoved;
        }

        private void UnRegisterEvents()
        {
            _hubSubscriptionManager.OnCardLabelRemoved -= OnCardLabelRemoved;
            _hubSubscriptionManager.OnCardMemberAdded -= OnCardMemberAdded;
            _hubSubscriptionManager.OnCardMemberRemoved -= OnCardMemberRemoved;
            _hubSubscriptionManager.OnCardLabelToggled -= OnCardLabelToggled;
            _hubSubscriptionManager.OnCardDatesChanged -= OnCardDatesChanged;
            _hubSubscriptionManager.OnCardAttachmentRemoved -= OnCardAttachmentRemoved;
        }

        private void OnCardAttachmentRemoved(object? sender, AttachmentRemovedEvent e)
        {
            var attachment = _card.Attachments.FirstOrDefault(x => x.Id == e.AttachmentId);

            if (attachment != null)
            {
                _card.Attachments.Remove(attachment);
                StateHasChanged();
            }
        }

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
                var request = new UploadCardFileCommand
                {
                    Data = buffer,
                    FileName = fileName,
                    Extension = extension,
                    CardId = CardId,
                    Type = FileHelper.GetAttachmentType(extension),
                    BoardId = BoardId
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

        private async Task RemoveAttachment(Guid attachmentId)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to delete this attachment?"},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var command = new RemoveCardAttachmentCommand(CardId, attachmentId)
                {
                    BoardId = BoardId
                };

                var result = await _cardManager.RemoveCardAttachmentAsync(command);

                if (result.Succeeded)
                {
                    var attachment = _card.Attachments.FirstOrDefault(x => x.Id == attachmentId);

                    if (attachment != null)
                    {
                        _card.Attachments.Remove(attachment);
                    }
                }

                DisplayMessage(result);
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


        private async Task AddSubTask()
        {
            var parameters = new DialogParameters<CreateChildIssueModal>
            {
                {
                    modal => modal.CreateChildIssueCommandModel,
                    new CreateChildIssueCommand()
                    {
                        CardId = CardId,
                        BoardId = BoardId
                    }
                },
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateChildIssueModal>(_localizer["Create child issue"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var cardAdded = result.Data as CardDto;
                if (cardAdded != null)
                {
                    _card.Children.Add(cardAdded);
                }
            }
        }

        private async Task EditCard(CardDto card)
        {
            var parameters = new DialogParameters<EditCardModal>
                {
                    { c => c.CardId, card.Id },
                    { c => c.BoardId, BoardId },
                    { c => c.BoardKey, BoardKey }
                };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = _dialogService.ShowAsync<EditCardModal>(_localizer["Edit card"], parameters, options);

            MudDialog.Cancel();
            var dialogResult = await dialog;
        }

        private void OnCardLabelToggled(object? sender, CardLabelToggledEvent e)
        {
            var label = _card.Labels.FirstOrDefault(l => l.Id == e.Label.Id);

            if (e.Label.IsChecked && label == null)
            {
                _card.Labels.Add(e.Label);
            }
            else if (!e.Label.IsChecked && label != null)
            {
                _card.Labels.Remove(label);
            }

            StateHasChanged();
        }

        private void OnCardMemberAdded(object? sender, CardMemberAddedEvent e)
        {
            _card.Members.Add(e.Member);
            StateHasChanged();
        }

        private void OnCardMemberRemoved(object? sender, CardMemberRemovedEvent e)
        {
            var memberToRemove = _card.Members.FirstOrDefault(x => x.Id == e.Member.Id);
            if (memberToRemove != null)
            {
                _card.Members.Remove(memberToRemove);
            }

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
                .UpdateDescriptionAsync(new UpdateCardDescriptionCommand(CardId, cardDescription)
                {
                    BoardId = BoardId
                });

            DisplayMessage(response);
        }

        private async Task SaveIssueType(string summary)
        {
            if (_card.IssueType.Summary == summary)
            {
                return;
            }

            var issueType = _card.IssueTypes.FirstOrDefault(t => t.Summary.Equals(summary));

            if ((issueType == null))
            {
                return;
            }

            _card.IssueType = issueType;

            var command = new UpdateCardIssueTypeCommand(CardId, issueType.Id)
            {
                BoardId = BoardId
            };

            var result = await _cardManager
                .UpdateIssueTypeAsync(command);

            if (result.Succeeded && result.Data)
            {
                _card.IssueType = issueType;
                OnCardUpdated?.Invoke(this, _card);
            }

            DisplayMessage(result);
        }

        async void UpdateStoryPoints(string debouncedText)
        {
            _updatingStoryPoints = true;
            short? storyPoints = string.IsNullOrEmpty(debouncedText) ? null : short.Parse(debouncedText);

            var result = await _cardManager
                .UpdateStoryPointsAsync(new UpdateCardStoryPointsCommand(BoardId, CardId, storyPoints));

            if (result.Succeeded && result.Data)
            {
                _card.StoryPoints = storyPoints;

                OnCardUpdated?.Invoke(this, _card);
            }

            DisplayMessage(result);

            _updatingStoryPoints = false;
        }

        private async Task SaveCheckListTitle(Guid checkListId, string title)
        {
            var response = await _checkListManager
                .UpdateTitleAsync(new UpdateCheckListTitleCommand(checkListId, title)
                {
                    BoardId = BoardId
                });

            var checkList = _card.CheckLists.FirstOrDefault(x => x.Id == checkListId);
            checkList.Title = title;

            DisplayMessage(response);
        }

        private async Task SaveCheckListItemDescription(Guid checkListItemId, string description)
        {
            var response = await _checkListItemManager
                .UpdateListItemDescriptionAsync(new UpdateListItemDescriptionCommand(checkListItemId, description)
                {
                    BoardId = BoardId
                });

            var checkListItem = _card.CheckLists.SelectMany(list => list.Items)
                .FirstOrDefault(x => x.Id == checkListItemId);

            checkListItem.Description = description;

            DisplayMessage(response);
        }

        private async Task SaveTitle(string newTitle)
        {
            var result = await _cardManager.UpdateTitleAsync(new UpdateCardTitleCommand(CardId, newTitle)
            {
                BoardId = BoardId
            });

            if (result.Succeeded)
            {
                _card.Title = newTitle;

                OnCardUpdated?.Invoke(this, _card);
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
                { c => c.CardId, CardId },
                { c => c.BoardId, BoardId }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CardMembersModal>(_localizer["Add card member"], parameters, options);

            var result = await dialog.Result;
        }

        private async Task AddCheckListItem(EditableCheckListItemModel checkListItem)
        {
            var response = await _checkListManager
                .CreateCheckListItemAsync(new CreateCheckListItemCommand(checkListItem.CheckListId,
                checkListItem.Description, checkListItem.DueDate, CardId)
                {
                    BoardId = BoardId
                });

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
                UpdateListItemCheckedCommand(item.Id, !item.IsChecked, CardId)
                {
                    BoardId = BoardId
                });

            DisplayMessage(response);
        }

        private async Task ListItemDueDateChanged(EditableCheckListItemModel item)
        {
            item.DatePicker.Close();

            var response = await _checkListItemManager
                .UpdateListItemDueDateAsync(new
                UpdateListItemDueDateCommand(item.Id, item.DueDate)
                {
                    BoardId = BoardId
                });


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
                var command = new UpdateCardStatusCommand(CardId, Domain.Enums.CardStatus.Archived)
                {
                    BoardId = BoardId
                };

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
                { c => c.BoardId, BoardId },
                { c => c.CardId, CardId },
                { c => c.StartDate, _card.StartDate },
                { c => c.DueDate, _card.DueDate.HasValue ? _card.DueDate.Value.Date : null },
                { c => c.DueTime, _card.DueDate.HasValue ? _card.DueDate.Value.TimeOfDay : new TimeSpan(10, 00, 00) },
                { c => c.DueDateReminder, _card.DueDateReminderType ?? Domain.Enums.DueDateReminderType.None }
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

        private async Task LoadCardHistory()
        {
            if (_historyLoaded)
            {
                return;
            }

            var activityResult = await _cardManager.GetCardActivityAsync(new GetCardActivityQuery(CardId));

            if (activityResult.Succeeded)
            {
                _card.Activities.Clear();
                _card.Activities.AddRange(activityResult.Data);
            }

            _historyLoaded = true;
        }

        private Task<IResult<List<CommentDto>>> LoadCommentsTask()
        {
            return _commentManager.GetCardComments(CardId);
        }

        private void Reactivate()
        {
            _navigationManager.NavigateTo($"/projects/{BoardId}/archived-items");
        }

        private async Task AddComment(string comment)
        {
            if (comment.Equals("<p> </p>") || comment.Equals("<p><br></p>"))
            {
                _snackBar.Add($"Comment cannot be empty", Severity.Error);

                return;
            }

            CreateCommentCommandModel.Text = comment;

            var createCommentResult = await _commentManager
                .CreateCommentAsync(CreateCommentCommandModel);

            if (!createCommentResult.Succeeded)
            {
                DisplayMessage(createCommentResult);
            }

            var commentAdded = createCommentResult.Data;

            _card.Comments.Add(new CommentDto()
            {
                Id = commentAdded.Id,
                Text = comment,
                User = commentAdded.User,
                DateCreated = commentAdded.DateCreated,
                Editable = true
            });

            CreateCommentCommandModel.Text = string.Empty;

            await _commentsTextEditor.Reset();
        }

        private async Task EditComment(Guid commentId)
        {
            var comment = _card.Comments.FirstOrDefault(c => c.Id == commentId);

            IDialogReference dialog = null;

            var parameters = new DialogParameters<EditableTextEditorFieldModal>
            {
                { c => c.EditorCssStyle, string.Empty },
                { c => c.Text, comment.Text },
                { c => c.SaveText, "Update comment" },
                { c => c.SaveIcon, Icons.Material.Filled.InsertComment },
                { c => c.Title, "Edit comment" },
                { c => c.TitleIcon, Icons.Material.Filled.Edit },
                { c => c.OnSave, async (html) => await UpdateComment(comment, html, dialog) },
                { c => c.DisplayCancelButton, false },
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, CloseOnEscapeKey = true, DisableBackdropClick = false };
            dialog = _dialogService.Show<EditableTextEditorFieldModal>(_localizer["Edit comment"], parameters, options);

            var result = await dialog.Result;
        }

        private async Task RemoveComment(Guid commentId)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to delete this comment?"},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var comment = _card.Comments.FirstOrDefault(c => c.Id == commentId);
                if (comment == null)
                {
                    return;
                }

                comment.Editing = true;

                StateHasChanged();

                var result = await _commentManager.DeleteComment(commentId);

                if (result.Succeeded)
                {
                    _card.Comments.Remove(comment);
                }

                DisplayMessage(result);
            }
        }

        private async Task UpdateComment(CommentDto comment, string text, IDialogReference dialog)
        {
            if (text.Equals("<p> </p>") || comment.Equals("<p><br></p>"))
            {
                _snackBar.Add($"Comment cannot be empty", Severity.Error);

                return;
            }

            comment.Editing = true;
            StateHasChanged();

            var updateResult = await _commentManager.UpdateCommentAsync(new UpdateCommentCommand(comment.Id, text));

            if (updateResult.Succeeded && updateResult.Data)
            {
                comment.Text = text;
            }

            comment.Editing = false;
            DisplayMessage(updateResult);

            dialog.Close();
        }

        private async Task UpdateCardBoardList(GetBoardListResponse? boardList)
        {
            var currentBoardListId = _cardBoardList.Id;

            _cardBoardList = boardList;

            var moveUpdateResult = await _cardManager
                    .MoveCardAsync(new MoveCardCommand(_card.Id, boardList.Id, 
                    null, CardStatus.Active, Guid.NewGuid())
                    {
                        BoardId = BoardId
                    });

            if(!moveUpdateResult.Succeeded)
            {
                _cardBoardList = _card.BoardLists.FirstOrDefault(l => l.Id == currentBoardListId);
                DisplayMessage(moveUpdateResult);
            }
        }

        private async Task UpdateChildBoardList(CardDto card, Guid boardListId)
        {
            card.BoardListId = boardListId;

            var moveUpdateResult = await _cardManager
                    .MoveCardAsync(new MoveCardCommand(card.Id, boardListId,
                    null, CardStatus.Active, Guid.NewGuid())
                    {
                        BoardId = BoardId
                    });

            DisplayMessage(moveUpdateResult);
        }

        private async Task ToggleFullScreen(bool fullScreen)
        {
            await JSRuntime.InvokeVoidAsync("toggleFullScreenModal", "editCardModal", fullScreen);
        }

        private Func<IssueTypeDto, string> convertFunc = type =>
        {
            if (type == null || type.Id == Guid.Empty)
            {
                return "Select issue type";
            }

            return type.Summary;
        };

        Func<GetBoardListResponse, string> boardListConverter = p =>
        {
            return p?.Title ?? "Status";
        };

        Func<Guid, List<GetBoardListResponse>, string> childBoardListConverter = (childListId, boardLists) =>
        {
            var boardList = boardLists.FirstOrDefault(l => l.Id == childListId);
            return boardList?.Title ?? "Status";
        };

        private void ViewBacklog(Guid boardId)
        {
            MudDialog.Close();
            _navigationManager.NavigateTo($"/projects/{boardId}/backlog");
        }

        private void ViewBoard(Guid boardId, string boardTitle)
        {
            MudDialog.Close();

            var slug = StringUtilities.SlugifyString(boardTitle);
            _navigationManager.NavigateTo($"boards/{boardId}/{slug}");
        }

        public void Dispose()
        {
            UnRegisterEvents();
        }
    }
}
