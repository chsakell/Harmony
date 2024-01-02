using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Harmony.Client.Shared.Modals
{
    public partial class EditableTextEditorFieldModal
    {
        [CascadingParameter] private MudDialogInstance MudDialog { get; set; }

        [Parameter]
        public string Text { get; set; }

        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public object? Validator { get; set; }

        [Parameter]
        public string SaveIcon { get; set; } = Icons.Material.Filled.Save;

        [Parameter]
        public string SaveText { get; set; } = "Save";

        [Parameter]
        public string Title { get; set; } = "Edit";

        [Parameter]
        public bool DisplayCancelButton { get; set; } = true;

        [Parameter]
        public string TitleIcon { get; set; } = Icons.Material.Filled.Edit;

        [Parameter]
        public Action<string> OnSave { get; set; }

        [Parameter] public string EditorCssStyle { get; set; } = "min-height:150px;";

        private void Cancel()
        {
            MudDialog.Cancel();
        }
    }
}
