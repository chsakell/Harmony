namespace Harmony.Application.Configurations
{
    /// <summary>
    /// Broker configuration
    /// </summary>
public class BrokerConfiguration
{
    public string Host { get; set; }
    public int Port { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string VirtualHost { get; set; }
}
}