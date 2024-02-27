using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Domain.Enums;
using Microsoft.Extensions.Options;
using Polly.Registry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Automations.Services.Hosted
{
    public class AutomationNotificationsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public AutomationNotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<AutomationNotificationsConsumerHostedService>();
            _brokerConfiguration = brokerConfig.Value;

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
                _logger.LogError($"Failed to connect to RabbitMQ {_brokerConfiguration.Host}:{_brokerConfiguration.Port} {ex}");
            }

            _serviceProvider = serviceProvider;
        }

        private void InitRabbitMQ()
        {
            _logger.LogInformation($"Trying to connect to {_brokerConfiguration.Host}:{_brokerConfiguration.Port}");


            var factory = new ConnectionFactory
            {
                HostName = _brokerConfiguration.Host,
                Port = _brokerConfiguration.Port,
                AutomaticRecoveryEnabled = true,
                UserName = string.IsNullOrEmpty(_brokerConfiguration.Username)
                    ? ConnectionFactory.DefaultUser : _brokerConfiguration.Username,
                Password = string.IsNullOrEmpty(_brokerConfiguration.Password)
                    ? ConnectionFactory.DefaultPass : _brokerConfiguration.Password,
                VirtualHost = string.IsNullOrEmpty(_brokerConfiguration.VirtualHost) ?
                    ConnectionFactory.DefaultVHost : _brokerConfiguration.VirtualHost,
            };

            // create connection  
            _connection = factory.CreateConnection(AppServices.Automations);

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: BrokerConstants.NotificationsExchange, type: ExchangeType.Topic);

            _channel.QueueDeclare(
                queue: BrokerConstants.AutomationNotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(queue: BrokerConstants.AutomationNotificationsQueue,
                      exchange: BrokerConstants.NotificationsExchange,
                      routingKey: BrokerConstants.RoutingKeys.Automation);

            _channel.QueueBind(queue: BrokerConstants.AutomationNotificationsQueue,
                      exchange: BrokerConstants.NotificationsExchange,
                      routingKey: "notifications");

            _channel.BasicQos(0, 1, false);

            _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

            _logger.LogInformation($"Connected to {_brokerConfiguration.Host}:{_brokerConfiguration.Port}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_channel == null)
            {
                return;
            }

            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                if (ea.BasicProperties.Headers
                     .TryGetValue(BrokerConstants.NotificationHeader, out var notificationTypeRaw) &&
                     Enum.TryParse<NotificationType>(Encoding.UTF8.GetString((byte[])notificationTypeRaw), out var notificationType))
                {
                    switch (notificationType)
                    {
                        case NotificationType.CardMoved:
                            await RunAutomation<CardMovedMessage>(ea);
                            break;
                        case NotificationType.CardCreated:
                            await RunAutomation<CardCreatedMessage>(ea);
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

            _channel.BasicConsume(BrokerConstants.AutomationNotificationsQueue, true, consumer);
        }

        private async Task RunAutomation<T>(BasicDeliverEventArgs eventArgs)
        {
            using (IServiceScope scope = _serviceProvider.CreateScope())
            {
                var automationService = scope.ServiceProvider.GetRequiredService<IAutomationService<T>>();
                var automationNotification = JsonSerializer
                                    .Deserialize<T>(eventArgs.Body.Span);

                if (automationNotification != null)
                {
                    await automationService.Run(automationNotification);
                }
            }
        }

        private void OnConsumerConsumerCancelled(object? sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object? sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object? sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object? sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e) { }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
