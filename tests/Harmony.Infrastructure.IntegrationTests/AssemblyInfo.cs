using Harmony.Infrastructure.IntegrationTests.Harness;
using Xunit;

// One instance of each database engine for the whole run, started the first time a test asks for it.
[assembly: AssemblyFixture(typeof(DatabaseServers))]
