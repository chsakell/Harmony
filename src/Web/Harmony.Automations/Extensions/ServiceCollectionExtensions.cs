using Algolia.Search.Clients;
using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Automation;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Automations.Contracts;
using Harmony.Automations.Services;
using Harmony.Infrastructure.Mappings;
using Harmony.Infrastructure.Services.Identity;
using Harmony.Infrastructure.Services.Management;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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

        internal static IServiceCollection AddAutomationServices(this IServiceCollection services)
        {
            services.AddScoped<ICardMovedAutomationService, CardMovedAutomationService>();
            services.AddScoped<ISyncParentAndChildIssuesAutomationService, SyncParentAndChildIssuesAutomationService>();

            return services;
        }

        internal static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<MongoDbConfiguration>(configuration.GetSection("MongoDb"));

            Persistence.Configurations.MongoDb.MongoDbConfiguration.Configure();

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

        //internal static IServiceCollection AddNotificationDatabase(
        //    this IServiceCollection services,
        //    IConfiguration configuration)
        //    => services
        //        .AddDbContext<NotificationContext>(options =>
        //        {
        //            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
        //            options.LogTo(s => System.Diagnostics.Debug.WriteLine(s));
        //            options.EnableDetailedErrors(true);
        //            options.EnableSensitiveDataLogging(true);
        //        });
    }
}
