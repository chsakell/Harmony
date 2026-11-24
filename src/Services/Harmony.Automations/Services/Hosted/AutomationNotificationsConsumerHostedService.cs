using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Domain.Enums;
using Harmony.Messaging;
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
        private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
        private readonly RabbitMqHealthCheck _rabbitMqHealthCheck;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public AutomationNotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            RabbitMqHealthCheck rabbitMqHealthCheck,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<AutomationNotificationsConsumerHostedService>();
            _brokerConfiguration = brokerConfig.Value;
            _resiliencePipelineProvider = resiliencePipelineProvider;
            _rabbitMqHealthCheck = rabbitMqHealthCheck;
            _serviceProvider = serviceProvider;
        }

        private async Task InitRabbitMQAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Trying to connect to {_brokerConfiguration.Host}:{_brokerConfiguration.Port}");


            var factory = RabbitMqConnectionFactoryBuilder.Create(_brokerConfiguration, automaticRecovery: true);

            // create connection  
            _connection = await factory.CreateConnectionAsync(AppServices.Automations, cancellationToken);

            // create channel  
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.ExchangeDeclareAsync(exchange: BrokerConstants.NotificationsExchange, type: ExchangeType.Topic, cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: BrokerConstants.AutomationNotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(queue: BrokerConstants.AutomationNotificationsQueue,
                      exchange: BrokerConstants.NotificationsExchange,
                      routingKey: BrokerConstants.RoutingKeys.Automation,
                      cancellationToken: cancellationToken);

            await _channel.QueueBindAsync(queue: BrokerConstants.AutomationNotificationsQueue,
                      exchange: BrokerConstants.NotificationsExchange,
                      routingKey: "notifications",
                      cancellationToken: cancellationToken);

            await _channel.BasicQosAsync(0, 1, false, cancellationToken);

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;

            _logger.LogInformation($"Connected to {_brokerConfiguration.Host}:{_brokerConfiguration.Port}");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                await Task.Run(async () =>
                {
                    var pipeline = _resiliencePipelineProvider.GetPipeline(HarmonyRetryPolicy.WaitAndRetry);

                    await pipeline.ExecuteAsync(async token =>
                    {
                        await InitRabbitMQAsync(token);

                        if (_channel == null)
                        {
                            return;
                        }

                        stoppingToken.ThrowIfCancellationRequested();
                        var consumer = new AsyncEventingBasicConsumer(_channel);
                        consumer.ReceivedAsync += async (ch, ea) =>
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
                                    case NotificationType.CardStoryPointsChanged:
                                        await RunAutomation<CardStoryPointsChangedMessage>(ea);
                                        break;
                                    default:
                                        break;
                                }
                            }
                        };

                        consumer.ShutdownAsync += OnConsumerShutdownAsync;
                        consumer.RegisteredAsync += OnConsumerRegisteredAsync;
                        consumer.UnregisteredAsync += OnConsumerUnregisteredAsync;

                        await _channel.BasicConsumeAsync(BrokerConstants.AutomationNotificationsQueue, true, consumer, token);

                        _rabbitMqHealthCheck.Connected = true;
                    }, stoppingToken);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {_brokerConfiguration.Host}:{_brokerConfiguration.Port} {ex}");
            }
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


        private Task OnConsumerUnregisteredAsync(object sender, ConsumerEventArgs e)
        {
            _rabbitMqHealthCheck.Connected = false;
            return Task.CompletedTask;
        }

        private Task OnConsumerRegisteredAsync(object sender, ConsumerEventArgs e)
        {
            _rabbitMqHealthCheck.Connected = true;
            return Task.CompletedTask;
        }

        private Task OnConsumerShutdownAsync(object sender, ShutdownEventArgs e)
        {
            _rabbitMqHealthCheck.Connected = false;
            return Task.CompletedTask;
        }

        private Task RabbitMQ_ConnectionShutdownAsync(object sender, ShutdownEventArgs e)
        {
            _rabbitMqHealthCheck.Connected = false;
            return Task.CompletedTask;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            // Starts ExecuteAsync, which connects to the broker in the background
            await base.StartAsync(cancellationToken);

            // Hosted services start before the web server: holding back here keeps /healthz/ready from
            // answering the first readiness probes with "RabbitMQ connection pending"
            if (!await _rabbitMqHealthCheck.WaitForFirstConnectionAsync(RabbitMqHealthCheck.StartupConnectionTimeout, cancellationToken))
            {
                _logger.LogWarning("RabbitMQ {Host}:{Port} not connected after {Timeout}s - continuing startup, the consumer keeps retrying",
                    _brokerConfiguration.Host, _brokerConfiguration.Port, RabbitMqHealthCheck.StartupConnectionTimeout.TotalSeconds);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            if (_channel != null)
            {
                await _channel.CloseAsync(cancellationToken);
            }

            if (_connection != null)
            {
                await _connection.CloseAsync(cancellationToken);
            }
        }

        public override void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
            base.Dispose();
        }
    }
}
