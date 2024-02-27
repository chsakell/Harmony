using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Notifications;
using Harmony.Domain.Enums;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Registry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.SignalR.Services.Hosted
{
    public class PushNotificationsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public PushNotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<PushNotificationsConsumerHostedService>();
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
            _logger.LogInformation($"Trying to connect to {_brokerConfiguration.Host}:{_brokerConfiguration.Port} ");

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
            _connection = factory.CreateConnection(AppServices.SignalR);

            // create channel  
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(exchange: BrokerConstants.NotificationsExchange, type: ExchangeType.Topic);

            _channel.QueueDeclare(
                queue: BrokerConstants.SignalrNotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(queue: BrokerConstants.SignalrNotificationsQueue,
                      exchange: BrokerConstants.NotificationsExchange,
                      routingKey: BrokerConstants.RoutingKeys.SignalR);

            _channel.QueueBind(queue: BrokerConstants.SignalrNotificationsQueue,
                      exchange: BrokerConstants.NotificationsExchange,
                      routingKey: BrokerConstants.RoutingKeys.Notifications);

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
                    using (IServiceScope scope = _serviceProvider.CreateScope())
                    {
                        var hubClientNotifierService = scope.ServiceProvider.GetRequiredService<IHubClientNotifierService>();

                        switch (notificationType)
                        {
                            case NotificationType.CardCreated:
                                {
                                    var notification = JsonSerializer
                                                        .Deserialize<CardCreatedMessage>(ea.Body.Span);
                                    if (notification != null &&
                                        notification.Card.Status == CardStatus.Active)
                                    {
                                        await hubClientNotifierService
                                                .AddCardToBoard(notification);
                                    }
                                }
                                break;
                            case NotificationType.CardStatusChanged:
                                {
                                    var notification = JsonSerializer
                                                        .Deserialize<CardStatusChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService
                                            .ChangeCardStatus(notification);
                                }
                                break;
                            case NotificationType.CardMoved:
                                {
                                    var notification = JsonSerializer
                                                        .Deserialize<CardMovedMessage>(ea.Body.Span);
                                    if (notification != null &&
                                        notification.MovedFromListId.HasValue &&
                                        notification.MovedToListId.HasValue
                                        && !notification.ParentCardId.HasValue)
                                    {
                                        await hubClientNotifierService
                                                .UpdateCardPosition(notification.BoardId, notification.CardId,
                                                notification.MovedFromListId.Value, notification.MovedToListId.Value,
                                                notification.FromPosition, notification.ToPosition.Value, notification.UpdateId.Value);
                                    }
                                }
                                break;
                            case NotificationType.BoardListCreated:
                                {
                                    var notification = JsonSerializer
                                                        .Deserialize<BoardListCreatedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.AddBoardList(notification.BoardId, notification.BoardList);
                                }
                                break;
                            case NotificationType.CardTitleChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardTitleChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateCardTitle(message.BoardId, message.CardId, message.Title);
                                }
                                break;
                            case NotificationType.CardDescriptionChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardDescriptionChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateCardDescription(message.BoardId, message.CardId, message.Description);
                                }
                                break;
                            case NotificationType.CardDatesChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardDatesChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateCardDates(message.BoardId, message.CardId, message.StartDate, message.DueDate);
                                }
                                break;
                            case NotificationType.CardItemAdded:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardItemAddedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.CreateCheckListItem(message.BoardId, message.CardId);
                                }
                                break;
                            case NotificationType.CardIssueTypeChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardIssueTypeChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateCardIssueType(message.BoardId, message.CardId, message.IssueType);
                                }
                                break;
                            case NotificationType.BoardListTitleChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<BoardListTitleChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateBoardListTitle(message.BoardId, message.BoardListId, message.Title);
                                }
                                break;
                            case NotificationType.BoardListsPositionChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<BoardListsPositionsChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateBoardListsPositions(message.BoardId, message.ListPositions);
                                }
                                break;
                            case NotificationType.BoardListArchived:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<BoardListArchivedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.ArchiveBoardList(message.BoardId, message.ArchivedList, message.Positions);
                                }
                                break;
                            case NotificationType.CardItemCheckedChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardItemCheckedChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.ToggleCardListItemChecked(message.BoardId, message.CardId, message.CheckListItemId, message.IsChecked);
                                }
                                break;
                            case NotificationType.CardLabelRemoved:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardLabelRemovedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.RemoveCardLabel(message.BoardId, message.CardLabelId);
                                }
                                break;
                            case NotificationType.CardLabelToggled:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardLabelToggledMessage>(ea.Body.Span);

                                    await hubClientNotifierService.ToggleCardLabel(message.BoardId, message.CardId, message.Label);
                                }
                                break;
                            case NotificationType.CardStoryPointsChanged:
                                {
                                    var message = JsonSerializer
                                                        .Deserialize<CardStoryPointsChangedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.UpdateCardStoryPoints(message.BoardId, message.CardId, message.StoryPoints);
                                }
                                break;
                            case NotificationType.CardAttachmentAdded:
                                {
                                    var message = JsonSerializer
                                    .Deserialize<AttachmentAddedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.AddCardAttachment(message.BoardId, message.CardId, message.Attachment);
                                }
                                break;
                            case NotificationType.CardAttachmentRemoved:
                                {
                                    var message = JsonSerializer
                                    .Deserialize<AttachmentRemovedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.RemoveCardAttachment(message.BoardId, message.CardId, message.AttachmentId);
                                }
                                break;
                            case NotificationType.CardMemberRemoved:
                                {
                                    var message = JsonSerializer
                                    .Deserialize<CardMemberRemovedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.RemoveCardMember(message.BoardId, message.CardId, message.Member);
                                }
                                break;
                            case NotificationType.CardMemberAdded:
                                {
                                    var message = JsonSerializer
                                    .Deserialize<CardMemberAddedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.AddCardMember(message.BoardId, message.CardId, message.Member);
                                }
                                break;
                            case NotificationType.CheckListRemoved:
                                {
                                    var message = JsonSerializer
                                    .Deserialize<CheckListRemovedMessage>(ea.Body.Span);

                                    await hubClientNotifierService.RemoveCheckList(message.BoardId, message.CheckListId,
                                        message.CardId, message.TotalItems, message.TotalItemsCompleted);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(BrokerConstants.SignalrNotificationsQueue, true, consumer);
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
