using Algolia.Search.Clients;
using Hangfire;
using Hangfire.PostgreSql;
using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Infrastructure.Mappings;
using Harmony.Infrastructure.Services.Identity;
using Harmony.Infrastructure.Services.Management;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Notifications.Contracts.Notifications.SearchIndex;
using Harmony.Notifications.Services.Notifications.Email;
using Harmony.Notifications.Services.Notifications.SearchIndex;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Notifications.Extensions
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

        internal static IServiceCollection AddEmailNotificationServices(this IServiceCollection services)
        {
            services.AddScoped<ICardDueDateNotificationService, CardDueDateNotificationService>();
            services.AddScoped<IMemberAddedToCardNotificationService, MemberAddedToCardNotificationService>();
            services.AddScoped<IMemberRemovedFromCardNotificationService, MemberRemovedFromCardNotificationService>();
            services.AddScoped<ICardCompletedNotificationService, CardCompletedNotificationService>();
            services.AddScoped<IMemberAddedToBoardNotificationService, MemberAddedToBoardNotificationService>();
            services.AddScoped<IMemberRemovedFromBoardNotificationService, MemberRemovedFromBoardNotificationService>();
            services.AddScoped<IMemberAddedToWorkspaceNotificationService, MemberAddedToWorkspaceNotificationService>();
            services.AddScoped<IMemberRemovedFromWorkspaceNotificationService, MemberRemovedFromWorkspaceNotificationService>();

            return services;
        }

        internal static IServiceCollection AddSearchIndexNotificationServices(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationId = configuration["AlgoliaConfiguration:ApplicationId"];
            var apiKey = configuration["AlgoliaConfiguration:ApiKey"];


            services.Configure<AlgoliaConfiguration>(configuration.GetSection("AlgoliaConfiguration"));

            if (string.IsNullOrEmpty(applicationId) || string.IsNullOrEmpty(apiKey))
            {
                return services;
            }

            services.AddSingleton<ISearchClient>(new SearchClient(applicationId, apiKey));

            services.AddScoped<ISearchIndexNotificationService, AlgoliaSearchIndexNotificationService>();
            return services;
        }

        internal static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services
                .AddIdentity<HarmonyUser, HarmonyRole>(options =>
                {
                    options.Password.RequiredLength = 6;
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequireUppercase = false;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<HarmonyContext>()
                .AddDefaultTokenProviders();

            return services;
        }

        internal static IServiceCollection AddHarmonyDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<HarmonyContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("HarmonyConnection"));
                    options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                });

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
                .AddDbContext<NotificationContext>(options =>
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
                        .AddDbContext<NotificationContext>(options =>
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

        internal static IServiceCollection ConfigureBrevo(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));

            return services;
        }
    }
}
