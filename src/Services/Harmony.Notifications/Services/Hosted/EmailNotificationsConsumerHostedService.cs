using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Notifications.Email;
using Harmony.Domain.Enums;
using Harmony.Messaging;
using Harmony.Notifications.Contracts.Notifications.Email;
using Microsoft.Extensions.Options;
using Polly.Registry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Notifications.Services.Hosted
{
    public class EmailNotificationsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
        private readonly RabbitMqHealthCheck _rabbitMqHealthCheck;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public EmailNotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            RabbitMqHealthCheck rabbitMqHealthCheck,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<EmailNotificationsConsumerHostedService>();
            _brokerConfiguration = brokerConfig.Value;
            _resiliencePipelineProvider = resiliencePipelineProvider;
            _rabbitMqHealthCheck = rabbitMqHealthCheck;
            _serviceProvider = serviceProvider;
        }

        private async Task InitRabbitMQAsync(CancellationToken cancellationToken)
        {
            var factory = RabbitMqConnectionFactoryBuilder.Create(_brokerConfiguration, automaticRecovery: true);

            // create connection  
            _connection = await factory.CreateConnectionAsync(AppServices.Notifications, cancellationToken);

            // create channel  
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: BrokerConstants.EmailNotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);

            await _channel.BasicQosAsync(0, 1, false, cancellationToken);

            _connection.ConnectionShutdownAsync += RabbitMQ_ConnectionShutdownAsync;
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
                                 Enum.TryParse<EmailNotificationType>(Encoding.UTF8.GetString((byte[])notificationTypeRaw), out var notificationType))
                            {
                                using (IServiceScope scope = _serviceProvider.CreateScope())
                                {
                                    switch (notificationType)
                                    {
                                        case EmailNotificationType.MemberAddedToCard:
                                            var memberAddedToCardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberAddedToCardNotificationService>();
                                            var memberAddedToCardNotification = JsonSerializer
                                                                .Deserialize<MemberAddedToCardNotification>(ea.Body.Span);

                                            if (memberAddedToCardNotification != null)
                                            {
                                                await memberAddedToCardNotificationService.Notify(memberAddedToCardNotification);
                                            }
                                            break;
                                        case EmailNotificationType.MemberRemovedFromCard:
                                            var memberRemovedFromCardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberRemovedFromCardNotificationService>();
                                            var memberRemovedFromCardNotification = JsonSerializer
                                                                .Deserialize<MemberRemovedFromCardNotification>(ea.Body.Span);

                                            if (memberRemovedFromCardNotification != null)
                                            {
                                                await memberRemovedFromCardNotificationService.Notify(memberRemovedFromCardNotification);
                                            }
                                            break;
                                        case EmailNotificationType.CardDueDateUpdated:
                                            var _cardDueDateNotificationService = scope.ServiceProvider.GetRequiredService<ICardDueDateNotificationService>();
                                            var dateChangedNotification = JsonSerializer
                                                                .Deserialize<CardDueTimeUpdatedNotification>(ea.Body.Span);

                                            if (dateChangedNotification != null)
                                            {
                                                await _cardDueDateNotificationService.Notify(dateChangedNotification);
                                            }
                                            break;
                                        case EmailNotificationType.CardCompleted:
                                            var _cardCompletedNotificationService = scope.ServiceProvider.GetRequiredService<ICardCompletedNotificationService>();
                                            var cardCompletedNotification = JsonSerializer
                                                                .Deserialize<CardCompletedNotification>(ea.Body.Span);

                                            if (cardCompletedNotification != null)
                                            {
                                                await _cardCompletedNotificationService.Notify(cardCompletedNotification);
                                            }
                                            break;
                                        case EmailNotificationType.MemberAddedToBoard:
                                            var memberAddedToBoardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberAddedToBoardNotificationService>();
                                            var memberAddedToBoardNotification = JsonSerializer
                                                                .Deserialize<MemberAddedToBoardNotification>(ea.Body.Span);

                                            if (memberAddedToBoardNotification != null)
                                            {
                                                await memberAddedToBoardNotificationService.Notify(memberAddedToBoardNotification);
                                            }
                                            break;
                                        case EmailNotificationType.MemberRemovedFromBoard:
                                            var memberRemovedFromBoardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberRemovedFromBoardNotificationService>();
                                            var memberRemovedFromBoardNotification = JsonSerializer
                                                                .Deserialize<MemberRemovedFromBoardNotification>(ea.Body.Span);

                                            if (memberRemovedFromBoardNotification != null)
                                            {
                                                await memberRemovedFromBoardNotificationService.Notify(memberRemovedFromBoardNotification);
                                            }
                                            break;
                                        case EmailNotificationType.MemberAddedToWorkspace:
                                            var memberAddedToWorkspaceNotificationService = scope.ServiceProvider.GetRequiredService<IMemberAddedToWorkspaceNotificationService>();
                                            var memberAddedToWorkspaceNotification = JsonSerializer
                                                                .Deserialize<MemberAddedToWorkspaceNotification>(ea.Body.Span);

                                            if (memberAddedToWorkspaceNotification != null)
                                            {
                                                await memberAddedToWorkspaceNotificationService.Notify(memberAddedToWorkspaceNotification);
                                            }
                                            break;
                                        case EmailNotificationType.MemberRemovedFromWorkspace:
                                            var memberRemovedFromWorkspaceNotificationService = scope.ServiceProvider.GetRequiredService<IMemberRemovedFromWorkspaceNotificationService>();
                                            var memberRemovedFromWorkspaceNotification = JsonSerializer
                                                                .Deserialize<MemberRemovedFromWorkspaceNotification>(ea.Body.Span);

                                            if (memberRemovedFromWorkspaceNotification != null)
                                            {
                                                await memberRemovedFromWorkspaceNotificationService.Notify(memberRemovedFromWorkspaceNotification);
                                            }
                                            break;
                                    }
                                }
                            }
                        };

                        consumer.ShutdownAsync += OnConsumerShutdownAsync;
                        consumer.RegisteredAsync += OnConsumerRegisteredAsync;
                        consumer.UnregisteredAsync += OnConsumerUnregisteredAsync;

                        await _channel.BasicConsumeAsync(BrokerConstants.EmailNotificationsQueue, true, consumer, token);

                        _rabbitMqHealthCheck.Connected = true;
                    }, stoppingToken);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {_brokerConfiguration.Host}:{_brokerConfiguration.Port} {ex}");
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
