using Harmony.Application.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Extensions.DiagnosticSources;

namespace Harmony.Infrastructure.Extensions
{
    public static class MongoDbClientExtensions
    {
        /// <summary>
        /// ActivitySource name emitted by <see cref="DiagnosticsActivityEventSubscriber"/>; registered for tracing
        /// in Harmony.ServiceDefaults.
        /// </summary>
        public const string ActivitySourceName = "MongoDB.Driver.Core.Extensions.DiagnosticSources";

        /// <summary>
        /// Registers a single, thread-safe <see cref="IMongoClient"/> built from <c>MongoDB:ConnectionURI</c>.
        /// MongoDB recommends one client per application; the repositories used to create a new client per scope.
        /// The client publishes OpenTelemetry activities for every command.
        /// </summary>
        public static IServiceCollection AddHarmonyMongoClient(this IServiceCollection services)
        {
            services.TryAddSingleton<IMongoClient>(sp =>
            {
                var configuration = sp.GetRequiredService<IOptions<MongoDbConfiguration>>().Value;
                var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("Harmony.MongoDB");

                var settings = MongoClientSettings.FromUrl(new MongoUrl(configuration.ConnectionURI));
                settings.ConnectTimeout = TimeSpan.FromSeconds(10);
                settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
                settings.ClusterConfigurator = cb =>
                {
                    cb.Subscribe(new DiagnosticsActivityEventSubscriber(
                        new InstrumentationOptions { CaptureCommandText = true }));

                    if (logger.IsEnabled(LogLevel.Debug))
                    {
                        cb.Subscribe<CommandStartedEvent>(e =>
                            logger.LogDebug("{CommandName} - {Command}", e.CommandName, e.Command.ToJson()));
                    }
                };

                return new MongoClient(settings);
            });

            return services;
        }
    }
}
