using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.ToggleCardLabel;
using Harmony.Application.Features.Cards.Commands.UpdateCardDates;
using Harmony.Application.Features.Cards.Queries.GetLabels;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Application.Features.Workspaces.Queries.GetAllForUser;
using Harmony.Client.Infrastructure.Models.Board;
using Harmony.Client.Infrastructure.Models.Labels;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

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
        public DateTime? StartDate { get; set; }

        [Parameter]
        public DateTime? DueDate { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
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
