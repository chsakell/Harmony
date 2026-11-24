using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// OpenTelemetry (Ocelot downstream calls are traced through HttpClient instrumentation), health checks
builder.AddServiceDefaults();

builder.Services.AddCors();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOcelot();
builder.Services.AddSwaggerForOcelot(builder.Configuration);

builder.Configuration.AddJsonFile("ocelot.json");

var app = builder.Build();

app.UseCors(builder => builder
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials());

app.UseHttpsRedirection();
app.UseAuthorization();
app.UseSwaggerForOcelotUI(option =>
{
    option.PathToSwaggerGenerator = "/swagger/docs";
});
app.MapControllers();

app.UseWebSockets();

// Must run before Ocelot, which terminates the pipeline
app.UseDefaultHealthChecks();

await app.UseOcelot();

await app.RunAsync();
