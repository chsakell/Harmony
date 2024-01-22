using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class Sprints : IDisposable
    {
        [Parameter]
        public string Id { get; set; }

        private string _searchString = "";
        private int _totalItems;
        private List<GetSprintCardResponse> _cards = new List<GetSprintCardResponse>();
        private MudTable<GetSprintCardResponse> _table;
        private HashSet<GetSprintCardResponse> _selectedCards = new HashSet<GetSprintCardResponse>();
        private int _filterSprintStatus = -1;
        private IDisposable registration;
        private List<IssueTypeDto> _issueTypes;

        private TableGroupDefinition<GetSprintCardResponse> _groupDefinition = new()
        {
            GroupName = "Sprint",
            Indentation = false,
            Expandable = true,
            IsInitiallyExpanded = true,
            Selector = (e) => e.Sprint + $" [{e.SprintStatus}]"
        };

        protected override async Task OnInitializedAsync()
        {
            await _hubSubscriptionManager.ListenForBoardEvents(Id);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await LoadIssueTypes();
            }
        }

        protected override void OnAfterRender(bool firstRender)
        {
            if (firstRender)
            {
                registration = _navigationManager.RegisterLocationChangingHandler(LocationChangingHandler);
            }
        }

        private async ValueTask LocationChangingHandler(LocationChangingContext arg)
        {
            if (!arg.TargetLocation.Contains(Id))
            {
                await _hubSubscriptionManager.StopListeningForBoardEvents(Id);
            }
        }

        private async Task<TableData<GetSprintCardResponse>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<GetSprintCardResponse>
            {
                TotalItems = _totalItems,
                Items = _cards
            };
        }

        private async Task MoveToBacklog()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to move the selected cards to backlog?"},
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Warning }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var cardIds = _selectedCards.Where(c => c.CardId.HasValue)
                                        .Select(c => c.CardId.Value).ToList();

                var request = new MoveToBacklogCommand(Guid.Parse(Id), cardIds);

                var result = await _boardManager.MoveCardsToBacklog(request);

                if (result.Succeeded)
                {
                    await _table.ReloadServerData();
                }

                DisplayMessage(result);
            }
        }

        private async Task EditCard(GetSprintCardResponse item)
        {
            var parameters = new DialogParameters<EditCardModal>
                {
                    { c => c.CardId, item.CardId.Value },
                    { c => c.BoardId, Guid.Parse(Id) },
                    { c => c.BoardKey, item.BoardKey }
                };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = await _dialogService.ShowAsync<EditCardModal>(_localizer["Edit card"], parameters, options);

            var editCardModal = dialog.Dialog as EditCardModal;

            editCardModal.OnCardUpdated += (object? sender, EditableCardModel e) => EditCardModal_OnCardUpdated(sender, e, item);

            var result = await dialog.Result;

            editCardModal.OnCardUpdated -= (object? sender, EditableCardModel e) => EditCardModal_OnCardUpdated(sender, e, item); ;
        }

        private void EditCardModal_OnCardUpdated(object? sender, EditableCardModel e, GetSprintCardResponse item)
        {
            item.CardTitle = e.Title;
            item.StoryPoints = e.StoryPoints;
            item.CardDueDate = e.DueDate;
            item.CardIssueType = e.IssueType;
        }

        private async Task FilterSprintStatus(int status)
        {
            _filterSprintStatus = status;
            await _table.ReloadServerData();
        }

        private async Task StartSprint(Guid sprintId, string sprintName)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to start {sprintName}? " +
                $"All of it's cards will be available on the board" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Warning }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var request = new StartSprintCommand(Guid.Parse(Id), sprintId);

                var result = await _sprintManager.StartSprint(request);

                if (result.Succeeded)
                {
                    await _table.ReloadServerData();
                }

                DisplayMessage(result);
            }
        }

        private async Task CompleteSprint(Guid sprintId, string sprintName)
        {
            var pendingSprintResult = await _boardManager
                .GetPendingSprintCards(new GetPendingSprintCardsQuery(Guid.Parse(Id), sprintId));

            if (pendingSprintResult.Succeeded)
            {
                var pendingCards = pendingSprintResult.Data.PendingCards;
                var availableSprints = pendingSprintResult.Data.AvailableSprints;

                var parameters = new DialogParameters<CompleteSprintModal>
                {
                    { x => x.BoardId, Guid.Parse(Id) },
                    { x => x.PendingCards, pendingCards },
                    { x => x.AvailableSprints, availableSprints },
                    { x => x.SprintId , sprintId },
                };

                var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
                var dialog = _dialogService.Show<CompleteSprintModal>(_localizer["Complete sprint"], parameters, options);

                var result = await dialog.Result;

                if (!result.Canceled && result.Data is bool sprintSucceeded)
                {
                    await _table.ReloadServerData();
                }
            }
        }

        private async Task ViewReports(Guid sprintId)
        {
            var parameters = new DialogParameters<SprintReportsModal>
            {
                {
                    modal => modal.BoardId, Guid.Parse(Id)
                },
                {
                    modal => modal.SprintId, sprintId
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullScreen = false, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<SprintReportsModal>(_localizer["Sprint reports"], parameters, options);
            var result = await dialog.Result;
        }

        private async Task CreateSprint()
        {
            var parameters = new DialogParameters<CreateEditSprintModal>
            {
                {
                    modal => modal.CreateEditSprintCommandModel,
                    new CreateEditSprintCommand(Guid.Parse(Id))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateEditSprintModal>(_localizer["Create sprint"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task EditSprint(GetSprintCardResponse sprint)
        {
            var parameters = new DialogParameters<CreateEditSprintModal>
            {
                {
                    modal => modal.CreateEditSprintCommandModel,
                    new CreateEditSprintCommand(Guid.Parse(Id))
                    {
                        SprintId = sprint.SprintId,
                        Name = sprint.Sprint,
                        StartDate = sprint.SprintStartDate,
                        EndDate = sprint.SprintEndDate,
                        Goal = sprint.SprintGoal
                    }
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateEditSprintModal>(_localizer["Create sprint"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new GetSprintCardsQuery(Guid.Parse(Id))
            {
                BoardId = Guid.Parse(Id),
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
                OrderBy = orderings,
                SprintStatus = _filterSprintStatus == -1 ? null : (SprintStatus)_filterSprintStatus
            };

            var response = await _boardManager.GetSprintCards(request);

            if (response.Succeeded)
            {
                _totalItems = response.TotalCount;
                _cards = response.Data;
            }
            else
            {
                foreach (var message in response.Messages)
                {
                    _snackBar.Add(message, Severity.Error);
                }
            }
        }

        private void OnSearch(string text)
        {
            _searchString = string.IsNullOrEmpty(text) ? null : text;
            _table.ReloadServerData();
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

        private async Task LoadIssueTypes()
        {
            if (_issueTypes != null)
            {
                return;
            }

            var issueTypesResult = await _boardManager.GetIssueTypesAsync(Id);

            if (issueTypesResult.Succeeded)
            {
                _issueTypes = issueTypesResult.Data;
            }
        }

        void IDisposable.Dispose()
        {
            registration?.Dispose();
        }
    }
}
