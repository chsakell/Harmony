using Harmony.Shared.Constants.Application;
using Harmony.SignalR.Extensions;
using Harmony.SignalR.Hubs;
using Harmony.SignalR.Services.Hosted;
using Harmony.Application.Extensions;
using Harmony.Logging;
using Serilog;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Harmony.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);

builder.Services.AddClientNotificationServices();
builder.Services.AddMessaging(builder.Configuration);

builder.Services.AddHostedService<PushNotificationsConsumerHostedService>();

// Add services to the container.
builder.Services.AddCors();

builder.Services.AddSignalR(builder.Configuration);

builder.Services.AddControllersWithViews();

builder.Services.AddRetryPolicies();

builder.Services.AddSingleton<RabbitMqHealthCheck>();
builder.Services.AddHealthChecks()
    .AddCheck<RabbitMqHealthCheck>("rabbitmq", tags: ["ready"]);

var app = builder.Build();

app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials()
            );
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

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

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<SignalRHub>(ApplicationConstants.SignalR.HubUrl);

app.Run();
