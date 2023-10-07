using Harmony.Application.DTO;
using Harmony.Application.Features.Boards.Commands.Create;
using Harmony.Application.Features.Cards.Commands.CreateChecklist;
using Harmony.Application.Features.Cards.Commands.ToggleCardLabel;
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
        private List<LabelDto> _card = new();
        private bool _processing;
        private MudColorPicker colorPicker;
        private CreateLabelModel _createLabelModel = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        private MudDateRangePicker _picker;
        private DateRange _dateRange = new DateRange(DateTime.Now.Date, 
            DateTime.Now.AddDays(5).Date);

        [Parameter]
        public EditableCardModel Card { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            
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

        public IEnumerable<MudColor> HarmonyPalette { get; set; } = new MudColor[]
        {
            "#164b35", "#533f04", "#5f3811", "#601e16", "#352c63",
            "#216e4e", "#7f5f01", "#974f0c", "#ae2a19", "#5e4db2",
            "#4bce97", "#e2b203", "#faa53d", "#f87462", "#9f8fef",
            "#09326c", "#1d474c", "#37471f", "#50253f", "#454f59",
            "#0055cc", "#206b74", "#4c6b1f", "#943d73", "#596773",
            "#579dff", "#60c6d2", "#94c748", "#e774bb", "#8c9bab",
        };
    }
}
