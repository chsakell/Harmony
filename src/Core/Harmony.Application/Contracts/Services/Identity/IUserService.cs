using Harmony.Application.DTO;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;

namespace Harmony.Application.Contracts.Services.Identity
{
    /// <summary>
    /// Service to access Users
    /// </summary>
    public interface IUserService
    {
        Task<IResult<bool>> UpdateUserProfilePicture(string userId, string profilePicture);
        Task<Result<List<UserResponse>>> GetAllAsync();
        Task<Result<List<UserResponse>>> Search(string term);
        Task<Result<List<UserResponse>>> Search(string term, int pageNumber, int pageSize);
        Task<Result<List<UserResponse>>> GetAllAsync(IEnumerable<string> ids);
        Task<int> GetCountAsync();

        Task<IResult<UserResponse>> GetAsync(string userId);
        Task<IResult<UserPublicInfo>> GetPublicInfoAsync(string userId);

        Task<IResult> RegisterAsync(RegisterRequest request, string origin);

        Task<IResult<UserRolesResponse>> GetRolesAsync(string id);

        Task<IResult> UpdateRolesAsync(UpdateUserRolesRequest request, string userId);

        Task<IResult<string>> ConfirmEmailAsync(string userId, string code);

        Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, string origin);

        Task<IResult> ResetPasswordAsync(ResetPasswordRequest request);

        Task<IResult<List<Workspace>>> GetAccessWorkspacesAsync(string userId);

    }
}
