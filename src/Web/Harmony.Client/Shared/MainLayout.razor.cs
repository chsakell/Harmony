using Harmony.Client.Infrastructure.Settings;
using Microsoft.JSInterop;
using MudBlazor;

namespace Harmony.Client.Shared
{
    public partial class MainLayout : IDisposable
    {
        private MudTheme _currentTheme;
        private bool _blendUIEnabled {  get; set; }
        private bool _darkMode { get; set; }

        public string GetWrapperClasss()
        {
            var isDarkTheme = _currentTheme == HarmonyTheme.DarkTheme;

            return _blendUIEnabled ? "default-harmony" : (isDarkTheme ? "dark" : "default") + "-transparent ";
        }

        protected override async Task OnInitializedAsync()
        {
            _currentTheme = HarmonyTheme.DefaultTheme;
            _currentTheme = await _clientPreferenceManager.GetCurrentThemeAsync();
            _darkMode = _currentTheme == HarmonyTheme.DarkTheme;
            _blendUIEnabled = await _clientPreferenceManager.IsBlendUiEnabled();
            _interceptor.RegisterEvent();

            await SetBlendUi(_blendUIEnabled);
        }

        private async Task SetMode(bool isDark)
        {
            _darkMode = isDark;
            bool isDarkMode = await _clientPreferenceManager.SetDarkModeAsync(isDark);
            _currentTheme = isDarkMode
                ? HarmonyTheme.DarkTheme
                : HarmonyTheme.DefaultTheme;

            await _jsRuntime.InvokeVoidAsync("setBodyCssClass", GetWrapperClasss());
        }

        private async Task SetBlendUi(bool blendUiEnabled)
        {
            _blendUIEnabled = await _clientPreferenceManager.SetBlendUiAsync(blendUiEnabled);

            await _jsRuntime.InvokeVoidAsync("setBodyCssClass", GetWrapperClasss());
        }

        private void TransparentMode(bool isTransparent)
        {
            _blendUIEnabled = isTransparent;
        }

        public void Dispose()
        {
            _interceptor.DisposeEvent();
        }
    }
}
