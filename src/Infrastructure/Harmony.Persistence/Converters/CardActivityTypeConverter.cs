using Harmony.Domain.Enums;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Harmony.Persistence.Converters
{
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
