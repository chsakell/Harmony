using Harmony.Application.Features.Cards.Commands.UpdateCardDates;
using Harmony.Client.Infrastructure.Models.Labels;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardDatesModal
    {
        private bool _processing;
        private MudColorPicker colorPicker;
        private CreateLabelModel _createLabelModel = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private MudDateRangePicker _picker;
        private DateRange _dateRange = new();

        [Parameter]
        public int CardId { get; set; }

        [Parameter]
        public DateTime? StartDate { get; set; }

        [Parameter]
        public DateTime? DueDate { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Clear()
        {
            var result = await _cardManager
                .UpdateDatesAsync(new UpdateCardDatesCommand(CardId, null, null));

            if(result.Succeeded)
            {
                _dateRange.Start = null;
                _dateRange.End = null;
            }

            DisplayMessage(result);
        }

        protected override async Task OnInitializedAsync()
        {
            _dateRange = new DateRange(StartDate, DueDate);
        }


        private async Task SaveDates()
        {
            _processing = true;

            var startDate = _dateRange.Start == _dateRange.End ? null : _dateRange.Start;
            var result = await _cardManager
                .UpdateDatesAsync(new UpdateCardDatesCommand(CardId, startDate, _dateRange.End));

            DisplayMessage(result);

            _processing = false;

            MudDialog.Close();
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
