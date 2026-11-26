using System.Reflection;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Infrastructure.IntegrationTests.Conventions
{
    /// <summary>
    /// The MongoDB stores use fixed database names, so MongoDB tests run one at a time in one collection that runs
    /// in parallel with no other collection (NFR-003).
    /// </summary>
    [Trait("Requirement", "NFR-003")]
    public class MongoDbCollectionConventionTests
    {
        [Fact]
        public void MongoDbTests_AllInOneNonParallelCollection()
        {
            typeof(MongoDbCollection).GetCustomAttribute<CollectionDefinitionAttribute>()
                .ShouldNotBeNull().DisableParallelization.ShouldBeTrue();

            var mongoDbTestClasses = typeof(MongoDbCollectionConventionTests).Assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract)
                .Where(type => type.GetConstructors().Any(constructor =>
                    constructor.GetParameters().Any(parameter => parameter.ParameterType == typeof(MongoDbFixture))))
                .ToList();

            mongoDbTestClasses.ShouldNotBeEmpty();
            mongoDbTestClasses
                .Where(type => type.GetCustomAttribute<CollectionAttribute>()?.Name != MongoDbCollection.Name)
                .Select(type => type.Name)
                .ShouldBeEmpty($"MongoDB test classes need [Collection(MongoDbCollection.Name)]");
        }
    }
}
