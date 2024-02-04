using Algolia.Search.Clients;
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

        internal static IServiceCollection ConfigureBrevo(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));

            return services;
        }
    }
}
