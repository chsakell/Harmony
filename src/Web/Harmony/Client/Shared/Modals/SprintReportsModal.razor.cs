using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Shared.Utilities;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class SprintReportsModal
    {
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }
        private bool _loading = true;
        private readonly ChartOptions _options = new();

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public Guid SprintId { get; set; }

        private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.

        public List<ChartSeries> BurnDownSeries = new List<ChartSeries>();

        public string[] XAxisBurnDownLabels;

        private GetSprintReportsResponse SprintReports { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var sprintReportsResult = await _sprintManager.GetSprintReports(SprintId);

            if(sprintReportsResult.Succeeded)
            {
                SprintReports = sprintReportsResult.Data;
                _options.YAxisTicks = 10;
                
                if (SprintReports?.BurnDownReport != null)
                {
                    BurnDownSeries.Add(new ChartSeries()
                    {
                        Name = "Guide Line",
                        Data = SprintReports.BurnDownReport.GuideLineStoryPoints.ToArray()
                    });

                    BurnDownSeries.Add(new ChartSeries()
                    {
                        Name = "Remaining Story Points",
                        Data = SprintReports.BurnDownReport.RemainingStoryPoints.ToArray()
                    });

                    XAxisBurnDownLabels = SprintReports.BurnDownReport.Dates.ToArray();
                }
            }

            _loading = false;
        }

        private void ViewBacklog()
        {
            MudDialog.Cancel();

            _navigationManager.NavigateTo($"/projects/{BoardId}/backlog");
        }

        private void ViewBoard()
        {
            MudDialog.Cancel();

            var board = _workspaceManager?.UserWorkspaces?.SelectMany(w => w.Boards)?.FirstOrDefault(b => b.Id == BoardId);

            if(board == null)
            {
                return;
            }

            var slug = StringUtilities.SlugifyString(board.Title.ToString());

            _navigationManager.NavigateTo($"boards/{BoardId}/{slug}");
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
