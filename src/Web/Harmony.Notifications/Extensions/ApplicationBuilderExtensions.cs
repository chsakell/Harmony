using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Notifications.Extensions
{
    /// <summary>
    /// App builder extensions
    /// </summary>
    internal static class ApplicationBuilderExtensions
    {
        internal static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app, IConfiguration _configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var notificationContext = serviceScope.ServiceProvider.GetRequiredService<NotificationContext>();

            await notificationContext.Database.MigrateAsync();
            await notificationContext.Database.EnsureCreatedAsync();

            return app;
        }
    }
}
