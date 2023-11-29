using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Messaging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Harmony.Messaging
{
    public class RabbitMQNotificationPublisher : INotificationsPublisher
    {
        IConnection connection;
        public RabbitMQNotificationPublisher(IOptions<BrokerConfiguration> brokerConfig)
        {
            var config = brokerConfig.Value;

            var factory = new ConnectionFactory
            {
                HostName = config.Host,
                Port = config.Port
            };

            connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(
                queue: BrokerConstants.NotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);
        }

        public void Publish<T>(T notification)
        {
            using var channel = connection.CreateModel();

            var json = JsonConvert.SerializeObject(notification);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(
                exchange: "", 
                routingKey: BrokerConstants.NotificationsQueue, 
                basicProperties: null,
                body: body);
        }
    }
}
