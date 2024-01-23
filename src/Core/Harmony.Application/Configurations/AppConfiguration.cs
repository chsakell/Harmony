namespace Harmony.Application.Configurations
{
    /// <summary>
    /// Authentication configuration
    /// </summary>
    public class AppConfiguration
    {
        public string Secret { get; set; }
        public bool BehindSSLProxy { get; set; }
        public string ProxyIP { get; set; }
        public string ApplicationUrl { get; set; }
    }
}