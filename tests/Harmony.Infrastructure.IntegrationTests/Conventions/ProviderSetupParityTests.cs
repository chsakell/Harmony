using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Conventions
{
    /// <summary>
    /// The harness configures each provider as the host that owns the context does (NFR-005). The hosts'
    /// AddDatabase methods are internal, so the harness keeps a copy in <see cref="ProviderSetup"/> and this test
    /// reads each host's source to keep the two equal. If a host gains a setting, add it to ProviderSetup, then here.
    /// </summary>
    [Trait("Requirement", "NFR-005")]
    public class ProviderSetupParityTests
    {
        /// <summary>
        /// Settings that would change how queries or migrations behave. None of the hosts uses them on 2026-11-26.
        /// </summary>
        private static readonly string[] SettingsTheHarnessDoesNotCopy =
        [
            "EnableRetryOnFailure",
            "UseQuerySplittingBehavior",
            "AddInterceptors",
            "UseCollation",
            "ConfigureWarnings",
        ];

        [Theory]
        [InlineData("Harmony.Api", "HarmonyContext")]
        [InlineData("Harmony.Automations", "AutomationContext")]
        [InlineData("Harmony.Notifications", "NotificationContext")]
        public void HostDatabaseSetup_MatchesHarness(string host, string context)
        {
            var source = Compact(File.ReadAllText(
                Path.Combine(RepositoryRoot.Path, "src", "Services", host, "Extensions", "ServiceCollectionExtensions.cs")));

            source.ShouldContain(Compact($"AddDbContext<{context}>"), Case.Sensitive);
            source.ShouldContain(Compact($"UseSqlServer("), Case.Sensitive);
            source.ShouldContain(Compact($"MigrationsAssembly(\"{ProviderSetup.SqlServerMigrationsAssembly}\")"), Case.Sensitive);
            source.ShouldContain(Compact($"UseNpgsql("), Case.Sensitive);
            source.ShouldContain(Compact($"MigrationsAssembly(\"{ProviderSetup.PostgreSqlMigrationsAssembly}\")"), Case.Sensitive);
            source.ShouldContain(Compact($"AppContext.SetSwitch(\"{ProviderSetup.LegacyTimestampSwitch}\", true)"), Case.Sensitive);
            SettingsTheHarnessDoesNotCopy.ShouldAllBe(setting => !source.Contains(setting));
        }

        private static string Compact(string text) => string.Concat(text.Where(character => !char.IsWhiteSpace(character)));
    }
}
