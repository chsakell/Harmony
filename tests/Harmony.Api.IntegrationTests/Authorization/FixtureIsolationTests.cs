using Harmony.Api.IntegrationTests.Harness;
using Harmony.Persistence.DbContext;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// Each non-member request and its owner request run against a new fixture of their own (FR-032).
    /// </summary>
    public class FixtureIsolationTests(ApiTestHost host)
    {
        private const string Method = "DELETE";
        private const string Route = "api/CheckLists/{id:guid}";

        [Fact]
        [Trait("Requirement", "FR-032")]
        public async Task DeleteChecklist_NonMemberAndOwner_EachDeleteOnlyTheirOwnFixture()
        {
            var api = await host.GetAsync();
            var endpoint = api.Discovery().Actions.Single(action => action.Method == Method && action.Route == Route);
            var line = MatrixFile.Load().Lines.Single(line => line.Method == Method && line.Route == Route);
            var nonMemberFixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);
            var ownerFixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);
            var otherFixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);

            var nonMember = await RowRunner.RunNonMemberAsync(api, endpoint, line, nonMemberFixture, ownerFixture, TestContext.Current.CancellationToken);

            (await ChecklistExistsAsync(api, ownerFixture)).ShouldBeFalse("the owner request deletes its own fixture's checklist");
            (await ChecklistExistsAsync(api, otherFixture)).ShouldBeTrue("no request touches another fixture");
            var nonMemberWasLetIn = RowVerdict.Classify(Rule.NonMember, nonMember.NonMember) == Outcome.LetIn;
            (await ChecklistExistsAsync(api, nonMemberFixture)).ShouldBe(!nonMemberWasLetIn,
                "the non-member request acts only on its own fixture, and only when it is let in");
        }

        private static async Task<bool> ChecklistExistsAsync(ApiTestHost api, Fixture fixture)
        {
            using var scope = api.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<HarmonyContext>();
            return await context.CheckLists.AnyAsync(checkList => checkList.Id == fixture.ChecklistId, TestContext.Current.CancellationToken);
        }
    }
}
