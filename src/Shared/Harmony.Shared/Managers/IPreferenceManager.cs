using Harmony.Shared.Settings;
using Harmony.Shared.Wrapper;

namespace Harmony.Shared.Managers
{
    /// <summary>
    /// Preference manager for client settings
    /// </summary>
    public interface IPreferenceManager
    {
        Task SetPreference(IPreference preference);

        Task<IPreference> GetPreference();

        Task<IResult> ChangeLanguageAsync(string languageCode);
    }
}