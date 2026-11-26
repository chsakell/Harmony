using System.Text;
using AutoMapper;

namespace Harmony.Application.UnitTests.Mapping
{
    /// <summary>
    /// Ratchet for AutoMapper destination members that no rule fills. The known ones are listed in
    /// <see cref="UnmappedMemberBaseline"/>; a new one fails, and so does a listed one that is now mapped.
    /// </summary>
    public static class UnmappedMemberGuard
    {
        public static UnmappedMemberComparison Compare(
            IEnumerable<(string TypeMap, string Member)> actual,
            IEnumerable<(string TypeMap, string Member)> baseline)
        {
            var actualSet = actual.ToHashSet();
            var baselineSet = baseline.ToHashSet();

            return new UnmappedMemberComparison(
                NewUnmapped: Sorted(actualSet.Except(baselineSet)),
                NowMapped: Sorted(baselineSet.Except(actualSet)));
        }

        /// <summary>
        /// The unmapped destination members that <see cref="MapperConfiguration.AssertConfigurationIsValid()"/>
        /// reports, as <c>("Source -> Destination", member)</c> pairs.
        /// </summary>
        public static IReadOnlyList<(string TypeMap, string Member)> ReadUnmappedMembers(MapperConfiguration configuration)
        {
            try
            {
                configuration.AssertConfigurationIsValid();
                return [];
            }
            catch (AutoMapperConfigurationException exception) when (exception.Errors is not null)
            {
                return exception.Errors
                    .SelectMany(error => error.UnmappedPropertyNames.Select(member =>
                        ($"{error.TypeMap.SourceType.Name} -> {error.TypeMap.DestinationType.Name}", member)))
                    .ToList();
            }
        }

        private static List<(string TypeMap, string Member)> Sorted(IEnumerable<(string TypeMap, string Member)> entries) =>
            entries.OrderBy(entry => entry.TypeMap, StringComparer.Ordinal)
                .ThenBy(entry => entry.Member, StringComparer.Ordinal)
                .ToList();
    }

    public sealed record UnmappedMemberComparison(
        IReadOnlyList<(string TypeMap, string Member)> NewUnmapped,
        IReadOnlyList<(string TypeMap, string Member)> NowMapped)
    {
        public bool MatchesBaseline => NewUnmapped.Count == 0 && NowMapped.Count == 0;

        public string Message
        {
            get
            {
                var message = new StringBuilder();

                if (NewUnmapped.Count > 0)
                {
                    message.AppendLine("New unmapped destination members. Map them, or Ignore() them in the profile:");
                    AppendEntries(message, NewUnmapped);
                }

                if (NowMapped.Count > 0)
                {
                    message.AppendLine("These members are mapped now. Remove them from UnmappedMemberBaseline:");
                    AppendEntries(message, NowMapped);
                }

                return message.ToString();
            }
        }

        private static void AppendEntries(StringBuilder message, IEnumerable<(string TypeMap, string Member)> entries)
        {
            foreach (var (typeMap, member) in entries)
            {
                message.AppendLine($"  {typeMap}: {member}");
            }
        }
    }
}
