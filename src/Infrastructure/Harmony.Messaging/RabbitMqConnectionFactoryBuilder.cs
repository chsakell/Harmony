using Harmony.Application.Configurations;
using RabbitMQ.Client;

namespace Harmony.Messaging
{
    /// <summary>
    /// Single place that turns <see cref="BrokerConfiguration"/> into a RabbitMQ <see cref="ConnectionFactory"/>.
    /// The same configuration shape (BrokerConfiguration__Host/Port/Username/Password/VirtualHost) is supplied by
    /// appsettings, docker-compose, Kubernetes and the .NET Aspire AppHost.
    /// </summary>
    /// <remarks>
    /// RabbitMQ.Client 7 emits OpenTelemetry activities ("RabbitMQ.Client.Publisher" / "RabbitMQ.Client.Subscriber")
    /// and propagates the trace context through message headers, so publish → consume shows up as one distributed trace.
    /// </remarks>
    public static class RabbitMqConnectionFactoryBuilder
    {
        public static ConnectionFactory Create(BrokerConfiguration configuration, bool automaticRecovery = false)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            return new ConnectionFactory
            {
                HostName = string.IsNullOrEmpty(configuration.Host) ? "localhost" : configuration.Host,
                Port = configuration.Port > 0 ? configuration.Port : AmqpTcpEndpoint.UseDefaultPort,
                AutomaticRecoveryEnabled = automaticRecovery,
                UserName = string.IsNullOrEmpty(configuration.Username)
                    ? ConnectionFactory.DefaultUser : configuration.Username,
                Password = string.IsNullOrEmpty(configuration.Password)
                    ? ConnectionFactory.DefaultPass : configuration.Password,
                VirtualHost = string.IsNullOrEmpty(configuration.VirtualHost)
                    ? ConnectionFactory.DefaultVHost : configuration.VirtualHost,
            };
        }
    }
}
