﻿namespace Harmony.Shared.Constants.Application
{
    /// <summary>
    /// Application constants
    /// </summary>
    public static class ApplicationConstants
    {
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
            public const string OnCardTitleChanged = "OnCardTitleChanged";
            public const string OnCardDescriptionChanged = "OnCardDescriptionChanged";
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
    }
}