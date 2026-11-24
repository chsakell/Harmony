using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Microsoft.Extensions.Hosting;

/// <summary>
/// Common services for every Harmony host: OpenTelemetry, service discovery, HTTP resilience and health checks.
/// See https://aka.ms/dotnet/aspire/service-defaults
/// </summary>
public static class Extensions
{
    // Harmony services already expose these endpoints (used by Kubernetes probes and the Aspire AppHost).
    private const string ReadinessEndpointPath = "/healthz/ready";
    private const string LivenessEndpointPath = "/healthz/live";

    /// <summary>
    /// Trace sources emitted by libraries Harmony uses, in addition to ASP.NET Core / HttpClient / EF Core.
    /// </summary>
    private static readonly string[] AdditionalTraceSources =
    [
        // RabbitMQ.Client 7 has built-in tracing and propagates the trace context through message headers.
        "RabbitMQ.Client.Publisher",
        "RabbitMQ.Client.Subscriber",
        // MongoDB.Driver.Core.Extensions.DiagnosticSources (see Harmony.Infrastructure MongoDbClientExtensions).
        "MongoDB.Driver.Core.Extensions.DiagnosticSources",
        // SignalR hub method invocations (ASP.NET Core 9+).
        "Microsoft.AspNetCore.SignalR.Server",
    ];

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            // Resilience for every IHttpClientFactory client (including the gRPC clients).
            // Retries are disabled for POST/PUT/PATCH/DELETE so that gRPC calls and commands are never replayed.
            http.AddStandardResilienceHandler(options => options.Retry.DisableForUnsafeHttpMethods());

            // Service discovery: absolute URLs keep working, "https+http://api" style URLs resolve through Aspire.
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddSource(AdditionalTraceSources)
                    .AddAspNetCoreInstrumentation(options =>
                        // Exclude health check probes from tracing
                        options.Filter = context => !IsHealthCheckRequest(context))
                    // Covers outgoing HTTP, gRPC client calls and Ocelot downstream requests.
                    .AddHttpClientInstrumentation(options =>
                        options.FilterHttpRequestMessage = request =>
                            request.RequestUri is null || !request.RequestUri.AbsolutePath.StartsWith("/healthz"))
                    // SQL Server and PostgreSQL queries issued through EF Core.
                    .AddEntityFrameworkCoreInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static bool IsHealthCheckRequest(HttpContext context) =>
        context.Request.Path.StartsWithSegments(ReadinessEndpointPath)
        || context.Request.Path.StartsWithSegments(LivenessEndpointPath);

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            // Add a default liveness check to ensure app is responsive
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    /// <summary>
    /// Adds <c>/healthz/ready</c> and <c>/healthz/live</c> as terminal middleware (not endpoint routing), for hosts whose
    /// pipeline ends in a terminal middleware such as Ocelot, where mapped endpoints would never execute.
    /// Call it before that middleware.
    /// </summary>
    public static IApplicationBuilder UseDefaultHealthChecks(this IApplicationBuilder app)
    {
        app.UseHealthChecks(ReadinessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live") || r.Tags.Contains("ready")
        });

        app.UseHealthChecks(LivenessEndpointPath, new HealthCheckOptions
        {
            Predicate = r => r.Tags.Contains("live")
        });

        return app;
    }
}
