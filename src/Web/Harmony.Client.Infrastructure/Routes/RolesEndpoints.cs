using static Harmony.Shared.Constants.Application.ApplicationConstants;

namespace Harmony.Client.Infrastructure.Routes
{
    public static class RolesEndpoints
    {
        public static string Delete = $"{GatewayConstants.CoreApiPrefix}/identity/role";
        public static string GetAll = $"{GatewayConstants.CoreApiPrefix}/identity/role";
        public static string Save = $"{GatewayConstants.CoreApiPrefix}/identity/role";
        public static string GetPermissions = $"{GatewayConstants.CoreApiPrefix}/identity/role/permissions/";
        public static string UpdatePermissions = $"{GatewayConstants.CoreApiPrefix}/identity/role/permissions/update";
    }
}