using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;


namespace Harmony.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructureMappings(this IServiceCollection services)
        {
            services.AddAutoMapper(Assembly.GetExecutingAssembly());
        }
    }
}