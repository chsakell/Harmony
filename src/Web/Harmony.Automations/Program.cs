using Harmony.Application.Configurations;
using Harmony.Automations.Extensions;
using Harmony.Infrastructure.Extensions;
using Harmony.Notifications.Services.Hosted;

namespace Harmony.Automations
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContexts
            builder.Services.AddIdentityServices();
            builder.Services.AddHarmonyDatabase(builder.Configuration);
            //builder.Services.AddNotificationDatabase(builder.Configuration);

            builder.Services.AddRepositories();
            builder.Services.AddApplicationServices();
            builder.Services.Configure<BrokerConfiguration>(builder.Configuration.GetSection("BrokerConfiguration"));
            builder.Services.AddAutomationServices();

            builder.Services.AddMongoDb(builder.Configuration);
            builder.Services.AddMessaging(builder.Configuration);

            // Add Hangfire services.
            //builder.Services.AddHangfire(configuration => configuration
            //    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            //    .UseSimpleAssemblyNameTypeSerializer()
            //    .UseRecommendedSerializerSettings()
            //    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            //// Add the processing server as IHostedService
            //builder.Services.AddHangfireServer();
            builder.Services.AddHostedService<AutomationNotificationsConsumerHostedService>();
            builder.Services.AddMemoryCache();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            // app.UseHangfireDashboard();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
