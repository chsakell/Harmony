using Harmony.Application.Configurations;
using Harmony.Notifications.Services.Hosted;
using Harmony.Server.SignalR.Extensions;
using Harmony.Server.SignalR.Hubs;
using Harmony.Shared.Constants.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BrokerConfiguration>(builder.Configuration.GetSection("BrokerConfiguration"));

builder.Services.AddClientNotificationService();
builder.Services.AddHostedService<PushNotificationsConsumerHostedService>();

// Add services to the container.
builder.Services.AddCors();
builder.Services.AddSignalR();
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
