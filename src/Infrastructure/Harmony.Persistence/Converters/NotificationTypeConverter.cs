using Harmony.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Harmony.Persistence.Converters
{
    /// <summary>
    /// Custom converter for notification types
    /// </summary>
    public class NotificationTypeConverter : ValueConverter<EmailNotificationType, string>
    {
        public NotificationTypeConverter() : base(value => MapActivityTypeToString(value),
            value => MapStringToActivityType(value))
        {

        }

        private static string MapActivityTypeToString(EmailNotificationType value)
        {
            return value.ToString();
        }

        private static EmailNotificationType MapStringToActivityType(string value)
        {
            return Enum.Parse<EmailNotificationType>(value);
        }
    }
}
