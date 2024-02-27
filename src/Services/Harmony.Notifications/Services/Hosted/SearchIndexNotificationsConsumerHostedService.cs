using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Enums;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.SearchIndex;
using Microsoft.Extensions.Options;
using Polly.Registry;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Notifications.Services.Hosted
{
    public class SearchIndexNotificationsConsumerHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IModel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public SearchIndexNotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<SearchIndexNotificationsConsumerHostedService>();
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
            _connection = factory.CreateConnection(AppServices.Notifications);

            // create channel  
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(
                queue: BrokerConstants.SearchIndexNotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

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
                     .TryGetValue(BrokerConstants.NotificationHeader, out var notificationTypeRaw)  &&
                     Enum.TryParse<SearchIndexNotificationType>(Encoding.UTF8.GetString((byte[])notificationTypeRaw), out var notificationType)
                     && ea.BasicProperties.Headers.TryGetValue(BrokerConstants.IndexNameHeader, out var indexNameRaw) &&
                     (Encoding.UTF8.GetString((byte[])indexNameRaw) is string index))
                {
                    switch (notificationType)
                    {
                        case SearchIndexNotificationType.BoardCreated:
                            await Index<BoardCreatedIndexNotification>(ea, SearchIndexOperation.CreateIndex, index);
                            break;
                        case SearchIndexNotificationType.CardAddedToBoard:
                            await Index<CardCreatedIndexNotification>(ea, SearchIndexOperation.AddToIndex, index);
                            break;

                        case SearchIndexNotificationType.CardTitleUpdated:
                            await Index<CardTitleUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;

                        case SearchIndexNotificationType.CardStatusUpdated:
                            await Index<CardStatusUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;

                        case SearchIndexNotificationType.CardListUpdated:
                            await Index<CardListUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;
                        case SearchIndexNotificationType.CardIssueTypeUpdated:
                            await Index<CardIssueTypeUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;

                        case SearchIndexNotificationType.CardHasAttachmentsUpdated:
                            await Index<CardHasAttachmentsUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;

                        case SearchIndexNotificationType.CardMembersUpdated:
                            await Index<CardMembersUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;

                        case SearchIndexNotificationType.CardDescriptionUpdated:
                            await Index<CardDescriptionUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;

                        case SearchIndexNotificationType.CardDueDateUpdated:
                            await Index<CardDueDateUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex, index);
                            break;
                    }
                }
            };

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            _channel.BasicConsume(BrokerConstants.SearchIndexNotificationsQueue, true, consumer);
        }

        private async Task Index<T>(BasicDeliverEventArgs args, SearchIndexOperation operation, string index) where T : class, ISearchIndexNotification
        {
            var notification = JsonSerializer.Deserialize<T>(args.Body.Span);

            if (notification != null)
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var algoliaConfiguration = scope.ServiceProvider.GetRequiredService<IOptions<AlgoliaConfiguration>>();
                    
                    if(algoliaConfiguration == null || !algoliaConfiguration.Value.Enabled)
                    {
                        return;
                    }

                    var searchIndexNotificationService = scope.ServiceProvider.GetRequiredService<ISearchIndexNotificationService>();

                    switch(operation)
                    {
                        case SearchIndexOperation.CreateIndex:
                            await searchIndexNotificationService.CreateIndex(notification, index);
                            break;
                        case SearchIndexOperation.AddToIndex:
                            await searchIndexNotificationService.AddToIndex(notification, index);
                            break;
                        case SearchIndexOperation.UpdateObjectInIndex:
                            await searchIndexNotificationService.UpdateCard(notification, index);
                            break;
                    }
                    
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
