using Harmony.Domain.Enums;
using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class SprintEndpoints
    {
        public static string Index = $"{GatewayConstants.CoreApiPrefix}/sprints";

        public static string Reports(Guid sprintId) => $"{Index}/{sprintId}/reports/";

        public static string Cards(Guid sprintId, int pageNumber, int pageSize, 
            string searchTerm, string[] orderBy, CardStatus? status)
        {
            var url = $"{Index}/{sprintId}/cards/?pageNumber={pageNumber}" +
                $"&pageSize={pageSize}&searchTerm={searchTerm}" + (status == null ? string.Empty : $"&status={status}") +
                $"&orderBy=";

            if (orderBy?.Any() == true)
            {
                foreach (var orderByPart in orderBy)
                {
                    url += $"{orderByPart},";
                }
                url = url[..^1];
            }
            return url;
        }

        public static string Start(Guid sprintId) => $"{Index}/{sprintId}/start/";

        public static string Complete(Guid sprintId) => $"{Index}/{sprintId}/complete/";
        public static string CreateIssue(Guid sprintId) => $"{Index}/{sprintId}/cards/";
    }
}