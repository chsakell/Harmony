using Microsoft.Extensions.FileProviders;
using Harmony.Application.Extensions;
using Harmony.Infrastructure.Extensions;
using Harmony.Application.Enums;
using Harmony.Api.Extensions;
using Harmony.Api.Services.gRPC;
using Harmony.Application.Configurations;
using System.Reflection;
using Harmony.Api.Mappings;
using Harmony.Logging;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(SeriLogger.Configure);

// Add services to the container.
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});


builder.Services.AddRetryPolicies();
builder.Services.AddEndpointConfiguration(builder.Configuration);

var endpointConfiguration =
    builder.Configuration.GetSection(nameof(AppEndpointConfiguration))
    .Get<AppEndpointConfiguration>();

builder.Services.AddCors(
    options => options.AddPolicy(
        "wasm",
        policy => policy.WithOrigins(endpointConfiguration.FrontendUrl)
            .AllowAnyMethod()
            .SetIsOriginAllowed(pol => true)
            .AllowAnyHeader()
            .AllowCredentials()));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddCurrentUserService();

builder.Services.AddEndpointsApiExplorer();
builder.Services.RegisterSwagger();
builder.Services.AddInfrastructureMappings();
builder.Services.AddSqlServerRepositories();
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Services.GetApplicationSettings(builder.Configuration));

builder.Services.AddSignalR();
builder.Services.AddClientNotificationService();
builder.Services.AddApplicationLayer();

builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly(), typeof(AutomationProfile).Assembly);
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

builder.Services.AddApplicationServices();
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddMongoDb(builder.Configuration);

// gRPC services
builder.Services.AddGrpc();

// Configure your Search Engine
builder.Services.AddSearching(SearchEngine.Database, builder.Configuration);
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseCors("wasm");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

//app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Files")),
    RequestPath = new PathString("/Files")
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapGrpcService<CardService>();
app.MapGrpcService<UserCardService>();
app.MapGrpcService<BoardService>();
app.MapGrpcService<WorkspaceService>();
app.MapGrpcService<UserService>();
app.MapGrpcService<UserNotificationService>();

app.UseEndpoints();
app.ConfigureSwagger();
await app.InitializeDatabase(builder.Configuration);

app.Run();
