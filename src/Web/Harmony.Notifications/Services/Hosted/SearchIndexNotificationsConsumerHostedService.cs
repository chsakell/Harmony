using Harmony.Application.Configurations;
using Harmony.Application.Constants;
using Harmony.Application.Enums;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;
using Harmony.Notifications.Contracts.Notifications.SearchIndex;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

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
            IServiceProvider serviceProvider)
        {
            _logger = loggerFactory.CreateLogger<SearchIndexNotificationsConsumerHostedService>();
            _brokerConfiguration = brokerConfig.Value;

            try
            {
                InitRabbitMQ();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to RabbitMQ {ex}");
            }

            _serviceProvider = serviceProvider;
        }

        private void InitRabbitMQ()
        {
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
            _connection = factory.CreateConnection();

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
                     Enum.TryParse<SearchIndexNotificationType>(Encoding.UTF8.GetString((byte[])notificationTypeRaw), out var notificationType))
                {
                    switch (notificationType)
                    {
                        case SearchIndexNotificationType.CardAddedToBoard:
                            await Index<CardCreatedIndexNotification>(ea, SearchIndexOperation.AddToIndex);
                            break;

                        case SearchIndexNotificationType.CardTitleUpdated:
                            await Index<CardTitleUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex);
                            break;

                        case SearchIndexNotificationType.CardStatusUpdated:
                            await Index<CardStatusUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex);
                            break;

                        case SearchIndexNotificationType.CardListUpdated:
                            await Index<CardListUpdatedIndexNotification>(ea, SearchIndexOperation.UpdateObjectInIndex);
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

        private async Task Index<T>(BasicDeliverEventArgs args, SearchIndexOperation operation) where T : class, ISearchIndexNotification
        {
            var notification = JsonSerializer.Deserialize<T>(args.Body.Span);

            if (notification != null)
            {
                using (IServiceScope scope = _serviceProvider.CreateScope())
                {
                    var searchIndexNotificationService = scope.ServiceProvider.GetRequiredService<ISearchIndexNotificationService>();

                    switch(operation)
                    {
                        case SearchIndexOperation.AddToIndex:
                            await searchIndexNotificationService.AddToIndex(notification);
                            break;
                        case SearchIndexOperation.UpdateObjectInIndex:
                            await searchIndexNotificationService.UpdateCard(notification);
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
