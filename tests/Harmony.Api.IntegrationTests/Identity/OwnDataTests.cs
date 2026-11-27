using System.Net.Http.Json;
using Harmony.Api.IntegrationTests.Authorization;
using Harmony.Api.IntegrationTests.Harness;
using Harmony.Application.Features.Users.Commands.UpdateUserNotifications;
using Harmony.Application.Responses;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Identity
{
    /// <summary>
    /// A user reads only their own account details and notification settings, identified by the login and never by an
    /// ID in the request (US3; ADR 2026-11-27-api-authorization-model, rule 4).
    /// </summary>
    public class OwnDataTests(ApiTestHost host)
    {
        [Fact]
        [Trait("Requirement", "FR-120")]
        public async Task Account_LoggedInUser_ReturnsOwnDetailsOnly()
        {
            var api = await host.GetAsync();
            var a = await TestUsers.CreateAsync(api.Services, "owna");
            var b = await TestUsers.CreateAsync(api.Services, "ownb");
            using var client = api.CreateClient(api.Tokens.For(a));

            using var response = await client.GetAsync("api/identity/account", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            var body = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);
            var result = await response.Content.ReadFromJsonAsync<Result<UserResponse>>(TestContext.Current.CancellationToken);
            result.ShouldNotBeNull().Succeeded.ShouldBeTrue(string.Join("; ", result.Messages));
            result.Data.Id.ShouldBe(a.Id);
            result.Data.Email.ShouldBe(a.Email);
            result.Data.FirstName.ShouldBe(a.FirstName);
            result.Data.LastName.ShouldBe(a.LastName);
            ShouldHoldNoneOf(body, b);
        }

        [Fact]
        [Trait("Requirement", "FR-121")]
        public async Task Notifications_LoggedInUser_ReturnsOwnSettingsOnly()
        {
            var api = await host.GetAsync();
            var a = await TestUsers.CreateAsync(api.Services, "nota");
            var b = await TestUsers.CreateAsync(api.Services, "notb");
            using var clientA = api.CreateClient(api.Tokens.For(a));
            using var clientB = api.CreateClient(api.Tokens.For(b));
            await SetNotificationsAsync(clientA, [EmailNotificationType.CardCompleted]);
            await SetNotificationsAsync(clientB, [EmailNotificationType.CommentAddedToCard, EmailNotificationType.MemberAddedToBoard]);

            using var response = await clientA.GetAsync("api/UserNotifications", TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            var result = await response.Content.ReadFromJsonAsync<Result<List<EmailNotificationType>>>(TestContext.Current.CancellationToken);
            result.ShouldNotBeNull().Succeeded.ShouldBeTrue(string.Join("; ", result.Messages));
            result.Data.ShouldBe([EmailNotificationType.CardCompleted]);
        }

        /// <summary>The removed read-by-ID routes match no endpoint, so a logged-in caller gets 404 (FR-109).</summary>
        [Theory]
        [Trait("Requirement", "FR-122")]
        [InlineData("api/identity/user/{0}")]
        [InlineData("api/UserNotifications/{0}")]
        public async Task ReadById_OtherUsersId_Returns404WithoutTheirData(string route)
        {
            var api = await host.GetAsync();
            var a = await TestUsers.CreateAsync(api.Services, "byida");
            var b = await TestUsers.CreateAsync(api.Services, "byidb");
            using var client = api.CreateClient(api.Tokens.For(a));

            using var response = await client.GetAsync(string.Format(System.Globalization.CultureInfo.InvariantCulture, route, b.Id),
                TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(404);
            ShouldHoldNoneOf(await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken), b);
        }

        [Fact]
        [Trait("Requirement", "FR-122")]
        public void MatrixFile_HasNoUserScopeLine()
        {
            MatrixFile.Load().Lines.ShouldNotContain(line => line.Scope == Scope.User,
                "an endpoint that reads another user's data by ID is administration or resource, or takes the user from the login");
        }

        private static async Task SetNotificationsAsync(HttpClient client, List<EmailNotificationType> notifications)
        {
            using var response = await client.PostAsJsonAsync("api/UserNotifications", new UpdateUserNotificationsCommand(notifications),
                TestContext.Current.CancellationToken);
            ((int)response.StatusCode).ShouldBe(200);
        }

        /// <summary>The user name, email and last name of a harness user each carry a unique suffix.</summary>
        private static void ShouldHoldNoneOf(string body, TestUser other)
        {
            body.ShouldNotContain(other.Id);
            body.ShouldNotContain(other.Email, Case.Insensitive);
            body.ShouldNotContain(other.LastName);
        }
    }
}
