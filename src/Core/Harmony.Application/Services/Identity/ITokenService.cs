

using Harmony.Application.Identity;
using Harmony.Shared.Wrapper;

namespace Harmony.Application.Services.Identity
{
    public interface ITokenService 
    {
        Task<Result<TokenResponse>> LoginAsync(TokenRequest model);

        Task<Result<TokenResponse>> GetRefreshTokenAsync(RefreshTokenRequest model);
    }
}