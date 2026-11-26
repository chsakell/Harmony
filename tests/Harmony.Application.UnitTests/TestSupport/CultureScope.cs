using System.Globalization;

namespace Harmony.Application.UnitTests.TestSupport
{
    /// <summary>
    /// Sets the current culture and UI culture for the lifetime of the scope, then restores them.
    /// Culture flows with the execution context, so tests running in parallel are not affected.
    /// </summary>
    /// <example><code>using var _ = new CultureScope(CultureInfo.InvariantCulture);</code></example>
    public sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _originalCulture;
        private readonly CultureInfo _originalUICulture;

        public CultureScope(CultureInfo culture)
        {
            _originalCulture = CultureInfo.CurrentCulture;
            _originalUICulture = CultureInfo.CurrentUICulture;

            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _originalCulture;
            CultureInfo.CurrentUICulture = _originalUICulture;
        }
    }
}
