using Harmony.Application.DTO;
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

        public List<ChartSeries> Series = new List<ChartSeries>()
    {
        new ChartSeries() { Name = "Series 1", Data = new double[] { 90, 79, 72, 69, 62, 62, 55, 65, 70 } },
        new ChartSeries() { Name = "Series 2", Data = new double[] { 10, 41, 35, 51, 49, 62, 69, 91, 148 } },
    };
        public string[] XAxisLabels = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep" };

        Random random = new Random();
        public void RandomizeData()
        {
            var new_series = new List<ChartSeries>()
        {
            new ChartSeries() { Name = "Series 1", Data = new double[9] },
            new ChartSeries() { Name = "Series 2", Data = new double[9] },
        };

        for (int i = 0; i < 9; i++)
        {
            new_series[0].Data[i] = random.NextDouble() * 100;
            new_series[1].Data[i] = random.NextDouble() * 100;
        }
        Series = new_series;
        StateHasChanged();
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
