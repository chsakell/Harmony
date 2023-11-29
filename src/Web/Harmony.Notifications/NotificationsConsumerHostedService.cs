using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Enums;
using Harmony.Application.Notifications;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading.Channels;

namespace Harmony.Notifications
{
    public class NotificationsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private IConnection? _connection;
        private IModel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public NotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig)
        {
            _logger = loggerFactory.CreateLogger<NotificationsConsumerHostedService>();
            _brokerConfiguration = brokerConfig.Value;

            InitRabbitMQ();
        }

        private void InitRabbitMQ()
        {
            var factory = new ConnectionFactory 
            { 
                HostName = _brokerConfiguration.Host,
                Port = _brokerConfiguration.Port,
                AutomaticRecoveryEnabled = true
            };

            // create connection  
            _connection = factory.CreateConnection();

            // create channel  
            _channel = _connection.CreateModel();

            //_channel.ExchangeDeclare("demo.exchange", ExchangeType.Topic);
            _channel.QueueDeclare(
                queue: BrokerConstants.NotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            //_channel.QueueBind("notifications", "", "notifications", null);
            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, ea) =>
            {
               if(ea.BasicProperties.Headers
                    .TryGetValue(BrokerConstants.NotificationHeader, out var notificationTypeRaw) && 
                    Enum.TryParse<NotificationType>(Encoding.UTF8.GetString((byte[])notificationTypeRaw), out var notificationType))
                {
                    switch (notificationType)
                    {
                        case NotificationType.CardChangedDueDate:
                            var notification = JsonSerializer
                                                .Deserialize<BaseNotification>(ea.Body.Span);
                            break;
                        default:
                            break;
                    }
                }
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(BrokerConstants.NotificationsQueue, true, consumer);
            return Task.CompletedTask;
        }

        private void OnConsumerConsumerCancelled(object? sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object? sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object? sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object? sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
