using System.Reflection;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Shouldly;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

namespace Harmony.Infrastructure.IntegrationTests.Conventions
{
    /// <summary>
    /// Every relational test runs once per provider, with the provider in its display name (FR-005). A relational
    /// test takes a <see cref="RelationalProvider"/> or is in a class that uses <see cref="RelationalDatabaseFixture"/>.
    /// </summary>
    [Trait("Requirement", "FR-005")]
    public class HarnessConventionTests
    {
        [Fact]
        public async Task RelationalTests_EveryTestMethod_RunsOnBothProviders()
        {
            var rows = await new BothProvidersAttribute().GetData(
                typeof(HarnessConventionTests).GetMethod(nameof(RelationalTests_EveryTestMethod_RunsOnBothProviders))!,
                new DisposalTracker());
            rows.Select(row => row.GetData().Single()).ShouldBe(
                [RelationalProvider.SqlServer, RelationalProvider.PostgreSql], ignoreOrder: true);

            var violations = FindViolations(typeof(HarnessConventionTests).Assembly.GetTypes());

            violations.ShouldBeEmpty(
                $"A relational test needs [{nameof(BothProvidersAttribute)[..^"Attribute".Length]}] and no other data attribute: "
                + string.Join(", ", violations));
        }

        private static List<string> FindViolations(IEnumerable<Type> types) =>
            types
                .Where(type => type.IsClass && !type.IsAbstract)
                .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                    .Where(method => method.IsDefined(typeof(FactAttribute), inherit: true))
                    .Where(method => UsesRelationalDatabaseFixture(type) || TakesProvider(method))
                    .Where(method => !TakesProvider(method) || !RunsOnBothProvidersOnly(method))
                    .Select(method => $"{type.Name}.{method.Name}"))
                .ToList();

        private static bool UsesRelationalDatabaseFixture(Type type) =>
            type.GetInterfaces().Contains(typeof(IClassFixture<RelationalDatabaseFixture>));

        private static bool TakesProvider(MethodInfo method) =>
            method.GetParameters().Any(parameter => parameter.ParameterType == typeof(RelationalProvider));

        private static bool RunsOnBothProvidersOnly(MethodInfo method)
        {
            var dataAttributes = method.GetCustomAttributes(inherit: true).OfType<IDataAttribute>().ToList();
            return dataAttributes.Count == 1 && dataAttributes[0] is BothProvidersAttribute;
        }
    }
}
