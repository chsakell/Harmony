using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Messaging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System.Text;

namespace Harmony.Messaging
{
    public class RabbitMQProducer : IMessageProducer
    {
        IConnection connection;
        const string NotificationsQueue = "notifications";
        public RabbitMQProducer(IOptions<BrokerConfiguration> brokerConfig)
        {
            var config = brokerConfig.Value;

            var factory = new ConnectionFactory
            {
                HostName = config.Host,
                Port = config.Port
            };

            connection = factory.CreateConnection();

            using var channel = connection.CreateModel();

            channel.QueueDeclare(NotificationsQueue, exclusive: false);
        }

        public void SendMessage<T>(T message)
        {
            using var channel = connection.CreateModel();

            var json = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(json);

            channel.BasicPublish(exchange: "", routingKey: NotificationsQueue, body: body);
        }
    }
}
