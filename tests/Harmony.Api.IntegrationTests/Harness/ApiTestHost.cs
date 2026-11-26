using System.Security.Cryptography;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Microsoft.AspNetCore.Mvc.Testing;
using Npgsql;
using Testcontainers.PostgreSql;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// The run's test host (assembly fixture). The first <see cref="GetAsync"/> starts, once: a disposable
    /// PostgreSQL container (random name and port, no volume; NFR-002), a temporary folder that becomes the process's
    /// current directory (the Api resolves its <c>Files</c> folder from it; plan, Flagged concern 3(b)), and the Api
    /// on a new database, which the Api's own startup migrates and seeds (FR-001). A run that selects only pure tests
    /// starts nothing. <see cref="DisposeAsync"/> undoes only what started.
    /// </summary>
    public sealed class ApiTestHost : IAsyncDisposable
    {
        private readonly Lazy<Task> started;
        private PostgreSqlContainer? container;
        private string? folder;
        private string? previousDirectory;
        private HarmonyApiFactory? factory;
        private string? databaseName;
        private string? secret;

        public ApiTestHost()
        {
            started = new Lazy<Task>(StartAsync);
        }

        public IServiceProvider Services => Started(factory).Services;

        /// <summary>The name of this run's database on the container.</summary>
        public string DatabaseName => Started(databaseName);

        /// <summary>The JWT signing secret generated for this run (FR-007).</summary>
        public string Secret => Started(secret);

        public string ContainerName => Started(container).Name;

        /// <summary>The temporary folder that is the current directory while the host runs.</summary>
        public string Folder => Started(folder);

        /// <summary>
        /// Starts a PostgreSQL container through <see cref="DockerGuard"/>; <paramref name="configure"/> lets a test
        /// point it at an unreachable Docker endpoint (FR-003).
        /// </summary>
        public static Task<PostgreSqlContainer> StartPostgreSqlAsync(Func<PostgreSqlBuilder, PostgreSqlBuilder>? configure = null) =>
            DockerGuard.StartAsync(() =>
            {
                var builder = new PostgreSqlBuilder(ImageTags.PostgreSql);
                return (configure is null ? builder : configure(builder)).Build();
            });

        public async Task<ApiTestHost> GetAsync()
        {
            await started.Value;
            return this;
        }

        /// <summary>
        /// A client for the in-memory server. HTTPS base address and no redirects, so UseHttpsRedirection never
        /// answers in place of the endpoint.
        /// </summary>
        public HttpClient CreateClient() => Started(factory).CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            AllowAutoRedirect = false,
        });

        public async ValueTask DisposeAsync()
        {
            if (factory is not null)
            {
                await factory.DisposeAsync();
            }

            if (previousDirectory is not null)
            {
                Directory.SetCurrentDirectory(previousDirectory);
            }

            if (folder is not null && Directory.Exists(folder))
            {
                Directory.Delete(folder, recursive: true);
            }

            if (container is not null)
            {
                await container.DisposeAsync();
            }
        }

        private async Task StartAsync()
        {
            container = await StartPostgreSqlAsync();

            folder = Path.Combine(Path.GetTempPath(), "h-api-" + Guid.NewGuid().ToString("N")[..8]);
            Directory.CreateDirectory(Path.Combine(folder, "Files"));
            previousDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(folder);

            databaseName = "h_" + Guid.NewGuid().ToString("N")[..8];
            var connectionString = new NpgsqlConnectionStringBuilder(container.GetConnectionString()) { Database = databaseName }.ConnectionString;
            secret = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            factory = new HarmonyApiFactory(connectionString, secret);

            // Services starts the server: Program runs to app.Run(), migrating and seeding the database on the way.
            await Task.Run(() => factory.Services);
        }

        private T Started<T>(T? value)
            where T : class =>
            value ?? throw new InvalidOperationException("The test host has not started. Call GetAsync first.");
    }
}
