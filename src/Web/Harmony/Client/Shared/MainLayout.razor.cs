using Harmony.Client.Infrastructure.Settings;
using MudBlazor;

namespace Harmony.Client.Shared
{
    public partial class MainLayout : IDisposable
    {
        private MudTheme _currentTheme;

        protected override async Task OnInitializedAsync()
        {
            _currentTheme = HarmonyTheme.DefaultTheme;
            _currentTheme = await _clientPreferenceManager.GetCurrentThemeAsync();
            _interceptor.RegisterEvent();
        }

        private async Task DarkMode()
        {
            bool isDarkMode = await _clientPreferenceManager.ToggleDarkModeAsync();
            _currentTheme = isDarkMode
                ? HarmonyTheme.DefaultTheme
                : HarmonyTheme.DarkTheme;
        }

        public void Dispose()
        {
            _interceptor.DisposeEvent();
        }
    }
}
