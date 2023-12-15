using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Queries.GetSprints;
using Harmony.Application.Features.Sprints.Queries.GetSprintReports;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class SprintReportsModal
    {
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public Guid SprintId { get; set; }

        private int Index = -1; //default value cannot be 0 -> first selectedindex is 0.

        public List<ChartSeries> BurnDownSeries = new List<ChartSeries>();
        //{
        //    new ChartSeries() { Name = "Series 1", Data = new double[] { 90, 79, 72, 69, 62, 62, 55, 65, 70 } },
        //    new ChartSeries() { Name = "Series 2", Data = new double[] { 10, 41, 35, 51, 49, 62, 69, 91, 148 } },
        //};
        public string[] XAxisBurnDownLabels;

        private GetSprintReportsResponse SprintReports { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var sprintReportsResult = await _sprintManager.GetSprintReports(SprintId);

            if(sprintReportsResult.Succeeded)
            {
                SprintReports = sprintReportsResult.Data;

                if(SprintReports.BurnDownReport != null)
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
        }

        private void Cancel()
        {
            MudDialog.Cancel();
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
