using Harmony.Application.Constants;
using Harmony.Application.Contracts.Services;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Harmony.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly List<string> TRUSTED_CLIENTS = new List<string>() 
        {
            ServiceConstants.HarmonyAutomations,
            ServiceConstants.HarmonyNotifications 
        };

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            FullName = $"{httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)}  " +
                $"{httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Surname)}";

            UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable().Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList();
            _httpContextAccessor = httpContextAccessor;
        }

        public string UserId { get; }
        public string FullName { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }

        public bool IsTrustedClientRequest
        {
            get
            {
                var trustedClientHeader = _httpContextAccessor.HttpContext.Request
                    .Headers[ServiceConstants.TrustedClientHeader];

                if (!trustedClientHeader.IsNullOrEmpty() && 
                    TRUSTED_CLIENTS.Contains(trustedClientHeader.ToString()))
                {
                    return true;
                }

                return false;
            }
        }

        public string? GetHeader(string name)
        {
            if (!IsTrustedClientRequest)
            {
                throw new UnauthorizedAccessException("");
            }

            var header = _httpContextAccessor?.HttpContext?.Request?.Headers[name];

            if (header.HasValue && !header.Value.IsNullOrEmpty())
            {
                return _httpContextAccessor?.HttpContext?.Request?.Headers["trustedClientId"].ToString();
            }

            return null;
        }
    }
}
