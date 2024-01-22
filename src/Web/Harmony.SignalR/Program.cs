using Harmony.Application.Configurations;
using Harmony.Shared.Constants.Application;
using Harmony.SignalR.Extensions;
using Harmony.SignalR.Hubs;
using Harmony.SignalR.Services.Hosted;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddClientNotificationService();
builder.Services.AddMessaging(builder.Configuration);

builder.Services.AddHostedService<PushNotificationsConsumerHostedService>();

// Add services to the container.
builder.Services.AddCors();

builder.Services.AddSignalR(builder.Configuration);

builder.Services.AddControllersWithViews();

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

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<SignalRHub>(ApplicationConstants.SignalR.HubUrl);

app.Run();
