using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Features.Cards.Commands.CreateLink;
using Harmony.Application.Features.Cards.Queries.SearchCards;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class AddLinkIssueModal
    {
        private bool _processing;

        private List<CardDto> _cards;
        private MudTable<CardDto> _table;
        private string _searchString = "";
        private int _totalItems;

        private int selectedRowNumber = -1;
        private CardDto _selectedCard;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public CreateLinkCommand CreateLinkCommandModel { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task SubmitAsync()
        {
            _processing = true;

            var result = await _cardManager
                .CreateLink(CreateLinkCommandModel);

            MudDialog.Close(result.Data);

            if (!result.Succeeded)
            {
                DisplayMessage(result);
            }

            _processing = false;
        }

        private async Task<TableData<CardDto>> ReloadData(TableState state, CancellationToken token)
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

        private async Task LoadData(int pageNumber, int pageSize, TableState state)
        {
            string[] orderings = null;
            if (!string.IsNullOrEmpty(state.SortLabel))
            {
                orderings = state.SortDirection != SortDirection.None ? new[] { $"{state.SortLabel} {state.SortDirection}" } : new[] { $"{state.SortLabel}" };
            }

            var request = new SearchCardsQuery(CreateLinkCommandModel.BoardId)
            {
                PageSize = pageSize,
                PageNumber = pageNumber + 1,
                SearchTerm = _searchString?.ToLower().Trim(),
                OrderBy = orderings,
                SkipCardId = CreateLinkCommandModel.SourceCardId
                //Status = _status
            };

            var response = await _cardManager.Search(request);
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

        private void SelectCard(CardDto card)
        {
            _selectedCard = card;

            if(_selectedCard != null)
            {
                CreateLinkCommandModel.TargetCardId = _selectedCard.Id;
            }
            else
            {
                CreateLinkCommandModel.TargetCardId = null;
            }
        }

        private string SelectedRowClassFunc(CardDto card, int rowNumber)
        {
            if (selectedRowNumber == rowNumber)
            {
                selectedRowNumber = -1;

                //_table.SetSelectedItem(null);

                return string.Empty;
            }
            else if (_table.SelectedItem != null && _table.SelectedItem.Equals(card))
            {
                selectedRowNumber = rowNumber;

                return "mud-harmony";
            }
            else
            {
                return string.Empty;
            }
        }

        private void OnSearch(string text)
        {
            _searchString = string.IsNullOrEmpty(text) ? null : text;
            _table.ReloadServerData();
        }

        Func<LinkType, string> converter = type =>
        {
            return type.GetDescription();
        };

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
