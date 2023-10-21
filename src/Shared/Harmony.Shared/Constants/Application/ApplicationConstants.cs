namespace Harmony.Shared.Constants.Application
{
    public static class ApplicationConstants
    {
        public static class SignalR
        {
            public const string HubUrl = "/signalRHub";

            public const string ListenForBoardEvents = "ListenForBoardEvents";

            public const string OnBoardListAdded = "OnBoardListAdded";
            public const string OnBoardListArchived = "OnBoardListArchived";
            public const string OnCardTitleChanged = "OnCardTitleChanged";
            public const string OnCardDescriptionChanged = "OnCardDescriptionChanged";
            public const string OnCardDatesChanged = "OnCardDatesChanged";
            public const string OnCardLabelToggled = "OnCardLabelToggled";
            public const string OnCardAttachmentAdded = "OnCardAttachmentAdded";
            public const string OnCardItemAdded = "OnCardItemAdded";
            public const string OnCardItemChecked = "OnCardItemChecked";
        }
    }
}