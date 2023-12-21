﻿using Harmony.Domain.Enums;

namespace Harmony.Application.Notifications.SearchIndex
{
    public abstract class BaseSearchIndexNotification : ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
    }

    public interface ISearchIndexNotification
    {
        public abstract SearchIndexNotificationType Type { get; }
    }
}
