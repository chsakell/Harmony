namespace Harmony.Client.Infrastructure.Routes
{
    public static class CommentEndpoints
    {
        public static string Index = "api/comments";

        public static string GetCard(Guid cardId) => $"{Index}/{cardId}/";
    }
}