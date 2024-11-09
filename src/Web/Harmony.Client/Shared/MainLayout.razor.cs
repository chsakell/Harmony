using Harmony.Client.Infrastructure.Settings;
using Microsoft.JSInterop;
using MudBlazor;

namespace Harmony.Client.Shared
{
    public partial class MainLayout : IDisposable
    {
        private bool _blendUIEnabled {  get; set; }
        private bool _isDarkMode;
        private MudThemeProvider _mudThemeProvider;
        MudTheme _appTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                AppbarBackground = "rgba(255, 0, 0, 0)"
            },
            PaletteDark = new PaletteDark()
        };
        public string GetWrapperClasss()
        {
            return _blendUIEnabled ? "default-harmony" : (IsDarkModeEnabled ? "dark" : "default") + "-transparent ";
        }

        public bool IsDarkModeEnabled => _isDarkMode == true;

        protected override async Task OnInitializedAsync()
        {
            _blendUIEnabled = await _clientPreferenceManager.IsBlendUiEnabled();
            _interceptor.RegisterEvent();

            //_mudThemeProvider.Theme.PaletteLight.AppbarBackground = "rgba(255, 0, 0, 0)";
            await SetBlendUi(_blendUIEnabled);
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                if (_mudThemeProvider != null)
                {
                    _isDarkMode = await _mudThemeProvider.GetSystemPreference();
                    StateHasChanged();
                }
            }
        }

        private async Task SetMode(bool isDark)
        {
            _isDarkMode = isDark;
            bool isDarkMode = await _clientPreferenceManager.SetDarkModeAsync(isDark);

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
