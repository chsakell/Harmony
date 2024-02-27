namespace Harmony.Shared.Constants.Application
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class ApplicationConstants
    {
        public static class GatewayConstants
        {
            public const string CoreApiPrefix = "core/api";
            public const string AutomationsApiPrefix = "automations/api";
        }

        public static class AppServices
        {
            public const string Api = "Harmony.Api";
            public const string Automations = "Harmony.Automations";
            public const string Notifications = "Harmony.Notifications";
            public const string SignalR = "Harmony.SignalR";
        }

        public static class SignalR
        {
            public const string HubUrl = "/signalRHub";

            public const string ListenForBoardEvents = "ListenForBoardEvents";
            public const string StopListeningForBoardEvents = "StopListeningForBoardEvents";

            // Board events
            public const string OnBoardListAdded = "OnBoardListAdded";
            public const string OnBoardListTitleChanged = "OnBoardListTitleChanged";
            public const string OnBoardListsPositionsChanged = "OnBoardListsPositionsChanged";
            public const string OnBoardListArchived = "OnBoardListArchived";
            public const string OnCardCreated = "OnCardCreated";
            public const string OnCardStatusChanged = "OnCardStatusChanged";
            public const string OnCardTitleChanged = "OnCardTitleChanged";
            public const string OnCardIssueTypeChanged = "OnCardIssueTypeChanged";
            public const string OnCardDescriptionChanged = "OnCardDescriptionChanged";
            public const string OnCardStoryPointsChanged = "OnCardStoryPointsChanged";
            public const string OnCardItemPositionChanged = "OnCardItemPositionChanged";
            public const string OnCardDatesChanged = "OnCardDatesChanged";
            public const string OnCardLabelToggled = "OnCardLabelToggled";
            public const string OnCardMemberAdded = "OnCardMemberAdded";
            public const string OnCardMemberRemoved = "OnCardMemberRemoved";
            public const string OnCardAttachmentAdded = "OnCardAttachmentAdded";
            public const string OnCardAttachmentRemoved = "OnCardAttachmentRemoved";
            public const string OnCardItemAdded = "OnCardItemAdded";
            public const string OnCardItemChecked = "OnCardItemChecked";
            public const string OnCardLabelRemoved = "OnCardLabelRemoved";
            public const string OnCheckListRemoved = "OnCheckListRemoved";
        }

        public static class HarmonyRetryPolicy
        {
            public const string WaitAndRetry = "wait-and-retry";
        }
    }
}