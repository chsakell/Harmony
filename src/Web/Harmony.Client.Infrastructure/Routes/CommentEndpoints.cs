using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class CommentEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/comments";

        public static string GetCard(Guid cardId) => $"{Index}/{cardId}/";
        public static string GetComment(Guid commentId) => $"{Index}/{commentId}/";
    }
}