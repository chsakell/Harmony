using System.Text.RegularExpressions;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Conventions
{
    /// <summary>
    /// The tests run the database images the deployment files run (NFR-006).
    /// </summary>
    [Trait("Requirement", "NFR-006")]
    public class ImageTagTests
    {
        private static readonly Regex ImageLine = new(@"^\s*image:\s*(?<image>\S+)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant);

        [Theory]
        [InlineData("docker-compose.yml", ImageTags.SqlServer)]
        [InlineData("docker-compose.yml", ImageTags.MongoDb)]
        [InlineData("docker-compose.postgres.yml", ImageTags.PostgreSql)]
        public void ImageTags_MatchDockerComposeFiles(string composeFile, string imageTag)
        {
            var repository = imageTag[..imageTag.LastIndexOf(':')];
            var compose = File.ReadAllText(Path.Combine(RepositoryRoot.Path, composeFile));

            var imagesOfRepository = ImageLine.Matches(compose)
                .Select(match => match.Groups["image"].Value)
                .Where(image => image.StartsWith(repository + ":", StringComparison.Ordinal))
                .Distinct()
                .ToList();

            imagesOfRepository.ShouldBe([imageTag], $"{composeFile} runs {repository} with another tag");
        }
    }
}
