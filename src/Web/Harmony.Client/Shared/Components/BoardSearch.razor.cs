using Harmony.Application.DTO;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Shared.Components
{
    public partial class BoardSearch
    {
        [Parameter] 
        public List<IssueTypeDto> IssueTypes { get; set; }

        [Parameter] 
        public List<Guid> SelectedIssueTypeIds { get; set; }

        [Parameter]
        public EventCallback<List<Guid>> SelectedIssueTypeIdsChanged { get; set; }    

        private IssueTypeDto _selectedIssueType;
        private IEnumerable<IssueTypeDto> _selectedIssueTypes { get; set; } = Enumerable.Empty<IssueTypeDto>();

        protected override void OnInitialized()
        {
            _selectedIssueTypes = IssueTypes.Where(it => SelectedIssueTypeIds.Contains(it.Id));
        }

        private async Task SetFromIssueTypes(IEnumerable<IssueTypeDto> issueTypes)
        {
            _selectedIssueTypes = issueTypes;

            await SelectedIssueTypeIdsChanged.InvokeAsync(_selectedIssueTypes.Select(t => t.Id).ToList());
        }

        Func<IssueTypeDto, string> issueTypeConverter = p =>
        {
            return p?.Summary;
        };
    }
}
