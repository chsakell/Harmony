using Harmony.Application.Contracts.Services;
using System.Security.Claims;

namespace Harmony.Api.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            FullName = $"{httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name)}  " +
                $"{httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Surname)}";

            UserId = httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            Claims = httpContextAccessor.HttpContext?.User?.Claims.AsEnumerable().Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList();
        }

        public string UserId { get; }
        public string FullName { get; }
        public List<KeyValuePair<string, string>> Claims { get; set; }
    }
}
