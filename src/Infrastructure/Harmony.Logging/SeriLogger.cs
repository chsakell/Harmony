using Microsoft.Extensions.Hosting;
using Serilog.Sinks.Elasticsearch;
using Serilog;

namespace Harmony.Logging
{
    public static class SeriLogger
    {
        /// <summary>
        /// Serilog configuration shared by all Harmony services.
        /// Use with <c>builder.Host.UseSerilog(SeriLogger.Configure, writeToProviders: true)</c> so that events are also
        /// forwarded to the OpenTelemetry logging provider registered by Harmony.ServiceDefaults (Aspire dashboard / OTLP).
        /// The Elasticsearch sink is optional and only enabled when <c>ElasticConfiguration:Uri</c> is set.
        /// </summary>
        public static Action<HostBuilderContext, LoggerConfiguration> Configure =>
           (context, configuration) =>
           {
               var applicationName = context.HostingEnvironment.ApplicationName;
               var environmentName = context.HostingEnvironment.EnvironmentName;

               configuration
                    .Enrich.FromLogContext()
                    .Enrich.WithMachineName()
                    .Enrich.WithProperty("Environment", environmentName)
                    .Enrich.WithProperty("Application", applicationName)
                    .WriteTo.Debug()
                    .WriteTo.Console()
                    .ReadFrom.Configuration(context.Configuration);

               var elasticUri = context.Configuration["ElasticConfiguration:Uri"];

               if (string.IsNullOrWhiteSpace(elasticUri))
               {
                   return;
               }

               configuration.WriteTo.Elasticsearch(
                   new ElasticsearchSinkOptions(new Uri(elasticUri))
                   {
                       IndexFormat = $"{applicationName?.ToLower().Replace(".", "-")}-{environmentName?.ToLower().Replace(".", "-")}-{DateTime.UtcNow:yyyy-MM}",
                       AutoRegisterTemplate = true,
                       NumberOfShards = 2,
                       NumberOfReplicas = 1
                   });
           };
    }
}
