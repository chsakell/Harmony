using Blazored.TextEditor;
using Microsoft.AspNetCore.Components;

namespace Harmony.Client.Shared.Components
{
    public partial class PostTextEditor
    {
        BlazoredTextEditor _editor;

        [Parameter]
        public string Contents { get; set; }

        public async Task<string> GetHTML()
        {
            return await _editor.GetHTML();
        }

        [Parameter] public string EditorCssStyle { get; set; }

        public async Task LoadHtml(string html)
        {
            await _editor.LoadHTMLContent(html);
        }
    }
}
