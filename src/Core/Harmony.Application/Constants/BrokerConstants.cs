namespace Harmony.Application.Constants
{
    public class BrokerConstants
    {
        public const string NotificationsExchange = "harmony_notifications";

        public const string EmailNotificationsQueue = "email_notifications";
        public const string SearchIndexNotificationsQueue = "search_index_notifications";
        public const string NotificationHeader = "notification_type";
        public const string IndexNameHeader = "index_name";

        public const string AutomationNotificationsQueue = "notifications.automation";
        public const string SignalrNotificationsQueue = "notifications.signalr";

        public class RoutingKeys
        {
            public const string Notifications = "notifications";
            public const string Automation = "notifications.automation";
            public const string SignalR = "notifications.signalr";
        }
    }
}
