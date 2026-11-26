using System.Text.Json.Nodes;
using Harmony.Api.IntegrationTests.Harness;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Authorization
{
    /// <summary>
    /// The non-member rule's request puts the fixture's IDs wherever the name map or the line's bindings assign a
    /// resource kind (FR-031). The fixtures here are only IDs: nothing is written to the database.
    /// </summary>
    [Trait("Requirement", "FR-031")]
    public class RequestBuilderTests(ApiTestHost host)
    {
        private static readonly Fixture Ids = new(
            "owner-id", Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(),
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

        private static readonly TestUser Caller = new("caller-id", "caller", "caller@matrix.test", "caller", "x", ["Basic"]);

        [Fact]
        public async Task UpdateCardTitle_Fixture_RouteIdAndBodyCardIdAreCardAndBoardIdIsBoard()
        {
            var (endpoint, line) = await LiveAsync("PUT", "api/Cards/{id:guid}/title", "id=card");

            using var request = RequestBuilder.ForCaller(endpoint, line, Ids, Caller);

            request.RequestUri!.OriginalString.ShouldBe($"api/Cards/{Ids.CardId}/title");
            var body = await BodyAsync(request);
            body["CardId"]!.GetValue<string>().ShouldBe(Ids.CardId.ToString());
            body["BoardId"]!.GetValue<string>().ShouldBe(Ids.BoardId.ToString());
            body["Title"]!.GetValue<string>().ShouldBe(RequestBuilder.TextPlaceholder);
        }

        [Fact]
        public async Task NestedAndPluralIds_GetFixtureIds()
        {
            var (createCard, createCardLine) = await LiveAsync("POST", "api/Cards", "IssueType.Id=issuetype");
            var (loadBoard, loadBoardLine) = await LiveAsync("POST", "api/Board/{id:guid}/load", "id=board");

            using var create = RequestBuilder.ForCaller(createCard, createCardLine, Ids, Caller);
            using var load = RequestBuilder.ForCaller(loadBoard, loadBoardLine, Ids, Caller);

            var createBody = await BodyAsync(create);
            createBody["IssueType"]!["Id"]!.GetValue<string>().ShouldBe(Ids.IssueTypeId.ToString());
            createBody["ListId"]!.GetValue<string>().ShouldBe(Ids.ListId.ToString());
            createBody["SprintId"]!.GetValue<string>().ShouldBe(Ids.SprintId.ToString());
            var loadBody = await BodyAsync(load);
            loadBody["IssueTypeIds"]!.AsArray().Select(id => id!.GetValue<string>()).ShouldBe([Ids.IssueTypeId.ToString()]);
            loadBody["ListIds"]!.AsArray().Select(id => id!.GetValue<string>()).ShouldBe([Ids.ListId.ToString()]);
            loadBody["Assignees"]!.AsArray().ShouldBeEmpty();
            loadBody["MaxCardsPerList"]!.GetValue<int>().ShouldBe(0);
        }

        [Fact]
        public async Task StringWorkspaceId_GetsIdText()
        {
            var (endpoint, line) = await LiveAsync("POST", "api/Board", string.Empty);

            using var request = RequestBuilder.ForCaller(endpoint, line, Ids, Caller);

            (await BodyAsync(request))["WorkspaceId"]!.GetValue<string>().ShouldBe(Ids.WorkspaceId.ToString());
        }

        [Fact]
        public async Task QueryIdAndUserId_GetFixtureAndCallerIds()
        {
            var (endpoint, line) = await LiveAsync("DELETE", "api/CheckLists/{id:guid}", "id=checklist");

            using var request = RequestBuilder.ForCaller(endpoint, line, Ids, Caller);

            request.RequestUri!.OriginalString.ShouldBe($"api/CheckLists/{Ids.ChecklistId}?boardId={Ids.BoardId}");
            request.Content.ShouldBeNull();
        }

        [Fact]
        public async Task FixedBody_KeepsValuesAndReplacesIds()
        {
            var endpoint = Synthetic(typeof(FooCommand));
            var line = Line("POST", "api/Foo", "FooId=label; body={\"Name\":\"Kept\",\"BoardId\":\"00000000-0000-0000-0000-000000000000\",\"UserId\":\"x\"}");

            using var request = RequestBuilder.ForCaller(endpoint, line, Ids, Caller);

            var body = await BodyAsync(request);
            body["Name"]!.GetValue<string>().ShouldBe("Kept");
            body["BoardId"]!.GetValue<string>().ShouldBe(Ids.BoardId.ToString());
            body["UserId"]!.GetValue<string>().ShouldBe(Caller.Id);
            body["FooId"].ShouldBeNull("a fixed body is sent as written, with only its ID properties replaced");
        }

        [Fact]
        public void BodyPropertyWithoutKind_FailsNamingIt()
        {
            var endpoint = Synthetic(typeof(FooCommand));

            var exception = Should.Throw<InvalidOperationException>(() => RequestBuilder.ForCaller(endpoint, Line("POST", "api/Foo", string.Empty), Ids, Caller));

            exception.Message.ShouldContain("FooId");
            exception.Message.ShouldContain("POST api/Foo");
        }

        [Fact]
        public void RouteIdWithoutKind_FailsNamingIt()
        {
            var endpoint = Synthetic(bodyType: null, route: "api/Foo/{id:guid}");

            var exception = Should.Throw<InvalidOperationException>(() => RequestBuilder.ForCaller(endpoint, Line("POST", "api/Foo/{id:guid}", string.Empty), Ids, Caller));

            exception.Message.ShouldContain("'id'");
        }

        private async Task<(DiscoveredEndpoint Endpoint, MatrixLine Line)> LiveAsync(string method, string route, string bindings)
        {
            var api = await host.GetAsync();
            var endpoint = api.Discovery().Actions.Single(action => action.Method == method && action.Route == route);
            return (endpoint, Line(method, route, bindings));
        }

        private static async Task<JsonNode> BodyAsync(HttpRequestMessage request) =>
            JsonNode.Parse(await request.Content!.ReadAsStringAsync(TestContext.Current.CancellationToken))!;

        private static MatrixLine Line(string method, string route, string bindings) =>
            MatrixFile.Parse(string.Join('\n',
            [
                "- Anonymous known gaps: 1",
                "- Non-member known gaps: 1",
                MatrixFile.Header,
                "|---|---|---|---|---|---|",
                $"| {route} | {method} | resource | gap R-07 | gap R-08 | {bindings} |",
            ])).Lines.Single();

        private static DiscoveredEndpoint Synthetic(Type? bodyType, string route = "api/Foo")
        {
            var action = new ControllerActionDescriptor { DisplayName = "FooController.Post" };
            var description = new ApiDescription { HttpMethod = "POST", RelativePath = route, ActionDescriptor = action };
            if (bodyType is not null)
            {
                description.ParameterDescriptions.Add(new ApiParameterDescription { Name = "command", Source = BindingSource.Body, Type = bodyType });
            }

            var endpoint = new RouteEndpoint(_ => Task.CompletedTask, RoutePatternFactory.Parse(route), 0, new EndpointMetadataCollection(action), "FooController.Post");
            return new DiscoveredEndpoint("POST", route, AllowAnonymous: false, action, description, endpoint);
        }

        public sealed class FooCommand
        {
            public Guid BoardId { get; set; }

            public Guid FooId { get; set; }

            public string? Name { get; set; }

            public string? UserId { get; set; }
        }
    }
}
