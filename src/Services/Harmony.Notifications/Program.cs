using Hangfire;
using Harmony.Application.Configurations;
using Harmony.Application.Extensions;
using Harmony.Logging;
using Harmony.Messaging;
using Harmony.Notifications.Contracts.Notifications.Email;
using Harmony.Notifications.Extensions;
using Harmony.Notifications.Services.EmailProviders;
using Harmony.Notifications.Services.Hosted;
using Harmony.Persistence.DbContext;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;

namespace Harmony.Notifications
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Host.UseSerilog(SeriLogger.Configure);

            builder.Services.AddEndpointConfiguration(builder.Configuration);

            // Add DbContexts
            builder.Services.AddDatabase(builder.Configuration);

            builder.Services.Configure<BrokerConfiguration>(builder.Configuration.GetSection("BrokerConfiguration"));

            // Email service providers
            builder.Services.Configure<GmailSettings>
                        (options => builder.Configuration
                        .GetSection("GmailSettings").Bind(options));

            // Choose below your email provider
            //builder.Services.AddSingleton<IEmailService, GmailEmailService>();
            builder.Services.AddSingleton<IEmailService, BrevoEmailService>();

            builder.Services.ConfigureBrevo(builder.Configuration);

            builder.Services.AddHttpClient();

            builder.Services.AddRetryPolicies();

            // Add services to the container.
            builder.Services.AddRazorPages();
            
            builder.Services.AddEmailNotificationServices();
            builder.Services.AddSearchIndexNotificationServices(builder.Configuration);

            // Add Hangfire services.
            builder.Services.AddHangFire(builder.Configuration);

            // Add the processing server as IHostedService
            builder.Services.AddHangfireServer();
            builder.Services.AddHostedService<EmailNotificationsConsumerHostedService>();
            builder.Services.AddHostedService<SearchIndexNotificationsConsumerHostedService>();
            builder.Services.AddMemoryCache();

            builder.Services.AddSingleton<RabbitMqHealthCheck>();
            builder.Services.AddHealthChecks()
                .AddDbContextCheck<NotificationContext>("database", tags: ["ready"])
                .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"]);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            await app.InitializeDatabase(builder.Configuration);

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseHangfireDashboard();

            app.UseRouting();

            app.MapHealthChecks("/healthz/ready", new HealthCheckOptions
            {
                Predicate = healthCheck => healthCheck.Tags.Contains("ready")
            });

            app.MapHealthChecks("/healthz/live", new HealthCheckOptions
            {
                Predicate = _ => true
            });

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}
