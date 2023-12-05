using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Services;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace Harmony.Notifications
{
    public class NotificationsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public NotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<NotificationsConsumerHostedService>();
            _brokerConfiguration = brokerConfig.Value;

            InitRabbitMQ();
            _serviceProvider = serviceProvider;
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += async (ch, ea) =>
            {
                if (ea.BasicProperties.Headers
                     .TryGetValue(BrokerConstants.NotificationHeader, out var notificationTypeRaw) &&
                     Enum.TryParse<NotificationType>(Encoding.UTF8.GetString((byte[])notificationTypeRaw), out var notificationType))
                {
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        switch (notificationType)
                        {
                            case NotificationType.MemberAddedToCard:
                                var memberAddedToCardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberAddedToCardNotificationService>();
                                var memberAddedToCardNotification = JsonSerializer
                                                    .Deserialize<MemberAddedToCardNotification>(ea.Body.Span);

                                if (memberAddedToCardNotification != null)
                                {
                                    await memberAddedToCardNotificationService.Notify(memberAddedToCardNotification);
                                }
                                break;
                            case NotificationType.CardDueDateUpdated:
                                var _cardDueDateNotificationService = scope.ServiceProvider.GetRequiredService<ICardDueDateNotificationService>();
                                var dateChangedNotification = JsonSerializer
                                                    .Deserialize<CardDueTimeUpdatedNotification>(ea.Body.Span);

                                if (dateChangedNotification != null)
                                {
                                    await _cardDueDateNotificationService.Notify(dateChangedNotification);
                                }
                                break;
                            case NotificationType.CardCompleted:
                                var _cardCompletedNotificationService = scope.ServiceProvider.GetRequiredService<ICardCompletedNotificationService>();
                                var cardCompletedNotification = JsonSerializer
                                                    .Deserialize<CardCompletedNotification>(ea.Body.Span);

                                if (cardCompletedNotification != null)
                                {
                                    await _cardCompletedNotificationService.Notify(cardCompletedNotification);
                                }
                                break;
                            case NotificationType.MemberAddedToBoard:
                                var memberAddedToBoardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberAddedToBoardNotificationService>();
                                var memberAddedToBoardNotification = JsonSerializer
                                                    .Deserialize<MemberAddedToBoardNotification>(ea.Body.Span);

                                if (memberAddedToBoardNotification != null)
                                {
                                    await memberAddedToBoardNotificationService.Notify(memberAddedToBoardNotification);
                                }
                                break;
                            case NotificationType.MemberRemovedFromBoard:
                                var memberRemovedFromBoardNotificationService = scope.ServiceProvider.GetRequiredService<IMemberRemovedFromBoardNotificationService>();
                                var memberRemovedFromBoardNotification = JsonSerializer
                                                    .Deserialize<MemberRemovedFromBoardNotification>(ea.Body.Span);

                                if (memberRemovedFromBoardNotification != null)
                                {
                                    await memberRemovedFromBoardNotificationService.Notify(memberRemovedFromBoardNotification);
                                }
                                break;
                            case NotificationType.MemberAddedToWorkspace:
                                var memberAddedToWorkspaceNotificationService = scope.ServiceProvider.GetRequiredService<IMemberAddedToWorkspaceNotificationService>();
                                var memberAddedToWorkspaceNotification = JsonSerializer
                                                    .Deserialize<MemberAddedToWorkspaceNotification>(ea.Body.Span);

                                if (memberAddedToWorkspaceNotification != null)
                                {
                                    await memberAddedToWorkspaceNotificationService.Notify(memberAddedToWorkspaceNotification);
                                }
                                break;
                            case NotificationType.MemberRemovedFromWorkspace:
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

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(BrokerConstants.NotificationsQueue, true, consumer);
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
