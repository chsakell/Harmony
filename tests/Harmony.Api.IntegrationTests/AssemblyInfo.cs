using Harmony.Api.IntegrationTests.Harness;
using Xunit;

// One PostgreSQL instance and one test host for the whole run, started the first time a test asks for it.
[assembly: AssemblyFixture(typeof(ApiTestHost))]
