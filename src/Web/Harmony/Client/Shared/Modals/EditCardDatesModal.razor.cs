using Harmony.Application.Features.Cards.Commands.UpdateCardDates;
using Harmony.Client.Infrastructure.Models.Labels;
using Harmony.Domain.Enums;
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
        public Guid CardId { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public DateTime? StartDate { get; set; }

        [Parameter]
        public DateTime? DueDate { get; set; }

        [Parameter]
        public TimeSpan? DueTime { get; set; }

        [Parameter]
        public DueDateReminderType DueDateReminder { get; set; } = DueDateReminderType.None;

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task Clear()
        {
            var result = await _cardManager
                .UpdateDatesAsync(new UpdateCardDatesCommand(CardId, null, null, null)
                {
                    BoardId = BoardId
                });

            if(result.Succeeded)
            {
                _dateRange.Start = null;
                _dateRange.End = null;
            }

            DisplayMessage(result);
        }

        protected override void OnInitialized()
        {
            _dateRange = new DateRange(StartDate, DueDate);
        }


        private async Task SaveDates()
        {
            _processing = true;

            var startDate = _dateRange.Start == _dateRange.End ? null : _dateRange.Start;

            var dueDate = _dateRange.End;

            if(dueDate.HasValue && DueTime.HasValue)
            {
                dueDate = dueDate.Value.Add(DueTime.Value);
            }

            var result = await _cardManager
                .UpdateDatesAsync(new UpdateCardDatesCommand(CardId, startDate, dueDate, DueDateReminder)
                {
                    BoardId= BoardId
                });

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

        Func<DueDateReminderType, string> converter = type =>
        {
            switch (type)
            {
                case DueDateReminderType.None:
                    return "None";
                case DueDateReminderType.AtDueDate:
                    return "At time of due date";
                case DueDateReminderType.FiveMinutesBefore:
                    return "5 Minutes before";
                case DueDateReminderType.TenMinutesBefore:
                    return "10 minutes before";
                case DueDateReminderType.FifteenMinutesBefore:
                    return "15 minutes before";
                case DueDateReminderType.OneHourBefore:
                    return "1 hour before";
                case DueDateReminderType.TwoHoursBefore:
                    return "2 hours before";
                case DueDateReminderType.OneDayBefore:
                    return "1 day before";
                case DueDateReminderType.TwoDaysBefore:
                    return "2 days before";
                default:
                    return "None";
            }
        };
    }
}
