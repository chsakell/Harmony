using Harmony.Application.Contracts.Services.Hubs;
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
    }
}
