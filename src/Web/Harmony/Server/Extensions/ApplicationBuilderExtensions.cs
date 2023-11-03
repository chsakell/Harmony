using Harmony.Application.Contracts.Persistence;
using Harmony.Persistence.DbContext;
using Harmony.Server.Hubs;
using Harmony.Shared.Constants.Application;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Server.Extensions
{
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

        internal static async Task<IApplicationBuilder> InitializeDatabase(this IApplicationBuilder app, Microsoft.Extensions.Configuration.IConfiguration _configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();

            var harmonyContext = serviceScope.ServiceProvider.GetRequiredService<HarmonyContext>();

            harmonyContext.Database.Migrate();
            await harmonyContext.Database.EnsureCreatedAsync();

            var initializers = serviceScope.ServiceProvider.GetServices<IDatabaseSeeder>();

            foreach (var initializer in initializers.OrderBy(init => init.Order))
            {
                await initializer.Initialize();
            }

            return app;
        }

        internal static IApplicationBuilder UseEndpoints(this IApplicationBuilder app)
            => app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapFallbackToFile("index.html");
                endpoints.MapHub<SignalRHub>(ApplicationConstants.SignalR.HubUrl);
            });
    }
}
