using Harmony.Server.Extensions;
using Microsoft.Extensions.FileProviders;
using Harmony.Application.Extensions;
using Harmony.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddCurrentUserService();
builder.Services.RegisterSwagger();
builder.Services.AddInfrastructureMappings();
builder.Services.AddRepositories();
builder.Services.AddServerLocalization();
builder.Services.AddIdentityServices();
builder.Services.AddJwtAuthentication(builder.Services.GetApplicationSettings(builder.Configuration));
builder.Services.AddSignalR();
builder.Services.AddApplicationLayer();
builder.Services.AddApplicationServices();

var app = builder.Build();

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

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Files")),
    RequestPath = new PathString("/Files")
});

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints();
app.ConfigureSwagger();
await app.InitializeDatabase(builder.Configuration);

app.Run();
