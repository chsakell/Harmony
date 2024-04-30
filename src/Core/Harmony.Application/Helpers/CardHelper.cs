using System.Text.RegularExpressions;

namespace Harmony.Application.Helpers
{
    /// <summary>
    /// Helper functions for cards
    /// </summary>
    public class CardHelper
    {
        private static char[] SERIAL_KEY_ALLOWED_NEIGHBOUR_CHARS = [' ', '_', '-', '/'];

        public static string DisplayDates(DateTime? startDate, DateTime? dueDate)
        {
            var result = string.Empty;

            if (startDate.HasValue && dueDate.HasValue)
            {
                if (startDate.Value.Month == dueDate.Value.Month)
                {
                    return $"{startDate.Value.ToString("MMM")} {startDate.Value.ToString("dd")} - {dueDate.Value.ToString("dd")}";
                }


                return $"{startDate.Value.ToString("MMM dd")} - {dueDate.Value.ToString("MMM dd")}";
            }
            else if (startDate.HasValue)
            {
                return $"{startDate.Value.ToString("MMM dd")}";
            }
            else if (dueDate.HasValue)
            {
                return $"{dueDate.Value.ToString("MMM dd")}";
            }

            return result;
        }

        public static string GetSerialKey(string text)
        {
            List<string> matches = Regex.Matches(text, @"([a-zA-Z]{3,5})-(\d+)", RegexOptions.IgnoreCase)
                       .Cast<Match>()
                       .Select(x => x.Value).ToList();

            foreach (var match in matches)
            {
                var matchIndex = text.IndexOf(match, StringComparison.InvariantCultureIgnoreCase);

                if (matchIndex == 0 && NextCharAllowed(text, match, matchIndex))
                {
                    return match;
                }
                else if (matchIndex > 0)
                {
                    var previousChar = text[matchIndex - 1];
                    if (PreviewCharAllowed(text, matchIndex) && NextCharAllowed(text, match, matchIndex))
                    {
                        return match;
                    }
                }
            }

            return null;
        }

        private static bool PreviewCharAllowed(string original, int termIndex)
        {
            var previousChar = original[termIndex - 1];

            if (SERIAL_KEY_ALLOWED_NEIGHBOUR_CHARS.Contains(previousChar))
            {
                return true;
            }

            return false;
        }

        private static bool NextCharAllowed(string original, string term, int termIndex)
        {
            if (original.Length == term.Length + termIndex)
            {
                return true;
            }

            var nextChar = original[termIndex + term.Length];

            if (SERIAL_KEY_ALLOWED_NEIGHBOUR_CHARS.Contains(nextChar))
            {
                return true;
            }

            return false;
        }
    }
}
