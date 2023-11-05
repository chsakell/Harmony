using Harmony.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Harmony.Persistence.Converters
{
    /// <summary>
    /// Custom converter for activity types
    /// </summary>
    public class CardActivityTypeConverter : ValueConverter<CardActivityType, string>
    {
        public CardActivityTypeConverter() : base(value => MapActivityTypeToString(value),
            value => MapStringToActivityType(value))
        {

        }

        private static string MapActivityTypeToString(CardActivityType value)
        {
            return value.ToString();
        }

        private static CardActivityType MapStringToActivityType(string value)
        {
            return Enum.Parse<CardActivityType>(value);
        }
    }
}
