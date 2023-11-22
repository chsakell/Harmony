using AutoMapper.Execution;
using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.CreateSprint;
using Harmony.Application.Features.Boards.Queries.GetBacklog;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Cards.Commands.CreateBacklog;
using Harmony.Application.Features.Cards.Commands.MoveToBacklog;
using Harmony.Application.Features.Lists.Commands.CreateList;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Application.Features.Users.Commands.UploadProfilePicture;
using Harmony.Application.Features.Workspaces.Queries.GetBacklog;
using Harmony.Application.Features.Workspaces.Queries.GetSprints;
using Harmony.Application.Features.Workspaces.Queries.GetWorkspaceUsers;
using Harmony.Client.Infrastructure.Store.Kanban;
using Harmony.Client.Shared.Dialogs;
using Harmony.Client.Shared.Modals;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static Harmony.Shared.Constants.Permission.Permissions;
using static MudBlazor.CategoryTypes;

namespace Harmony.Client.Pages.Management
{
    public partial class Sprints
    {
        [Parameter]
        public string Id { get; set; }

        private string _searchString = "";
        private int _totalItems;
        private List<GetSprintCardResponse> _cards = new List<GetSprintCardResponse>();
        private MudTable<GetSprintCardResponse> _table;
        private HashSet<GetSprintCardResponse> _selectedCards = new HashSet<GetSprintCardResponse>();

        private TableGroupDefinition<GetSprintCardResponse> _groupDefinition = new()
        {
            GroupName = "Sprint",
            Indentation = false,
            Expandable = true,
            IsInitiallyExpanded = true,
            Selector = (e) => e.Sprint + $" [{e.SprintStatus}]"
        };

        protected override Task OnInitializedAsync()
        {
            return base.OnInitializedAsync();
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
            }

            //var parameters = new DialogParameters<Confirmation>
            //{
            //    { x => x.ContentText, $"Are you sure you want to complete {sprintName}? " +
            //    $"All of it's cards will be available on the board" },
            //    { x => x.ButtonText, "Yes" },
            //    { x => x.Color, Color.Warning }
            //};

            //var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            //var dialogResult = await dialog.Result;

            //if (!dialogResult.Canceled)
            //{
            //    var request = new UploadProfilePictureCommand
            //    {
            //        Data = new byte[0],
            //        Type = Domain.Enums.AttachmentType.ProfilePicture
            //    };

            //    var result = await _fileManager.UploadProfilePicture(request);

            //    if (result.Succeeded)
            //    {
            //        _user.ProfilePicture = result.Data.ProfilePicture;
            //    }

            //    DisplayMessage(result);
            //}
        }

        private async Task CreateSprint()
        {
            var parameters = new DialogParameters<CreateSprintModal>
            {
                {
                    modal => modal.CreateSprintCommandModel,
                    new CreateSprintCommand(Guid.Parse(Id))
                }
            };

            var options = new DialogOptions { CloseButton = true, MaxWidth = MaxWidth.Medium, FullWidth = true, DisableBackdropClick = true };
            var dialog = _dialogService.Show<CreateSprintModal>(_localizer["Create sprint"], parameters, options);
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
                OrderBy = orderings
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
    }
}
