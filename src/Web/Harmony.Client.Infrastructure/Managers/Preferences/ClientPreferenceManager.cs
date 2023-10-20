using Blazored.LocalStorage;
using MudBlazor;

using Microsoft.Extensions.Localization;
using Harmony.Client.Infrastructure.Settings;
using Harmony.Shared.Wrapper;
using Harmony.Shared.Settings;
using Harmony.Shared.Storage;

namespace Harmony.Client.Infrastructure.Managers.Preferences
{
    public class ClientPreferenceManager : IClientPreferenceManager
    {
        private readonly ILocalStorageService _localStorageService;
        private readonly IStringLocalizer<ClientPreferenceManager> _localizer;

        public ClientPreferenceManager(
            ILocalStorageService localStorageService,
            IStringLocalizer<ClientPreferenceManager> localizer)
        {
            _localStorageService = localStorageService;
            _localizer = localizer;
        }

        public async Task<bool> ToggleDarkModeAsync()
        {
            var preference = await GetPreference() as ClientPreference;
            if (preference != null)
            {
                preference.IsDarkMode = !preference.IsDarkMode;
                await SetPreference(preference);
                return !preference.IsDarkMode;
            }

            return false;
        }

        public async Task<IResult> ChangeLanguageAsync(string languageCode)
        {
            var preference = await GetPreference() as ClientPreference;
            if (preference != null)
            {
                preference.LanguageCode = languageCode;
                await SetPreference(preference);
                return new Result
                {
                    Succeeded = true,
                    Messages = new List<string> { _localizer["Client Language has been changed"] }
                };
            }

            return new Result
            {
                Succeeded = false,
                Messages = new List<string> { _localizer["Failed to get client preferences"] }
            };
        }

        public async Task<MudTheme> GetCurrentThemeAsync()
        {
            var preference = await GetPreference() as ClientPreference;
            if (preference != null)
            {
                if (preference.IsDarkMode == true) return HarmonyTheme.DarkTheme;
            }
            return HarmonyTheme.DefaultTheme;
        }

        public async Task<IPreference> GetPreference()
        {
            return await _localStorageService.GetItemAsync<ClientPreference>(StorageConstants.Local.Preference) ?? new ClientPreference();
        }

        public async Task SetPreference(IPreference preference)
        {
            await _localStorageService.SetItemAsync(StorageConstants.Local.Preference, preference as ClientPreference);
        }

        public async Task<string> GetSelectedWorkspace()
        {
            var preference = await GetPreference() as ClientPreference;
            if (preference != null)
            {
                return preference.Workspace;
            }

            return null;
        }

        public async Task<bool> SetSelectedWorkspace(Guid workspaceId)
        {
            var preference = await GetPreference() as ClientPreference;
            if (preference != null)
            {
                preference.Workspace = workspaceId.ToString();
                await SetPreference(preference);
                return true;
            }

            return false;
        }
    }
}