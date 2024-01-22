using Harmony.Application.DTO;
using Harmony.Application.Features.Cards.Commands.ToggleCardLabel;
using Harmony.Application.Features.Cards.Queries.GetLabels;
using Harmony.Application.Features.Labels.Commands.CreateCardLabel;
using Harmony.Application.Features.Labels.Commands.RemoveCardLabel;
using Harmony.Application.Features.Labels.Commands.UpdateTitle;
using Harmony.Client.Infrastructure.Models.Labels;
using Harmony.Client.Shared.Dialogs;
using Harmony.Shared.Wrapper;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using MudBlazor.Utilities;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditCardLabelsModal
    {
        private List<LabelDto> _cardLabels = new();
        private bool _processing;
        private bool _loading;

        private MudColorPicker colorPicker;
        private CreateLabelModel _createLabelModel = new();
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public Guid CardId { get; set; }

        [Parameter]
        public Guid BoardId { get; set; }

        private void Cancel()
        {
            MudDialog.Cancel();
        }

        protected override async Task OnInitializedAsync()
        {
            _loading = true;

            var cardLabelsResponse = await _cardManager
                .GetCardLabelsAsync(new GetCardLabelsQuery(CardId));

            if(cardLabelsResponse.Succeeded)
            {
                _cardLabels = cardLabelsResponse.Data;
            }

            _loading = false;
        }

        private async Task DeleteLabel(LabelDto label)
        {
            var parameters = new DialogParameters<Confirmation>
            {
                { x => x.ContentText, $"Are you sure you want to delete this label? " +
                    $"Label will be removed from all cards in the board" },
                { x => x.ButtonText, "Yes" },
                { x => x.Color, Color.Error }
            };

            var dialog = _dialogService.Show<Confirmation>("Confirm", parameters);
            var dialogResult = await dialog.Result;

            if (!dialogResult.Canceled)
            {
                var result = await _labelManager
                    .RemoveCardLabel(new RemoveCardLabelCommand(label.Id));

                if (result.Succeeded && result.Data)
                {
                    _cardLabels.Remove(label);
                }

                DisplayMessage(result);
            }
        }

        private async Task CreateNewLabel()
        {
            Guid? cardId = _createLabelModel.IsChecked ? CardId : null;
            var result = await _labelManager
                .CreateCardLabel(new CreateCardLabelCommand(BoardId, cardId,
                _createLabelModel.Color, _createLabelModel.Title));

            if(result.Succeeded)
            {
                _cardLabels.Add(new LabelDto()
                {
                    Id = result.Data.LabelId,
                    Colour = result.Data.Color,
                    IsChecked = result.Data.CardId.HasValue,
                    Title = result.Data.Title
                });
            }

            DisplayMessage(result);

            _createLabelModel.Color = null;
            _createLabelModel.Title = null;
            _createLabelModel.IsChecked = false;
        }

        private async Task UpdateLabelTitle(LabelDto label, string title)
        {
            var result = await _labelManager
                .UpdateLabelTitle(new UpdateLabelTitleCommand(label.Id, title));

            if(result.Succeeded)
            {
                label.Title = title;
            }

            DisplayMessage(result);
        }

        private async Task ToggleCardLabel(LabelDto label, bool isChecked)
        {
            var result = await _cardManager.ToggleCardLabel(new ToggleCardLabelCommand(CardId, label.Id)
            {
                BoardId = BoardId
            });

            if(result.Succeeded)
            {
                label.IsChecked = isChecked;
            }
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
