using System.Reflection;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Harmony.Infrastructure.IntegrationTests.Harness
{
    /// <summary>
    /// Runs a relational theory once per provider (FR-005). The theory takes one <see cref="RelationalProvider"/>
    /// parameter, so its display name ends with <c>(provider: SqlServer)</c> or <c>(provider: PostgreSql)</c>.
    /// HarnessConventionTests checks that every relational test uses it.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class BothProvidersAttribute : DataAttribute
    {
        private static readonly RelationalProvider[] Providers = [RelationalProvider.SqlServer, RelationalProvider.PostgreSql];

        public override ValueTask<IReadOnlyCollection<ITheoryDataRow>> GetData(MethodInfo testMethod, DisposalTracker disposalTracker) =>
            new(Providers.Select(provider => (ITheoryDataRow)new TheoryDataRow<RelationalProvider>(provider)).ToList());

        public override bool SupportsDiscoveryEnumeration() => true;
    }
}
