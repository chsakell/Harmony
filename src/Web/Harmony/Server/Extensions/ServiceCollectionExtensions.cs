using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Server.Extensions
{
    /// <summary>
    /// Extension class to register services per category
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddDatabase(
            this IServiceCollection services,
            IConfiguration configuration)
            => services
                .AddDbContext<HarmonyContext>(options => options
                    .UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
            //.AddTransient<IDatabaseSeeder, DatabaseSeeder>();
    }
}
