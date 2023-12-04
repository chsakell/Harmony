using Harmony.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Harmony.Persistence.Converters
{
    /// <summary>
    /// Custom converter for notification types
    /// </summary>
    public class NotificationTypeConverter : ValueConverter<NotificationType, string>
    {
        public NotificationTypeConverter() : base(value => MapActivityTypeToString(value),
            value => MapStringToActivityType(value))
        {

        }

        private static string MapActivityTypeToString(NotificationType value)
        {
            return value.ToString();
        }

        private static NotificationType MapStringToActivityType(string value)
        {
            return Enum.Parse<NotificationType>(value);
        }
    }
}
