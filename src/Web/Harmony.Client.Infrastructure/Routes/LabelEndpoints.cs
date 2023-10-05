namespace Harmony.Client.Infrastructure.Routes
{
    public static class LabelEndpoints
    {
        public static string Index = "api/labels";

        public static string GetLabel(Guid labelId) => $"{Index}/{labelId}/";

        public static string LabelTitle(Guid labelId) => $"{Index}/{labelId}/title/";
    }
}