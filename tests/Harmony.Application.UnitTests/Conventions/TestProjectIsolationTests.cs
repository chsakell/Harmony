using Shouldly;
using Xunit;

namespace Harmony.Application.UnitTests.Conventions
{
    /// <summary>
    /// Unit tests must run without Docker, databases, a broker or the network, so this project may
    /// reference the Application layer only - never infrastructure or a service host.
    /// </summary>
    [Trait("Requirement", "NFR-002")]
    public class TestProjectIsolationTests
    {
        private static readonly string[] ForbiddenAssemblies =
        [
            "Harmony.Persistence*",
            "Harmony.Infrastructure",
            "Harmony.Messaging",
            "Harmony.Caching",
            "Harmony.Api",
            "Harmony.Automations",
            "Harmony.Notifications",
            "Harmony.SignalR",
            "Harmony.Integrations.*",
        ];

        [Fact]
        public void TestAssembly_References_NoInfrastructureOrServiceAssembly()
        {
            // Everything the build copies next to the test assembly: direct and transitive references.
            var referenced = Directory.GetFiles(AppContext.BaseDirectory, "Harmony.*.dll")
                .Select(Path.GetFileNameWithoutExtension)
                .OfType<string>();

            var forbidden = referenced.Where(IsForbidden).Order().ToList();

            forbidden.ShouldBeEmpty(
                "Harmony.Application.UnitTests must not reference infrastructure or service assemblies: "
                + string.Join(", ", forbidden));
        }

        private static bool IsForbidden(string assemblyName) =>
            ForbiddenAssemblies.Any(pattern => pattern.EndsWith('*')
                ? assemblyName.StartsWith(pattern.TrimEnd('*'), StringComparison.Ordinal)
                : assemblyName == pattern);
    }
}
