using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Enums;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;
using Harmony.Messaging;
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
        private readonly ResiliencePipelineProvider<string> _resiliencePipelineProvider;
        private readonly RabbitMqHealthCheck _rabbitMqHealthCheck;
        private readonly IServiceProvider _serviceProvider;
        private IConnection? _connection;
        private IChannel? _channel;
        private BrokerConfiguration _brokerConfiguration;

        public SearchIndexNotificationsConsumerHostedService(ILoggerFactory loggerFactory,
            IOptions<BrokerConfiguration> brokerConfig,
            ResiliencePipelineProvider<string> resiliencePipelineProvider,
            RabbitMqHealthCheck rabbitMqHealthCheck,
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<SearchIndexNotificationsConsumerHostedService>();
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
            _connection = await factory.CreateConnectionAsync(AppServices.Notifications, cancellationToken);

            // create channel  
            _channel = await _connection.CreateChannelAsync(cancellationToken: cancellationToken);

            await _channel.QueueDeclareAsync(
                queue: BrokerConstants.SearchIndexNotificationsQueue,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
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

                        consumer.ShutdownAsync += OnConsumerShutdownAsync;
                        consumer.RegisteredAsync += OnConsumerRegisteredAsync;
                        consumer.UnregisteredAsync += OnConsumerUnregisteredAsync;

                        await _channel.BasicConsumeAsync(BrokerConstants.SearchIndexNotificationsQueue, true, consumer, token);

                        _rabbitMqHealthCheck.Connected = true;
                    }, stoppingToken);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {_brokerConfiguration.Host}:{_brokerConfiguration.Port} {ex}");
            }
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
