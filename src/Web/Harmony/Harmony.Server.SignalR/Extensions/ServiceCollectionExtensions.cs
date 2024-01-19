using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Messaging;
using Harmony.Server.SignalR.Services;

namespace Harmony.Server.SignalR.Extensions
{
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddClientNotificationService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IHubClientNotifierService, HubClientNotifierService>();
            return services;
        }

        internal static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BrokerConfiguration>(configuration.GetSection("BrokerConfiguration"));

            services.AddSingleton<INotificationsPublisher, RabbitMQNotificationPublisher>();

            return services;
        }

        internal static IServiceCollection AddSignalR(this IServiceCollection services, IConfiguration configuration)
        {
            var redisConnection = configuration["RedisConnectionString"];

            if (!string.IsNullOrEmpty(redisConnection))
            {
                services.AddSignalR().AddStackExchangeRedis(redisConnection);
            }
            else
            {
                services.AddSignalR();
            }

            return services;
        }
    }
}
