using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public class LinkEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/links";

        public static string Link(Guid linkId, Guid boardId) => $"{Index}/{linkId}/?boardId={boardId}";
    }
}
