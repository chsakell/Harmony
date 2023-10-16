using Harmony.Application.Contracts.Repositories;
using Harmony.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Harmony.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureMappings(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            return services
                .AddScoped<IWorkspaceRepository, WorkspaceRepository>()
                .AddScoped<IUserWorkspaceRepository, UserWorkspaceRepository>()
                .AddScoped<IBoardRepository, BoardRepository>()
                .AddScoped<IUserBoardRepository, UserBoardRepository>()
                .AddScoped<IBoardListRepository, BoardListRepository>()
				.AddScoped<ICardRepository, CardRepository>()
                .AddScoped<IChecklistRepository, ChecklistRepository>()
                .AddScoped<ICheckListItemRepository, CheckListItemRepository>()
                .AddScoped<IBoardLabelRepository, BoardLabelRepository>()
                .AddScoped<ICardLabelRepository, CardLabelRepository>()
                .AddScoped<ICardActivityRepository, CardActivityRepository>()
                .AddScoped<IUserCardRepository, UserCardRepository>();
		}
    }
}