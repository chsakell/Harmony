using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Automations.Services;
using Harmony.Infrastructure.Mappings;
using Harmony.Infrastructure.Seed;
using Harmony.Infrastructure.Services.Identity;
using Harmony.Infrastructure.Services.Management;
using Harmony.Messaging;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Harmony.Automations.Extensions
{
    /// <summary>
    /// Extension class to register services per category
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IBoardService, BoardService>();
            services.AddAutoMapper(typeof(UserProfile).Assembly);

            return services;
        }

        internal static IServiceCollection AddEndpointConfiguration(this IServiceCollection services,
           IConfiguration configuration)
        {
            var endpointConfiguration = configuration
                .GetSection(nameof(AppEndpointConfiguration));

            services.Configure<AppEndpointConfiguration>(endpointConfiguration);

            return services;
        }

        internal static IServiceCollection AddJobsDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<NotificationContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("HarmonyJobsConnection"));
                    options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                });

        internal static IServiceCollection AddDbSeed(
            this IServiceCollection services)
            => services.AddScoped<IDatabaseSeeder, MongoDbSeeder>();

        public static void AddAutomationApplicationLayer(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        }

        internal static IServiceCollection AddAutomationServices(this IServiceCollection services)
        {
            services.AddScoped<IAutomationService<CardMovedMessage>, CardMovedAutomationService>();
            services.AddScoped<IAutomationService<CardCreatedMessage>, CardCreatedAutomationService>();

            services.AddScoped<ISyncParentAndChildIssuesAutomationService, SyncParentAndChildIssuesAutomationService>();
            services.AddScoped<ISmartAutoAssignAutomationService, SmartAutoAssignAutomationService>();
            return services;
        }

        internal static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbConfiguration>(configuration.GetSection("MongoDb"));

            Persistence.Configurations.MongoDb.MongoDbConfiguration.Configure();

            return services;
        }

        internal static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BrokerConfiguration>(configuration.GetSection("BrokerConfiguration"));

            services.AddSingleton<INotificationsPublisher, RabbitMQNotificationPublisher>();

            return services;
        }
    }
}
