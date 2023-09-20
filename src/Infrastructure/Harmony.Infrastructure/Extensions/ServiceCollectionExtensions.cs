using Harmony.Application.Contracts.Repositories;
using Harmony.Infrastructure.Repositories;
using MediatR;
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
                .AddTransient<IWorkspaceRepository, WorkspaceRepository>()
                .AddTransient<IBoardRepository, BoardRepository>()
				.AddTransient<IBoardListRepository, BoardListRepository>()
				.AddTransient<ICardRepository, CardRepository>();
		}
    }
}