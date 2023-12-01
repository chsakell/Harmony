using Hangfire;
using Harmony.Application.Configurations;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Extensions;
using Harmony.Notifications.Services;
using Microsoft.Extensions.Configuration;

namespace Harmony.Notifications
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContexts
            builder.Services.AddIdentityServices();
            builder.Services.AddHarmonyDatabase(builder.Configuration);
            builder.Services.AddNotificationDatabase(builder.Configuration);

            builder.Services.AddRepositories();
            builder.Services.AddApplicationServices();
            builder.Services.Configure<BrokerConfiguration>(builder.Configuration.GetSection("BrokerConfiguration"));
            
            builder.Services.Configure<GmailSettings>
                        (options => builder.Configuration
                        .GetSection("GmailSettings").Bind(options));

            // Add services to the container.
            builder.Services.AddRazorPages();
            builder.Services.AddSingleton<IEmailNotificationService, GmailNotificationService>();
            builder.Services.AddNotificationServices();

            // Add Hangfire services.
            builder.Services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer();
            builder.Services.AddHostedService<NotificationsConsumerHostedService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseHangfireDashboard();

            app.UseRouting();

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
