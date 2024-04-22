using Harmony.Domain.Enums.Automations;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class SourceControlEndpoints
    {
        public static string Index = $"{GatewayConstants.SourceControlApiPrefix}/activity";

        public static string Branches(string serialKey)
            => $"{Index}/branches?serialKey={serialKey}";

    }
}