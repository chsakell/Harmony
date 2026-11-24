using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Polly.Registry;
using RabbitMQ.Client;
using System.Text;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Messaging
{
    public class RabbitMQNotificationPublisher : INotificationsPublisher, IAsyncDisposable
    {
        private readonly ILogger<RabbitMQNotificationPublisher> _logger;
        private readonly BrokerConfiguration? _brokerConfiguration;
        private readonly Task _initialization;
        private IConnection? _connection;

        public RabbitMQNotificationPublisher(IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            ILogger<RabbitMQNotificationPublisher> logger)
        {
            _brokerConfiguration = brokerConfig.Value;
            _logger = logger;

            // RabbitMQ.Client 7 is async-only: connect in the background instead of blocking the constructor.
            // Publishers await this task, so nothing is published before the connection attempt completes.
            _initialization = InitializeAsync(resiliencePipelineProvider);
        }

        private async Task InitializeAsync(ResiliencePipelineProvider<string> resiliencePipelineProvider)
        {
            try
            {
                var pipeline = resiliencePipelineProvider.GetPipeline(HarmonyRetryPolicy.WaitAndRetry);

                await pipeline.ExecuteAsync(async token => await InitRabbitMQAsync(token));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {ex}");
            }
        }

        private async Task InitRabbitMQAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Trying to connect to {_brokerConfiguration.Host}:{_brokerConfiguration.Port} ");

            var factory = new ConnectionFactory
            {
                HostName = _brokerConfiguration.Host,
                Port = _brokerConfiguration.Port,
                UserName = string.IsNullOrEmpty(_brokerConfiguration.Username)
                    ? ConnectionFactory.DefaultUser : _brokerConfiguration.Username,
                Password = string.IsNullOrEmpty(_brokerConfiguration.Password)
                    ? ConnectionFactory.DefaultPass : _brokerConfiguration.Password,
                VirtualHost = string.IsNullOrEmpty(_brokerConfiguration.VirtualHost) ?
                    ConnectionFactory.DefaultVHost : _brokerConfiguration.VirtualHost,
            };

            try
            {
                _connection = await factory.CreateConnectionAsync(cancellationToken);

                await using var channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

                await channel.ExchangeDeclareAsync(exchange: BrokerConstants.NotificationsExchange, type: ExchangeType.Topic,
                    cancellationToken: cancellationToken);

                await channel.QueueDeclareAsync(
                    queue: BrokerConstants.EmailNotificationsQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                await channel.QueueDeclareAsync(
                    queue: BrokerConstants.SearchIndexNotificationsQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null,
                    cancellationToken: cancellationToken);

                _logger.LogInformation($"Connected to {_brokerConfiguration.Host}:{_brokerConfiguration.Port}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {_brokerConfiguration.Host}:{_brokerConfiguration.Port} {ex}");
            }
        }

        public Task PublishMessageAsync<T>(T message, NotificationType type, string routingKey)
        {
            return PublishAsync(message,
                exchange: BrokerConstants.NotificationsExchange,
                routingKey: routingKey,
                headers: new Dictionary<string, object?>
                {
                    { BrokerConstants.NotificationHeader, type.ToString() }
                });
        }

        public Task PublishEmailNotificationAsync<T>(T notification) where T : BaseEmailNotification
        {
            return PublishAsync(notification,
                exchange: "",
                routingKey: BrokerConstants.EmailNotificationsQueue,
                headers: new Dictionary<string, object?>
                {
                    { BrokerConstants.NotificationHeader, notification.Type.ToString() }
                });
        }

        public Task PublishSearchIndexNotificationAsync<T>(T notification, string index) where T : BaseSearchIndexNotification
        {
            return PublishAsync(notification,
                exchange: "",
                routingKey: BrokerConstants.SearchIndexNotificationsQueue,
                headers: new Dictionary<string, object?>
                {
                    { BrokerConstants.NotificationHeader, notification.Type.ToString() },
                    { BrokerConstants.IndexNameHeader, index }
                });
        }

        private async Task PublishAsync<T>(T payload, string exchange, string routingKey, IDictionary<string, object?> headers)
        {
            await _initialization;

            if (_connection == null || !_connection.IsOpen)
            {
                return;
            }

            await using var channel = await _connection.CreateChannelAsync();

            var json = JsonConvert.SerializeObject(payload);
            var body = Encoding.UTF8.GetBytes(json);

            var props = new BasicProperties
            {
                ContentType = "text/plain",
                DeliveryMode = DeliveryModes.Persistent,
                Headers = headers
            };

            await channel.BasicPublishAsync(
                exchange: exchange,
                routingKey: routingKey,
                mandatory: false,
                basicProperties: props,
                body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }
    }
}
