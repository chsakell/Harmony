using Slugify;

namespace Harmony.Shared.Utilities
{
    /// <summary>
    /// Utilities used with strings
    /// </summary>
    public static class StringUtilities
    {
        private static readonly SlugHelper _slugHelper;
        static StringUtilities()
        {
            var config = new SlugHelperConfiguration();
            config.StringReplacements.Add("&", "-");
            config.StringReplacements.Add(",", "-");
            config.StringReplacements.Add(".", "-");

            _slugHelper = new SlugHelper(config);
        }

        /// <summary>
        /// Returns a valid slug to be used in URLs
        /// </summary>
        /// <param name="original"></param>
        /// <returns></returns>
        public static string SlugifyString(string original)
        {
            return _slugHelper.GenerateSlug(original);
        }
    }
}
