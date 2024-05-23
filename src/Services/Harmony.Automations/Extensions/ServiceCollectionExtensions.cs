using Hangfire;
using Hangfire.PostgreSql;
using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Notifications;
using Harmony.Automations.Contracts;
using Harmony.Automations.Services;
using Harmony.Infrastructure.Mappings;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Seed;
using Harmony.Infrastructure.Services.Identity;
using Harmony.Infrastructure.Services.Management;
using Harmony.Messaging;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver.Core.Configuration;
using System.Reflection;

namespace Harmony.Automations.Extensions
{
    /// <summary>
    /// Extension class to register services per category
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        const string HarmonyJobsConnection = "HarmonyJobsConnection";
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

        internal static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var provider = (configuration.GetValue("DatabaseProvider", "SqlServer")).ToLower();

            return provider switch
            {
                "sqlserver" => AddSqlServerDatabase(services, configuration),
                "postgresql" => AddPostgreDatabase(services, configuration),
                _ => throw new Exception($"Unsupported provider: {provider}")
            };
        }

        internal static IServiceCollection AddSqlServerDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<AutomationContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString(HarmonyJobsConnection),
                        options => options.MigrationsAssembly("Harmony.Persistence.Migrations.SqlServer"));

                    options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                });

        internal static IServiceCollection AddPostgreDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            return services
                        .AddDbContext<AutomationContext>(options =>
                        {
                            options.UseNpgsql(configuration
                                .GetConnectionString(HarmonyJobsConnection),
                                options => options.MigrationsAssembly("Harmony.Persistence.Migrations.PostgreSql"));

                            options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
                            options.EnableDetailedErrors(true);
                            options.EnableSensitiveDataLogging(true);
                        });
        }

        internal static IServiceCollection AddHangFire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var provider = (configuration.GetValue("DatabaseProvider", "SqlServer")).ToLower();

            return provider switch
            {
                "sqlserver" => AddSqlServerHangFire(services, configuration),
                "postgresql" => AddPostgreSqlHangFire(services, configuration),
                _ => throw new Exception($"Unsupported provider: {provider}")
            };
        }

        internal static IServiceCollection AddSqlServerHangFire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(HarmonyJobsConnection);

            return services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(connectionString));
        }

        internal static IServiceCollection AddPostgreSqlHangFire(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString(HarmonyJobsConnection);

            return services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options =>
                {
                    options.UseNpgsqlConnection(connectionString);
                }));
        }

        internal static IServiceCollection AddDbSeed(
            this IServiceCollection services)
            => services.AddScoped<IDatabaseSeeder, MongoDbSeeder>();

        public static void AddAutomationApplicationLayer(this IServiceCollection services)
        {
            //services.AddAutoMapper(Assembly.GetExecutingAssembly());
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));
        }

        internal static IServiceCollection AddAutomationServices(this IServiceCollection services)
        {
            services.AddScoped<IAutomationService<CardMovedMessage>, CardMovedAutomationService>();
            services.AddScoped<IAutomationService<CardCreatedMessage>, CardCreatedAutomationService>();
            services.AddScoped<IAutomationService<CardStoryPointsChangedMessage>, CardStoryPointsChangedAutomationService>();

            services.AddScoped<ISyncParentAndChildIssuesAutomationService, SyncParentAndChildIssuesAutomationService>();
            services.AddScoped<ISmartAutoAssignAutomationService, SmartAutoAssignAutomationService>();
            services.AddScoped<ISumUpStoryPointsAutomationService, SumUpStoryPointsAutomationService>();
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

        public static IServiceCollection AddMongoDbRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped<IAutomationRepository, AutomationRepository>();
        }
    }
}
