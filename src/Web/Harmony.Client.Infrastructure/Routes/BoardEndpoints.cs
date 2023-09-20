namespace Harmony.Client.Infrastructure.Routes
{
    public static class CardEndpoints
    {
        public static string Index = "api/cards";

        public static string Move(Guid cardId) => $"{Index}/{cardId}/move/";
	}
}