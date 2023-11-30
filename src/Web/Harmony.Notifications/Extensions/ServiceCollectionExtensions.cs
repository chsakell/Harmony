using Harmony.Application.Configurations;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Persistence;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Account;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Infrastructure.Repositories;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;


namespace Harmony.Server.Extensions
{
    /// <summary>
    /// Extension class to register services per category
    /// </summary>
    public static class ServiceCollectionExtensions
    {


        internal static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            

			return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                //.AddScoped<IWorkspaceRepository, WorkspaceRepository>()
                //.AddScoped<IUserWorkspaceRepository, UserWorkspaceRepository>()
                //.AddScoped<IBoardRepository, BoardRepository>()
                //.AddScoped<IUserBoardRepository, UserBoardRepository>()
                //.AddScoped<IBoardListRepository, BoardListRepository>()
                .AddScoped<ICardRepository, CardRepository>();
                //.AddScoped<ICheckListRepository, CheckListRepository>()
                //.AddScoped<ICheckListItemRepository, CheckListItemRepository>()
                //.AddScoped<IBoardLabelRepository, BoardLabelRepository>()
                //.AddScoped<ICardLabelRepository, CardLabelRepository>()
                //.AddScoped<ICardActivityRepository, CardActivityRepository>()
                //.AddScoped<IUserCardRepository, UserCardRepository>()
                //.AddScoped<IIssueTypeRepository, IssueTypeRepository>()
                //.AddScoped<ISprintRepository, SprintRepository>();
        }


        //internal static IServiceCollection AddIdentityServices(this IServiceCollection services)
        //{
        //    services
        //        .AddIdentity<HarmonyUser, HarmonyRole>(options =>
        //        {
        //            options.Password.RequiredLength = 6;
        //            options.Password.RequireDigit = false;
        //            options.Password.RequireLowercase = false;
        //            options.Password.RequireNonAlphanumeric = false;
        //            options.Password.RequireUppercase = false;
        //            options.User.RequireUniqueEmail = true;
        //        })
        //        .AddEntityFrameworkStores<HarmonyContext>()
        //        .AddDefaultTokenProviders();

        //    return services;
        //}

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
    }
}
