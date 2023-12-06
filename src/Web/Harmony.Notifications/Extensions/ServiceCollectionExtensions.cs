using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.Contracts.Services.UserNotifications;
using Harmony.Infrastructure.Mappings;
using Harmony.Infrastructure.Repositories;
using Harmony.Infrastructure.Services.Identity;
using Harmony.Infrastructure.Services.Management;
using Harmony.Infrastructure.Services.UserNotifications;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Persistence;
using Harmony.Notifications.Services;
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

        internal static IServiceCollection AddNotificationServices(this IServiceCollection services)
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

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services.
                AddScoped<IWorkspaceRepository, WorkspaceRepository>()
                .AddScoped<IUserWorkspaceRepository, UserWorkspaceRepository>()
                .AddScoped<IBoardRepository, BoardRepository>()
                .AddScoped<IUserBoardRepository, UserBoardRepository>()
                .AddScoped<IBoardListRepository, BoardListRepository>()
                .AddScoped<ICardRepository, CardRepository>()
                .AddScoped<ICheckListRepository, CheckListRepository>()
                .AddScoped<ICheckListItemRepository, CheckListItemRepository>()
                .AddScoped<IBoardLabelRepository, BoardLabelRepository>()
                .AddScoped<ICardLabelRepository, CardLabelRepository>()
                .AddScoped<ICardActivityRepository, CardActivityRepository>()
                .AddScoped<IUserCardRepository, UserCardRepository>()
                .AddScoped<IIssueTypeRepository, IssueTypeRepository>()
                .AddScoped<ISprintRepository, SprintRepository>()
                .AddScoped<IUserNotificationRepository, UserNotificationRepository>();
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

        internal static IServiceCollection AddNotificationDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<NotificationContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
                    options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
                    options.EnableDetailedErrors(true);
                    options.EnableSensitiveDataLogging(true);
                });

        internal static IServiceCollection ConfigureBrevo(this IServiceCollection services, IConfiguration configuration)
        {
            //var apiKey = configuration["BrevoSettings:ApiKey"];
            //Configuration.Default.ApiKey.Add("api-key", apiKey);

            services.Configure<BrevoSettings>(configuration.GetSection("BrevoSettings"));

            return services;
        }
    }
}
