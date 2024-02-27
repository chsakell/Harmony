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
    public class RabbitMQNotificationPublisher : INotificationsPublisher
    {
        private readonly ILogger<RabbitMQNotificationPublisher> _logger;
        IConnection connection;
        private readonly BrokerConfiguration? _brokerConfiguration;
        public RabbitMQNotificationPublisher(IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            ILogger<RabbitMQNotificationPublisher> logger)
        {
            _brokerConfiguration = brokerConfig.Value;
            _logger = logger;

            try
            {
                var pipeline = resiliencePipelineProvider.GetPipeline(HarmonyRetryPolicy.WaitAndRetry);

                pipeline.Execute(token =>
                {
                    InitRabbitMQ();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {ex}");
            }
        }

        private void InitRabbitMQ()
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
                connection = factory.CreateConnection();

                using var channel = connection.CreateModel();

                channel.ExchangeDeclare(exchange: BrokerConstants.NotificationsExchange, type: ExchangeType.Topic);

                channel.QueueDeclare(
                    queue: BrokerConstants.EmailNotificationsQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                channel.QueueDeclare(
                    queue: BrokerConstants.SearchIndexNotificationsQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                _logger.LogInformation($"Connected to {_brokerConfiguration.Host}:{_brokerConfiguration.Port}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {_brokerConfiguration.Host}:{_brokerConfiguration.Port} {ex}");
            }
        }

        public void PublishMessage<T>(T message, NotificationType type, string routingKey)
        {
            if (connection == null || !connection.IsOpen)
            {
                return;
            }

            using var channel = connection.CreateModel();

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2;
            props.Headers = new Dictionary<string, object>
            {
                { BrokerConstants.NotificationHeader, type.ToString() }
            };

            channel.BasicPublish(
                exchange: BrokerConstants.NotificationsExchange,
                routingKey: routingKey,
                basicProperties: props,
                body: body);
        }

        public void PublishEmailNotification<T>(T notification) where T : BaseEmailNotification
        {
            if(connection == null || !connection.IsOpen)
            {
                return;
            }

            using var channel = connection.CreateModel();

            var json = JsonConvert.SerializeObject(notification);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2;
            props.Headers = new Dictionary<string, object>
            {
                { BrokerConstants.NotificationHeader, notification.Type.ToString() }
            };

            channel.BasicPublish(
                exchange: "", 
                routingKey: BrokerConstants.EmailNotificationsQueue, 
                basicProperties: props,
                body: body);
        }

        public void PublishSearchIndexNotification<T>(T notification, string index) where T : BaseSearchIndexNotification
        {
            if (connection == null || !connection.IsOpen)
            {
                return;
            }

            using var channel = connection.CreateModel();

            var json = JsonConvert.SerializeObject(notification);
            var body = Encoding.UTF8.GetBytes(json);

            var props = channel.CreateBasicProperties();
            props.ContentType = "text/plain";
            props.DeliveryMode = 2;
            props.Headers = new Dictionary<string, object>
            {
                { BrokerConstants.NotificationHeader, notification.Type.ToString() },
                { BrokerConstants.IndexNameHeader, index }
            };

            channel.BasicPublish(
                exchange: "",
                routingKey: BrokerConstants.SearchIndexNotificationsQueue,
                basicProperties: props,
                body: body);
        }
    }
}
