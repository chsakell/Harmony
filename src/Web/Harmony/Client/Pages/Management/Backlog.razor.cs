using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Cards.Commands.MoveCard;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Pages.Management
{
    public partial class Backlog
    {
        [Parameter]
        public string Id { get; set; }

        private string _searchString = "";
        private int _totalItems;
        private List<GetBacklogItemResponse> _cards;
        private MudTable<GetBacklogItemResponse> _table;
        private HashSet<GetBacklogItemResponse> _selectedCards = new HashSet<GetBacklogItemResponse>();
        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
        }

        private async Task MoveToSprint()
        {
            var parameters = new DialogParameters<MoveToSprintModal>
            {
                {
                    modal => modal.BoardId,Guid.Parse(Id)
                },
                {
                    modal => modal.Items, _selectedCards
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<MoveToSprintModal>(_localizer["Move to sprint"], parameters, options);
            var result = await dialog.Result;

            if (!result.Canceled)
            {
                await _table.ReloadServerData();
            }
        }

        private async Task Move(GetBacklogItemResponse card, int offset)
        {
            var currentCardPosition = card.Position;

            var result = await _cardManager
                .MoveCardAsync(new MoveCardCommand(card.Id, null,
                        (short)(currentCardPosition + offset), Domain.Enums.CardStatus.Backlog));

            if (result.Succeeded)
            {
                await _table.ReloadServerData();
            }

            DisplayMessage(result);
        }

        private async Task<TableData<GetBacklogItemResponse>> ReloadData(TableState state)
        {
            if (!string.IsNullOrWhiteSpace(_searchString))
            {
                state.Page = 0;
            }
            await LoadData(state.Page, state.PageSize, state);

            return new TableData<GetBacklogItemResponse>
            {
                TotalItems = _totalItems,
                Items = _cards
            };
        }

        private async Task CreateIssue()
        {
            var parameters = new DialogParameters<CreateBacklogModal>
            {
                {
                    modal => modal.CreateBacklogCommandModel,
                    new CreateBacklogCommand(null, Guid.Parse(Id))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateBacklogModal>(_localizer["Create backlog item"], parameters, options);
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

            var request = new GetBacklogQuery(Guid.Parse(Id))
            {
                BoardId = Guid.Parse(Id),
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString,
                OrderBy = orderings
            };

            var response = await _boardManager.GetBacklog(request);
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
    }
}
