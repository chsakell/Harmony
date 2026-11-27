using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Harmony.Shared.Constants.Permission;
using Microsoft.IdentityModel.Tokens;

namespace Harmony.Api.IntegrationTests.Harness
{
    /// <summary>
    /// Issues tokens the way <c>IdentityService.GenerateEncryptedToken</c> does (FR-007): the same claim types, the
    /// permission claims of the user's roles (FR-140), HMAC SHA-256, and a key that is the UTF-8 bytes of the secret.
    /// <see cref="For"/> signs with the run's secret.
    /// </summary>
    public sealed class TestTokens(string secret)
    {
        /// <param name="expires">When the token expires (UTC); two hours from now by default. The Api allows no clock skew.</param>
        public string For(TestUser user, DateTime? expires = null) => ForKey(user, secret, expires);

        /// <summary>A token for <paramref name="user"/> signed with another key, for the negative check.</summary>
        public static string ForKey(TestUser user, string key, DateTime? expires = null)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Name, user.FirstName),
                new(ClaimTypes.Surname, user.LastName),
                new(ClaimTypes.MobilePhone, string.Empty),
            };
            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
            claims.AddRange(user.Permissions.Select(permission => new Claim(ApplicationClaimTypes.Permission, permission)));

            var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(claims: claims, expires: expires ?? DateTime.UtcNow.AddHours(2), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
