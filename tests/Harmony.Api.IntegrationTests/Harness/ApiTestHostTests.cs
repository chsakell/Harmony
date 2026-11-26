using System.Text;
using Harmony.Application.Configurations;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Persistence.DbContext;
using Harmony.Persistence.Identity;
using Harmony.Shared.Constants.Role;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Harness
{
    public class ApiTestHostTests(ApiTestHost host)
    {
        /// <summary>The secret committed in src/Services/Harmony.Api/appsettings.json, which the run must not use.</summary>
        private const string CommittedSecret = "JeSvMeXtj/4y0WxPOP8qX4014QO7sgBtxxJqgOYZ7YSmVuKMRz9fmhv5ZW4M7SwJ";

        [Fact]
        [Trait("Requirement", "FR-001")]
        public async Task HealthReady_HostStarted_Returns200()
        {
            var api = await host.GetAsync();
            using var client = api.CreateClient();

            using var response = await client.GetAsync("/healthz/ready", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
        }

        [Fact]
        [Trait("Requirement", "FR-001")]
        public async Task Database_HostStarted_AppliedMigrationsEqualPostgreSqlMigrations()
        {
            var api = await host.GetAsync();
            using var scope = api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HarmonyContext>();

            var applied = await context.Database.GetAppliedMigrationsAsync(TestContext.Current.CancellationToken);
            var defined = context.Database.GetMigrations().ToList();

            defined.ShouldNotBeEmpty();
            applied.ShouldBe(defined);
            context.GetService<IMigrationsAssembly>().Assembly.GetName().Name
                .ShouldBe("Harmony.Persistence.Migrations.PostgreSql");
        }

        [Fact]
        [Trait("Requirement", "FR-001")]
        public async Task Roles_HostStarted_SeededRolesExist()
        {
            var api = await host.GetAsync();
            using var scope = api.Services.CreateScope();
            var roles = scope.ServiceProvider.GetRequiredService<RoleManager<HarmonyRole>>();

            var names = await roles.Roles.Select(role => role.Name).ToListAsync(TestContext.Current.CancellationToken);

            names.ShouldContain(RoleConstants.AdministratorRole);
            names.ShouldContain(RoleConstants.BasicRole);
        }

        /// <summary>
        /// Program reads the provider and the secret from builder.Configuration before Build(), so this proves the
        /// harness settings reach that early code (NFR-003), not only the options read later.
        /// </summary>
        [Fact]
        [Trait("Requirement", "NFR-003")]
        public async Task Database_HostStarted_UsesNpgsqlAndRunSecret()
        {
            var api = await host.GetAsync();
            using var scope = api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HarmonyContext>();

            context.Database.ProviderName.ShouldBe("Npgsql.EntityFrameworkCore.PostgreSQL");
            new NpgsqlConnectionStringBuilder(context.Database.GetConnectionString()).Database.ShouldBe(api.DatabaseName);

            api.Secret.ShouldNotBe(CommittedSecret);
            api.Services.GetRequiredService<IOptions<AppConfiguration>>().Value.Secret.ShouldBe(api.Secret);
            var bearer = api.Services.GetRequiredService<IOptionsMonitor<JwtBearerOptions>>().Get(JwtBearerDefaults.AuthenticationScheme);
            bearer.TokenValidationParameters.IssuerSigningKey.ShouldBeOfType<SymmetricSecurityKey>()
                .Key.ShouldBe(Encoding.UTF8.GetBytes(api.Secret));
        }

        /// <summary>
        /// Points the host's own container start at an endpoint where no Docker daemon listens, so it fails as it
        /// does when Docker is stopped, without stopping the developer's Docker.
        /// </summary>
        [Fact]
        [Trait("Requirement", "FR-003")]
        public async Task DockerGuard_EndpointUnreachable_ThrowsMessageNamingDocker()
        {
            var exception = await Should.ThrowAsync<InvalidOperationException>(
                () => ApiTestHost.StartPostgreSqlAsync(builder => builder.WithDockerEndpoint("tcp://127.0.0.1:1")));

            exception.Message.ShouldStartWith(DockerGuard.Message);
            exception.Message.ShouldContain("Docker");
        }

        [Fact]
        [Trait("Requirement", "NFR-002")]
        public async Task Container_NameDoesNotStartWithHarmony()
        {
            var api = await host.GetAsync();

            api.ContainerName.TrimStart('/').ShouldNotStartWith("harmony-", Case.Insensitive);
        }
    }
}
