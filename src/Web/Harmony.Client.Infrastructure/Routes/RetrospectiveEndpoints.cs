using Harmony.Domain.Enums;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class RetrospectiveEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/retrospectives";
    }
}