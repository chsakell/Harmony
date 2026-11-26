using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// All MongoDB tests: the stores use fixed database names (harmony_automation, harmony_source_control), so the
    /// tests run one at a time and drop those databases before each test (NFR-003). Checked by
    /// MongoDbCollectionConventionTests.
    /// </summary>
    [CollectionDefinition(Name, DisableParallelization = true)]
    public sealed class MongoDbCollection : ICollectionFixture<MongoDbFixture>
    {
        public const string Name = "MongoDB";
    }
}
