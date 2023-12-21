using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Harmony.Messaging
{
    public class RabbitMQNotificationPublisher : INotificationsPublisher
    {
        private readonly ILogger<RabbitMQNotificationPublisher> _logger;
        IConnection connection;
        public RabbitMQNotificationPublisher(IOptions<BrokerConfiguration> brokerConfig,
            ILogger<RabbitMQNotificationPublisher> logger)
        {
            var config = brokerConfig.Value;
            _logger = logger;

            var factory = new ConnectionFactory
            {
                HostName = config.Host,
                Port = config.Port,
                UserName = string.IsNullOrEmpty(config.Username)
                    ? ConnectionFactory.DefaultUser : config.Username,
                Password = string.IsNullOrEmpty(config.Password)
                    ? ConnectionFactory.DefaultPass : config.Password,
                VirtualHost = string.IsNullOrEmpty(config.VirtualHost) ?
                    ConnectionFactory.DefaultVHost : config.VirtualHost,
            };

            try
            {
                connection = factory.CreateConnection();

                using var channel = connection.CreateModel();

                channel.QueueDeclare(
                    queue: BrokerConstants.NotificationsQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {ex}");
            }
        }

        public void Publish<T>(T notification) where T : BaseEmailNotification
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
                routingKey: BrokerConstants.NotificationsQueue, 
                basicProperties: props,
                body: body);
        }
    }
}
