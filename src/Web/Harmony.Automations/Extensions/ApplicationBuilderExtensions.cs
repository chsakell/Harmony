using Harmony.Application.Contracts.Persistence;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Automations.Extensions
{
    /// <summary>
    /// App builder extensions
    /// </summary>
    internal static class ApplicationBuilderExtensions
    {
        internal static void ConfigureSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", typeof(Program).Assembly.GetName().Name);
                options.RoutePrefix = "swagger";
                options.DisplayRequestDuration();
            });
        }

        internal static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app, IConfiguration _configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var notificationContext = serviceScope.ServiceProvider.GetRequiredService<NotificationContext>();

            await notificationContext.Database.MigrateAsync();
            await notificationContext.Database.EnsureCreatedAsync();

            return app;
        }

        internal static async Task<IApplicationBuilder> SeedDatabase(this IApplicationBuilder app, IConfiguration _configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var initializers = serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>();

            foreach (var initializer in initializers.OrderBy(init => init.Order))
            {
                await initializer.Initialize();
            }

            return app;
        }
    }
}
