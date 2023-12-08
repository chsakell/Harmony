using Hangfire;
using Harmony.Application.Configurations;
using Harmony.Infrastructure.Extensions;
using Harmony.Notifications.Contracts;
using Harmony.Notifications.Extensions;
using Harmony.Notifications.Services.EmailProviders;

namespace Harmony.Notifications
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContexts
            builder.Services.AddIdentityServices();
            builder.Services.AddHarmonyDatabase(builder.Configuration);
            builder.Services.AddNotificationDatabase(builder.Configuration);

            builder.Services.AddRepositories();
            builder.Services.AddApplicationServices();
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

            // Add services to the container.
            builder.Services.AddRazorPages();
            
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


            await app.InitializeDatabase(builder.Configuration);

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
