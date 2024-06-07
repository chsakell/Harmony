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

        private IssueTypeDto _selectedIssueType;
        private IEnumerable<IssueTypeDto> _selectedIssueTypes { get; set; } = Enumerable.Empty<IssueTypeDto>();
        
        private IEnumerable<UserBoardResponse> _selectedAssignees { get; set; } = Enumerable.Empty<UserBoardResponse>();

        protected override async Task OnInitializedAsync()
        {
            _selectedIssueTypes = IssueTypes.Where(it => SelectedIssueTypeIds.Contains(it.Id));
            _selectedAssignees = BoardMembers.Where(a => SelectedAssignees.Contains(a.Id));
        }

        #region Issue Types

        private async Task SetIssueTypes(IEnumerable<IssueTypeDto> issueTypes)
        {
            _selectedIssueTypes = issueTypes;

            await SelectedIssueTypeIdsChanged.InvokeAsync(_selectedIssueTypes.Select(t => t.Id).ToList());
        }

        Func<IssueTypeDto, string> issueTypeConverter = p =>
        {
            return p?.Summary;
        };

        #endregion

        #region Assignees

        private async Task SetAssignees(IEnumerable<UserBoardResponse> assignees)
        {
            _selectedAssignees = assignees;

            await SelectedAssigneesChanged.InvokeAsync(_selectedAssignees.Select(a => a.Id).ToList());
        }

        Func<UserBoardResponse, string> assigneesConverter = p =>
        {
            return p?.UserName;
        };


        #endregion
    }
}
