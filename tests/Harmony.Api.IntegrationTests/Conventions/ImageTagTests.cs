using System.Text.RegularExpressions;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Conventions
{
    /// <summary>
    /// The test host runs on the PostgreSQL image the deployment runs (NFR-004).
    /// </summary>
    [Trait("Requirement", "NFR-004")]
    public class ImageTagTests
    {
        private static readonly Regex ImageLine = new(@"^\s*image:\s*(?<image>\S+)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant);

        [Fact]
        public void PostgreSqlImage_MatchesDockerComposePostgres()
        {
            var compose = File.ReadAllText(Path.Combine(RepositoryRoot.Path, "docker-compose.postgres.yml"));

            var postgresImages = ImageLine.Matches(compose)
                .Select(match => match.Groups["image"].Value)
                .Where(image => image.StartsWith("postgres:", StringComparison.Ordinal))
                .Distinct()
                .ToList();

            postgresImages.ShouldBe([ImageTags.PostgreSql]);
        }
    }
}
