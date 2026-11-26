using Harmony.Application.Contracts.Messaging;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Integrations.SourceControl.Protos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NSubstitute;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Harmony.Api's own Program, run in memory. It differs from a deployed Api only by the overrides in NFR-003.
    /// They are host settings (<c>UseSetting</c>) because Program reads the provider, the connection string, the
    /// secret and the endpoints from builder.Configuration before Build(), when only host settings are visible.
    /// </summary>
    public sealed class HarmonyApiFactory(string connectionString, string secret) : WebApplicationFactory<Program>
    {
        /// <summary>Not Development, so the deployed pipeline (exception handler, HSTS) applies.</summary>
        public const string EnvironmentName = "Testing";

        private IServiceCollection? services;

        /// <summary>Stands in for the RabbitMQ publisher (FR-006).</summary>
        public RecordingNotificationsPublisher Publisher { get; } = new();

        /// <summary>Stands in for the SourceControl gRPC client (FR-006); nothing in the Api calls it today.</summary>
        public SourceControlService.SourceControlServiceClient SourceControlClient { get; } =
            Substitute.For<SourceControlService.SourceControlServiceClient>();

        /// <summary>The host's service registrations after the test overrides, for checks on what is registered.</summary>
        public IReadOnlyList<ServiceDescriptor> ServiceDescriptors =>
            services?.ToList() ?? throw new InvalidOperationException("The host has not been built.");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            // WebApplicationFactory looks for MvcTestingAppManifest.json in the current directory, which the harness
            // has moved to the run's temporary folder, so the content root is given explicitly.
            builder.UseContentRoot(Path.Combine(RepositoryRoot.Path, "src", "Services", "Harmony.Api"));
            builder.UseEnvironment(EnvironmentName);
            builder.UseSetting("ConnectionStrings:HarmonyConnection", connectionString);
            builder.UseSetting("DatabaseProvider", "PostgreSQL");
            builder.UseSetting("AppConfiguration:Secret", secret);
            builder.UseSetting("ElasticConfiguration:Uri", string.Empty);
            builder.UseSetting("CacheProvider", "InMemory");
            builder.UseSetting("OTEL_EXPORTER_OTLP_ENDPOINT", string.Empty);

            builder.ConfigureTestServices(testServices =>
            {
                testServices.RemoveAll<INotificationsPublisher>();
                testServices.AddSingleton<INotificationsPublisher>(Publisher);
                testServices.RemoveAll<SourceControlService.SourceControlServiceClient>();
                testServices.AddSingleton(SourceControlClient);
                services = testServices;
            });
        }
    }
}
