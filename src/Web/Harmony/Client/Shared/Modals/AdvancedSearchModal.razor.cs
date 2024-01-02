using Harmony.Application.DTO;
using Harmony.Application.DTO.Search;
using Harmony.Application.Features.Search.Commands.AdvancedSearch;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class AdvancedSearchModal
    {
        private bool _searching;
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private BoardDto _selectedBoard { get; set; }
        private BoardListDto _selectedBoardList { get; set; }
        private List<BoardDto> _userBoards { get; set; } = new List<BoardDto>();

        private MudDatePicker _datePicker;
        private List<IssueTypeDto> _issueTypes = new List<IssueTypeDto>();
        private IEnumerable<SearchableCard> _cards = new List<SearchableCard>();
        private AdvancedSearchCommand _advancedSearchCommandModel = new AdvancedSearchCommand();
        private MudTable<SearchableCard> _mudTable;
        public bool FiltersDisabled => _selectedBoard == null;
        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            var userBoardsResult = await _boardManager.GetUserBoards();

            if(userBoardsResult.Succeeded)
            {
                _userBoards = userBoardsResult.Data;
            }
        }

        private async Task Search()
        {
            _searching = true;

            var result = await _searchManager.AdvancedSearch(_advancedSearchCommandModel);

            if (result.Succeeded)
            {
                _cards = result.Data;
            }

            _searching = false;
        }

        private void RowClickEvent(TableRowClickEventArgs<SearchableCard> tableRowClickEventArgs)
        {
            var card = tableRowClickEventArgs.Item;
            var slug = StringUtilities.SlugifyString(card.BoardTitle);

            MudDialog.Close();
            _navigationManager.NavigateTo($"boards/{card.BoardId}/{slug}/{card.CardId}");
        }

        private Func<IssueTypeDto, string> convertFunc = type =>
        {
            if(type.Id == Guid.Empty)
            {
                return "Select issue type";
            }

            return type.Summary;
        };

        private void SetBoard(BoardDto board)
        {
            _selectedBoard = board;

            _advancedSearchCommandModel.BoardId = board?.Id;
        }

        private void SetBoardList(BoardListDto boardList)
        {
            _selectedBoardList = boardList;

            _advancedSearchCommandModel.ListId = boardList?.Id;
        }

        private Func<BoardDto, string> boardConverterFunc = board =>
        {
            if (board == null)
            {
                return string.Empty;
            }

            return board.Title;
        };

        private Func<BoardListDto, string> boardListConverterFunc = list =>
        {
            if (list == null)
            {
                return string.Empty;
            }

            return list.Title;
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
