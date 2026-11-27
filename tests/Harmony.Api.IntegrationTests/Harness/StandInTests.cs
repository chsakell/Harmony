using System.Net.Http.Json;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Contracts.Services.Search;
using Harmony.Application.Features.Cards.Commands.UploadCardFile;
using Harmony.Application.Notifications.Email;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.IntegrationTests.Harness;
using Harmony.Infrastructure.Services.Search;
using Harmony.Integrations.SourceControl.Protos;
using Harmony.Messaging;
using Harmony.Shared.Wrapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Xunit;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// The test host sends nothing outside the test process and writes files only under the run's folder (FR-006),
    /// and differs from a deployed Api only by the NFR-003 overrides.
    /// </summary>
    public class StandInTests(ApiTestHost host)
    {
        [Fact]
        [Trait("Requirement", "FR-006")]
        public async Task Services_TestHost_PublisherAndSourceControlClientAreStandIns()
        {
            var api = await host.GetAsync();

            api.Services.GetRequiredService<INotificationsPublisher>().ShouldBeSameAs(api.Publisher);
            api.Services.GetRequiredService<SourceControlService.SourceControlServiceClient>().ShouldBeSameAs(api.SourceControlClient);
            api.ServiceDescriptors.ShouldNotContain(descriptor =>
                (descriptor.IsKeyedService ? descriptor.KeyedImplementationType : descriptor.ImplementationType)
                == typeof(RabbitMQNotificationPublisher));
            api.ServiceDescriptors.Count(descriptor => descriptor.ServiceType == typeof(INotificationsPublisher)).ShouldBe(1);
            api.ServiceDescriptors.Count(descriptor => descriptor.ServiceType == typeof(SourceControlService.SourceControlServiceClient)).ShouldBe(1);
        }

        [Fact]
        [Trait("Requirement", "FR-006")]
        public async Task Search_TestHost_DatabaseSearchAndNoAlgoliaClient()
        {
            var api = await host.GetAsync();
            using var scope = api.Services.CreateScope();

            scope.ServiceProvider.GetRequiredService<ISearchService>().ShouldBeOfType<DatabaseSearchService>();
            api.ServiceDescriptors.ShouldNotContain(descriptor =>
                (descriptor.ServiceType.Namespace ?? string.Empty).StartsWith("Algolia", StringComparison.Ordinal));
        }

        [Fact]
        [Trait("Requirement", "NFR-003")]
        public async Task Configuration_TestHost_ElasticAndOtlpEmpty()
        {
            var api = await host.GetAsync();
            var configuration = api.Services.GetRequiredService<IConfiguration>();

            configuration["ElasticConfiguration:Uri"].ShouldBeNullOrEmpty();
            configuration["OTEL_EXPORTER_OTLP_ENDPOINT"].ShouldBeNullOrEmpty();
            configuration["DatabaseProvider"].ShouldBe("PostgreSQL");
            configuration["CacheProvider"].ShouldBe("InMemory");
        }

        /// <summary>
        /// Changing a card's dates publishes an email notification for that card. This replaced the test on
        /// <c>POST api/Test</c>, which #25 removes (spec #25, Flagged concern 6). Matching on the fixture's card keeps it
        /// independent of other tests that publish in parallel.
        /// </summary>
        [Fact]
        [Trait("Requirement", "FR-006")]
        public async Task UpdateCardDates_OwnerPut_RecordsEmailNotification()
        {
            var api = await host.GetAsync();
            var fixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);
            using var client = api.CreateClient(api.Tokens.For(api.Owner));

            using var response = await client.PutAsJsonAsync($"api/Cards/{fixture.CardId}/dates", new
            {
                fixture.BoardId,
                fixture.CardId,
                StartDate = (DateTime?)null,
                DueDate = new DateTime(2026, 12, 31, 12, 0, 0, DateTimeKind.Utc),
                DueDateReminderType = (DueDateReminderType?)null,
            }, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            api.Publisher.Published.ShouldContain(message =>
                message.Kind == PublishedKind.Email && message.Payload is CardDueTimeUpdatedNotification
                && ((CardDueTimeUpdatedNotification)message.Payload).Id == fixture.CardId);
        }

        [Fact]
        [Trait("Requirement", "FR-006")]
        public async Task UploadCardFile_OwnerValidFile_WritesUnderRunFolderOnly()
        {
            var api = await host.GetAsync();
            var fixture = await FixtureFactory.CreateAsync(api.Services, api.Owner);
            var sourceFiles = Path.Combine(RepositoryRoot.Path, "src", "Services", "Harmony.Api", "Files");
            var before = Listing(sourceFiles);
            using var client = api.CreateClient(api.Tokens.For(api.Owner));

            using var response = await client.PostAsJsonAsync("api/files", new UploadCardFileCommand
            {
                BoardId = fixture.BoardId,
                CardId = fixture.CardId,
                FileName = "matrix.txt",
                Extension = ".txt",
                Type = AttachmentType.CardDocument,
                Data = "matrix"u8.ToArray(),
            }, TestContext.Current.CancellationToken);

            ((int)response.StatusCode).ShouldBe(200);
            var result = await response.Content.ReadFromJsonAsync<Result<UploadCardFileResponse>>(TestContext.Current.CancellationToken);
            result.ShouldNotBeNull().Succeeded.ShouldBeTrue(string.Join("; ", result.Messages));
            File.Exists(Path.Combine(api.Folder, "Files", "Cards", "Documents", result.Data.Attachment.FileName)).ShouldBeTrue();
            Listing(sourceFiles).ShouldBe(before);
        }

        private static List<string> Listing(string folder) =>
            Directory.Exists(folder)
                ? Directory.EnumerateFiles(folder, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal).ToList()
                : [];
    }
}
