using Harmony.Client.Infrastructure.Settings;
using MudBlazor;

namespace Harmony.Client.Shared
{
    public partial class MainLayout : IDisposable
    {
        private MudTheme _currentTheme;
        private bool _transparentBackground {  get; set; }

        public string GetWrapperClasss()
        {
            var isDarkTheme = _currentTheme == HarmonyTheme.DarkTheme;

            return _transparentBackground ?  (isDarkTheme ? "dark" : "default") + "-transparent " : "default-harmony";
        }

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

        private void TransparentMode(bool isTransparent)
        {
            _transparentBackground = isTransparent;
        }

        public void Dispose()
        {
            _interceptor.DisposeEvent();
        }
    }
}
