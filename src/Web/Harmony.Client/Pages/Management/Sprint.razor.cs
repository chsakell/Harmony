
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetSprintsSummary;
using Harmony.Application.Features.Cards.Commands.CreateSprintIssue;
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Retrospectives.Commands.Create;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Sprints.Queries.GetSprintCards;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Domain.Enums;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class Sprint
    {
        [Parameter]
        public string Id { get; set; }

        [Parameter]
        public string SprintId { get; set; }

        private List<CardDto> _cards;
        private MudTable<CardDto> _table;
        private string _searchString = "";
        private int _totalItems;
        private SprintDto _sprint;
        public SprintStatus? _status { get; set; } = null;

        private bool _loading;
        private HashSet<CardDto> _selectedCards = new HashSet<CardDto>();

        protected override async Task OnInitializedAsync()
        {
            var sprintResult = await _sprintManager.GetSprint(Guid.Parse(SprintId));

            if (sprintResult.Succeeded)
            {
                _sprint = sprintResult.Data;
            }
        }

        private async Task<TableData<CardDto>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<CardDto>
            {
                TotalItems = _totalItems,
                Items = _cards
            };
        }

        private async Task CreateIssue()
        {
            var parameters = new DialogParameters<CreateSprintIssueModal>
            {
                {
                    modal => modal.CreateSprintIssueCommandModel,
                    new CreateSprintIssueCommand(Guid.Parse(Id), Guid.Parse(SprintId))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateSprintIssueModal>(_localizer["Create sprint item"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task CreateRetrospective()
        {
            var parameters = new DialogParameters<CreateRetrospectiveModal>
            {
                {
                    modal => modal.CreateRetrospectiveCommandModel,
                    new CreateRetrospectiveCommand(Guid.Parse(Id))
                    {
                        SprintId = Guid.Parse(SprintId),
                    }
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Small, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateRetrospectiveModal>(_localizer["Create retrospective"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                var retrospective = result.Data as RetrospectiveDto;
                if (retrospective != null)
                {

                }
            }
        }

        private async Task EditCard(CardDto card)
        {
            var parameters = new DialogParameters<EditCardModal>
                {
                    { c => c.CardId, card.Id },
                    { c => c.BoardId, Guid.Parse(Id) },
                    { c => c.BoardKey, _sprint.Board.Key }
                };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Large, FullWidth = true, DisableBackdropClick = false };
            var dialog = await _dialogService.ShowAsync<EditCardModal>(_localizer["Edit card"], parameters, options);

            var editCardModal = dialog.Dialog as EditCardModal;

            editCardModal.OnCardUpdated += (object? sender, EditableCardModel e) => EditCardModal_OnCardUpdated(sender, e, card);

            var result = await dialog.Result;

            editCardModal.OnCardUpdated -= (object? sender, EditableCardModel e) => EditCardModal_OnCardUpdated(sender, e, card); ;
        }

        private void EditCardModal_OnCardUpdated(object? sender, EditableCardModel e, CardDto item)
        {
            item.Title = e.Title;
            item.StoryPoints = e.StoryPoints;
            item.DueDate = e.DueDate;
            item.IssueType = e.IssueType;
        }

        private async Task StartSprint()
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to start {_sprint.Name}? " +
                $"All of it's cards will be available on the board" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Warning }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var request = new StartSprintCommand(Guid.Parse(Id), _sprint.Id);

                var result = await _sprintManager.StartSprint(request);

                DisplayMessage(result);
            }
        }

        private async Task CompleteSprint()
        {
            var pendingSprintResult = await _boardManager
                .GetPendingSprintCards(new GetPendingSprintCardsQuery(Guid.Parse(Id), _sprint.Id));

            if (pendingSprintResult.Succeeded)
            {
                var pendingCards = pendingSprintResult.Data.PendingCards;
                var availableSprints = pendingSprintResult.Data.AvailableSprints;

                var parameters = new DialogParameters<CompleteSprintModal>
                {
                    { x => x.BoardId, Guid.Parse(Id) },
                    { x => x.PendingCards, pendingCards },
                    { x => x.AvailableSprints, availableSprints },
                    { x => x.SprintId , _sprint.Id },
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

        private void OpenRetrospective()
        {
            if(_sprint?.Retrospective == null)
            {
                return;
            }

            var slug = StringUtilities.SlugifyString(_sprint.Retrospective.Name);
            _navigationManager.NavigateTo($"boards/{_sprint.Retrospective.BoardId}/{slug}");
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
                var cardIds = _selectedCards.Select(c => c.Id).ToList();

                var request = new MoveToBacklogCommand(Guid.Parse(Id), cardIds);

                var result = await _boardManager.MoveCardsToBacklog(request);

                if (result.Succeeded)
                {
                    await _table.ReloadServerData();
                }

                DisplayMessage(result);
            }
        }

        private void OnSearch(string text)
        {
            _searchString = string.IsNullOrEmpty(text) ? null : text;
            _table.ReloadServerData();
        }

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new GetSprintCardsQuery(Guid.Parse(SprintId))
            {
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString?.ToLower().Trim(),
                OrderBy = orderings,
                //Status = _status
            };

            var response = await _sprintManager.GetSprintCards(request);
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
