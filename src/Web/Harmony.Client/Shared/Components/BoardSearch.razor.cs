using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Shared.Components
{
    public partial class BoardSearch
    {
        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter] 
        public List<IssueTypeDto> IssueTypes { get; set; }

        [Parameter] 
        public List<Guid> SelectedIssueTypeIds { get; set; }

        [Parameter]
        public EventCallback<List<Guid>> SelectedIssueTypeIdsChanged { get; set; }

        [Parameter]
        public List<UserBoardResponse> BoardMembers { get; set; }

        [Parameter]
        public EventCallback<List<string>> SelectedAssigneesChanged { get; set; }

        [Parameter]
        public List<string> SelectedAssignees { get; set; }

        [Parameter]
        public IEnumerable<BoardListDto> BoardLists { get; set; }

        [Parameter]
        public List<Guid> SelectedLists { get; set; }

        [Parameter]
        public EventCallback<List<Guid>> SelectedListsChanged { get; set; }

        private IssueTypeDto _selectedIssueType;
        
        private IEnumerable<IssueTypeDto> _selectedIssueTypes { get; set; } = Enumerable.Empty<IssueTypeDto>();
        private IEnumerable<UserBoardResponse> _selectedAssignees { get; set; } = Enumerable.Empty<UserBoardResponse>();
        private IEnumerable<BoardListDto> _selectedLists { get; set; } = Enumerable.Empty<BoardListDto>();

        protected override void OnInitialized()
        {
            _selectedIssueTypes = IssueTypes.Where(it => SelectedIssueTypeIds.Contains(it.Id));
            _selectedAssignees = BoardMembers.Where(a => SelectedAssignees.Contains(a.Id));
            _selectedLists = BoardLists.Where(it => SelectedLists.Contains(it.Id));
        }

        #region Issue Types

        private async Task SetIssueTypes()
        {
            await SelectedIssueTypeIdsChanged.InvokeAsync(_selectedIssueTypes.Select(t => t.Id).ToList());
        }

        Func<IssueTypeDto, string> issueTypeConverter = p =>
        {
            return p?.Summary;
        };

        #endregion

        #region Assignees

        private async Task SetAssignees()
        {
            await SelectedAssigneesChanged.InvokeAsync(_selectedAssignees.Select(a => a.Id).ToList());
        }

        Func<UserBoardResponse, string> assigneesConverter = p =>
        {
            return p?.UserName;
        };


        #endregion

        #region Lists

        private string GetMultiSelectionBoardListsText(List<string> selectedValues)
        {
            if (!_selectedLists.Any())
            {
                return string.Empty;
            }

            return string.Join(", ", _selectedLists.Select(x => x.Title));
        }

        private async Task SetLists()
        {
            await SelectedListsChanged.InvokeAsync(_selectedLists.Select(a => a.Id).ToList());
        }

        #endregion
    }
}
