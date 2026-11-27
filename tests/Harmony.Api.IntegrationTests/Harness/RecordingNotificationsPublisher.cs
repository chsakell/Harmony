using System.Collections.Concurrent;
using Harmony.Application.Contracts.Messaging;
using Harmony.Application.Notifications.Email;
using Harmony.Application.Notifications.SearchIndex;
using Harmony.Domain.Enums;

namespace Harmony.Api.IntegrationTests.Harness
{
    public enum PublishedKind
    {
        Message,
        Email,
        SearchIndex,
    }

    /// <param name="Kind">Which Publish method was called.</param>
    /// <param name="Payload">The message or notification.</param>
    /// <param name="Detail">The routing key (Message) or the index name (SearchIndex); empty for Email.</param>
    /// <param name="Type">The notification type (Message only).</param>
    public sealed record PublishedMessage(PublishedKind Kind, object? Payload, string Detail, NotificationType? Type = null);

    /// <summary>
    /// Stands in for the RabbitMQ publisher (FR-006): records every call in memory, thread-safe, and sends nothing.
    /// </summary>
    public sealed class RecordingNotificationsPublisher : INotificationsPublisher
    {
        private readonly ConcurrentQueue<PublishedMessage> published = new();

        public IReadOnlyCollection<PublishedMessage> Published => published.ToArray();

        public Task PublishMessageAsync<T>(T message, NotificationType type, string routingKey)
        {
            published.Enqueue(new PublishedMessage(PublishedKind.Message, message, routingKey, type));
            return Task.CompletedTask;
        }

        public Task PublishEmailNotificationAsync<T>(T notification)
            where T : BaseEmailNotification
        {
            published.Enqueue(new PublishedMessage(PublishedKind.Email, notification, string.Empty));
            return Task.CompletedTask;
        }

        public Task PublishSearchIndexNotificationAsync<T>(T notification, string index)
            where T : BaseSearchIndexNotification
        {
            published.Enqueue(new PublishedMessage(PublishedKind.SearchIndex, notification, index));
            return Task.CompletedTask;
        }
    }
}
