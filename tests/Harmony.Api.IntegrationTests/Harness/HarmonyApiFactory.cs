using Harmony.Infrastructure.IntegrationTests.Harness;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

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
        }
    }
}
