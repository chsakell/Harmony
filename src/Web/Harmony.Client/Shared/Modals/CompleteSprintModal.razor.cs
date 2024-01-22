using Harmony.Application.DTO;
using Harmony.Application.Features.Sprints.Commands.CompleteSprint;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class CompleteSprintModal
    {
        private bool _processing;
        private static Guid BackLogSprint = Guid.Parse("691dab90-563e-441d-963d-e75f4c4d7c99");
        private static Guid NewSprint = Guid.Parse("7337d664-c1de-4a1b-81f1-f45ab330da0c");

        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        [Parameter]
        public Guid SprintId { get; set; }

        [Parameter]
        public List<CardDto> PendingCards { get; set; }

        [Parameter]
        public List<SprintDto> AvailableSprints { get; set; }

        private SprintDto _moveToSprint;
        private SprintDto _backLogSprint = new SprintDto() { Id = BackLogSprint, Name = "Backlog" };
        private SprintDto _newSprint = new SprintDto() { Id = NewSprint,  Name = "New sprint" };

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        private async Task CompleteSprint()
        {
            _processing = true;

            Guid? moveToSprintId = null;

            var moveToBacklog = _moveToSprint.Id == BackLogSprint;
            var createNewSprint = _moveToSprint.Id == NewSprint;
            if(!moveToBacklog && !createNewSprint)
            {
                moveToSprintId = _moveToSprint.Id;
            }

            var result = await _sprintManager
                .CompleteSprint(new CompleteSprintCommand(BoardId, SprintId)
                {
                    MoveToBacklog = moveToBacklog,
                    CreateNewSprint = createNewSprint,
                    MoveToSprintId = moveToSprintId
                });

            DisplayMessage(result);

            _processing = false;

            if (result.Succeeded)
            {
                MudDialog.Close(result.Data);
            }
        }

        Func<SprintDto, string> converter = p =>
        {
            return p?.Name ?? "Move pending issues to";
        };

        private void DisplayMessage(IResult result)
        {
            if (result == null)
            {
                return;
            }

            var severity = result.Succeeded ? Severity.Success : Severity.Error;

            foreach (var message in result.Messages)
            {
               // _snackBar.Add(message, severity);
            }
        }
    }
}
