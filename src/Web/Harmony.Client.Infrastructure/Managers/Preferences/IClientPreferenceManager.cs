using Harmony.Shared.Managers;
using MudBlazor;

namespace Harmony.Client.Infrastructure.Managers.Preferences
{
    public interface IClientPreferenceManager : IPreferenceManager
    {
        Task<MudTheme> GetCurrentThemeAsync();
        Task<bool> IsBlendUiEnabled();
        Task<bool> ToggleDarkModeAsync();
        Task<bool> SetDarkModeAsync(bool isDark);
        Task<bool> SetBlendUiAsync(bool blendUiEnabled);
        Task<string> GetSelectedWorkspace();
        Task<bool> SetSelectedWorkspace(Guid workspaceId);
        Task<bool> ClearSelectedWorkspace(Guid workspaceId);
        Task<bool> ClearSelectedWorkspace();
    }
}