namespace Harmony.Shared.Constants.Application
{
    public static class ApplicationConstants
    {
        public static class SignalR
        {
            public const string HubUrl = "/signalRHub";
            public const string SendRegenerateTokens = "RegenerateTokensAsync";
            public const string ReceiveRegenerateTokens = "RegenerateTokens";

            public const string OnConnect = "OnConnectAsync";
            public const string ConnectUser = "ConnectUser";
            public const string OnDisconnect = "OnDisconnectAsync";
            public const string DisconnectUser = "DisconnectUser";

            public const string ListenForBoardEvents = "ListenForBoardEvents";

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