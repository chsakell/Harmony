using Harmony.Api.IntegrationTests.Harness;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The admin rule: three rows per administration endpoint, each sent as the anonymous rule's request (new random
    /// IDs, <c>{}</c> as the body) with a logged-in caller's harness token. The plain user (FR-110) and the user with
    /// every permission except the line's required one (FR-111) are refused with 403; the user with only the required
    /// permission is let in (FR-112), which guards against a policy that asks for the wrong permission. Each row is its
    /// own result named after the rule, the caller, the method and the route (spec #22, FR-014).
    /// </summary>
    public class AdminMatrixTests(ApiTestHost host)
    {
        [Theory]
        [Trait("Requirement", "FR-110")]
        [MemberData(nameof(MatrixRows.AdminPlainUser), MemberType = typeof(MatrixRows))]
        public Task AdminRequest_PlainUser_Returns403(string method, string route) =>
            RunAsync(AdminCaller.PlainUser, method, route);

        [Theory]
        [Trait("Requirement", "FR-111")]
        [MemberData(nameof(MatrixRows.AdminAllButRequired), MemberType = typeof(MatrixRows))]
        public Task AdminRequest_AllButRequiredPermission_Returns403(string method, string route) =>
            RunAsync(AdminCaller.AllButRequired, method, route);

        [Theory]
        [Trait("Requirement", "FR-112")]
        [MemberData(nameof(MatrixRows.AdminOnlyRequired), MemberType = typeof(MatrixRows))]
        public Task AdminRequest_OnlyRequiredPermission_IsLetIn(string method, string route) =>
            RunAsync(AdminCaller.OnlyRequired, method, route);

        private async Task RunAsync(AdminCaller caller, string method, string route)
        {
            if (MatrixRows.IsPlaceholder(method))
            {
                MatrixFile.Load().Lines.ShouldNotContain(line => line.Scope == Scope.Administration,
                    "the placeholder row stands in only while no line is an administration endpoint");
                return;
            }

            var api = await host.GetAsync();
            var line = MatrixFile.Load().Lines.Single(line => line.Method == method && line.Route == route);
            line.NonAdmin.ShouldBe(Mark.Refused);
            var permission = line.Permission.ShouldNotBeNull();
            var endpoint = api.Discovery().Actions.Single(action => action.Method == method && action.Route == route);

            var user = caller switch
            {
                AdminCaller.PlainUser => api.NonMember,
                AdminCaller.AllButRequired => await api.UserWithPermissionsAsync(
                    [.. Harmony.Shared.Constants.Permission.Permissions.GetRegisteredPermissions().Where(candidate => candidate != permission)]),
                _ => await api.UserWithPermissionsAsync(permission),
            };

            using var request = RequestBuilder.Anonymous(endpoint);
            var response = await RowRunner.SendAsync(api, endpoint, request, api.Tokens.For(user), TestContext.Current.CancellationToken);

            var verdict = RowVerdict.JudgeAdmin(caller, response, MatrixRows.AdminDisplayName(caller, method, route), permission);
            verdict.Passed.ShouldBeTrue(verdict.Message);
        }
    }
}
