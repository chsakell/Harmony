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
        private readonly ChartOptions _options = new()
        {
            ChartPalette = new[] { "#5F9DF7", "#C21292", "#711DB0", "#EF4040", "FFA732"}
        };

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public Guid SprintId { get; set; }

        private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.

        public List<ChartSeries> BurnDownSeries = new List<ChartSeries>();

        public string[] XAxisBurnDownLabels;

        private GetSprintReportsResponse Reports { get; set; }

        public double[] data = { 25, 45, 65 };
        public string[] labels = { "Oil", "Gas", "Water" };

        protected override async Task OnInitializedAsync()
        {
            var sprintReportsResult = await _sprintManager.GetSprintReports(SprintId);

            if(sprintReportsResult.Succeeded)
            {
                Reports = sprintReportsResult.Data;
                _options.YAxisTicks = 10;
                
                if (Reports?.BurnDownReport != null)
                {
                    BurnDownSeries.Add(new ChartSeries()
                    {
                        Name = "Guide Line",
                        Data = Reports.BurnDownReport.GuideLineStoryPoints.ToArray()
                    });

                    BurnDownSeries.Add(new ChartSeries()
                    {
                        Name = "Remaining Story Points",
                        Data = Reports.BurnDownReport.RemainingStoryPoints.ToArray()
                    });

                    XAxisBurnDownLabels = Reports.BurnDownReport.Dates.ToArray();
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
