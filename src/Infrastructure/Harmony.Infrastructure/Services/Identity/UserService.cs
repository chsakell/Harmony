using AutoMapper;
using Harmony.Application.Responses;
using Harmony.Shared.Wrapper;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.WebUtilities;
using Harmony.Shared.Constants.Role;
using Harmony.Application.Exceptions;
using Harmony.Persistence.Identity;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Requests.Identity;
using Harmony.Domain.Entities;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Enums;
using Harmony.Application.DTO;

namespace Harmony.Infrastructure.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<HarmonyUser> _userManager;
        private readonly RoleManager<HarmonyRole> _roleManager;
        private readonly IMapper _mapper;
        private readonly IUserNotificationRepository _userNotificationRepository;

        public UserService(
            UserManager<HarmonyUser> userManager,
            IMapper mapper,
            IUserNotificationRepository userNotificationRepository,
            RoleManager<HarmonyRole> roleManager
            )
        {
            _userManager = userManager;
            _mapper = mapper;
            _userNotificationRepository = userNotificationRepository;
            _roleManager = roleManager;
        }

        public async Task<IResult<bool>> UpdateUserProfilePicture(string userId, string profilePicture) 
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
            {
                return await Result<bool>.FailAsync("User Not Found");
            }

            user.ProfilePicture = profilePicture;
            var identityResult = await _userManager.UpdateAsync(user);
            var errors = identityResult.Errors.Select(e => e.Description.ToString()).ToList();
            return identityResult.Succeeded ? await Result<bool>.SuccessAsync(true) : await Result<bool>.FailAsync(errors);
        }

        public async Task<Result<List<UserResponse>>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = _mapper.Map<List<UserResponse>>(users);
            return await Result<List<UserResponse>>.SuccessAsync(result);
        }

        public async Task<Result<List<UserResponse>>> GetAllAsync(IEnumerable<string> ids)
        {
            var users = await _userManager.Users
                .Where(u => ids.Contains(u.Id))
                .ToListAsync();

            var result = _mapper.Map<List<UserResponse>>(users);
            return await Result<List<UserResponse>>.SuccessAsync(result);
        }

        public async Task<Result<List<UserResponse>>> Search(string term, int pageNumber, int pageSize)
        {
            var users = await _userManager.Users
                .Where(u => u.UserName.Contains(term) || u.FirstName.Contains(term) || 
                    u.LastName.Contains(term) || u.Email.Contains(term))
                .Skip((pageNumber - 1) * pageSize).Take(pageSize)
                .ToListAsync();

            var result = _mapper.Map<List<UserResponse>>(users);
            return await Result<List<UserResponse>>.SuccessAsync(result);
        }

        public async Task<Result<List<UserResponse>>> Search(string term)
        {
            var users = await _userManager.Users
                .Where(u => u.UserName.Contains(term) || u.FirstName.Contains(term) ||
                    u.LastName.Contains(term) || u.Email.Contains(term))
                .Take(40)
                .ToListAsync();

            var result = _mapper.Map<List<UserResponse>>(users);
            return await Result<List<UserResponse>>.SuccessAsync(result);
        }

        public async Task<IResult> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                return await Result.FailAsync(string.Format("Username {0} is already taken.", request.UserName));
            }
            var user = new HarmonyUser
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                IsActive = true, //request.ActivateUser,
                EmailConfirmed = true //request.AutoConfirmEmail
            };

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var userWithSamePhoneNumber = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);
                if (userWithSamePhoneNumber != null)
                {
                    return await Result.FailAsync(string.Format("Phone number {0} is already registered.", request.PhoneNumber));
                }
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RoleConstants.BasicRole);

                    foreach(var notificationType in Enum.GetValues<EmailNotificationType>())
                    {
                        await _userNotificationRepository.AddEntryAsync(new UserNotification()
                        {
                            UserId = user.Id,
                            NotificationType = notificationType
                        });
                    }

                    await _userNotificationRepository.Commit();

                    return await Result<string>.SuccessAsync(user.Id, string.Format("User {0} Registered.", user.UserName));
                }
                else
                {
                    return await Result.FailAsync(result.Errors.Select(a => a.Description.ToString()).ToList());
                }
            }
            else
            {
                return await Result.FailAsync(string.Format("Email {0} is already registered.", request.Email));
            }
        }

        private async Task<string> SendVerificationEmail(HarmonyUser user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/identity/user/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            return verificationUri;
        }

        public async Task<IResult<UserResponse>> GetAsync(string userId)
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var result = _mapper.Map<UserResponse>(user);
            return await Result<UserResponse>.SuccessAsync(result);
        }

        public async Task<IResult<UserPublicInfo>> GetPublicInfoAsync(string userId)
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var result = _mapper.Map<UserPublicInfo>(user);
            return await Result<UserPublicInfo>.SuccessAsync(result);
        }

        public async Task<IResult<List<Workspace>>> GetAccessWorkspacesAsync(string userId)
        {
            var user = await _userManager.Users.AsNoTracking()
                .Include(u => u.AccessWorkspaces)
                    .ThenInclude(ac => ac.Workspace)
                .Where(u => u.Id == userId)
                .FirstOrDefaultAsync();

            if(user == null)
            {
                return  await Result<List<Workspace>>.FailAsync("User not found");
            }

            var workspaces = user.AccessWorkspaces.Select(aw => aw.Workspace).ToList();

            return await Result<List<Workspace>>.SuccessAsync(workspaces);
        }

        public async Task<IResult<UserRolesResponse>> GetRolesAsync(string userId)
        {
            var viewModel = new List<UserRoleModel>();
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var userRolesViewModel = new UserRoleModel
                {
                    RoleName = role.Name,
                    RoleDescription = role.Description
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                viewModel.Add(userRolesViewModel);
            }
            var result = new UserRolesResponse { UserRoles = viewModel };
            return await Result<UserRolesResponse>.SuccessAsync(result);
        }

        public async Task<IResult> UpdateRolesAsync(UpdateUserRolesRequest request, string userId)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);

            var roles = await _userManager.GetRolesAsync(user);
            var selectedRoles = request.UserRoles.Where(x => x.Selected).ToList();

            var currentUser = await _userManager.FindByIdAsync(userId);
            if (!await _userManager.IsInRoleAsync(currentUser, RoleConstants.AdministratorRole))
            {
                var tryToAddAdministratorRole = selectedRoles
                    .Any(x => x.RoleName == RoleConstants.AdministratorRole);
                var userHasAdministratorRole = roles.Any(x => x == RoleConstants.AdministratorRole);
                if (tryToAddAdministratorRole && !userHasAdministratorRole || !tryToAddAdministratorRole && userHasAdministratorRole)
                {
                    return await Result.FailAsync("Not Allowed to add or delete Administrator Role if you have not this role.");
                }
            }

            var result = await _userManager.RemoveFromRolesAsync(user, roles);
            result = await _userManager.AddToRolesAsync(user, selectedRoles.Select(y => y.RoleName));
            return await Result.SuccessAsync("Roles Updated");
        }

        public async Task<IResult<string>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return await Result<string>.SuccessAsync(user.Id, string.Format("Account Confirmed for {0}. You can now use the /api/identity/token endpoint to generate JWT.", user.Email));
            }
            else
            {
                throw new ApiException(string.Format("An error occurred while confirming {0}", user.Email));
            }
        }

        public async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                return await Result.FailAsync("An Error has occurred!");
            }
            // For more information on how to enable account confirmation and password reset please
            // visit https://go.microsoft.com/fwlink/?LinkID=532713
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "account/reset-password";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var passwordResetURL = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);

            // TODO: Send email
            //var mailRequest = new MailRequest
            //{
            //    Body = string.Format("Please reset your password by <a href='{0}'>clicking here</a>.", HtmlEncoder.Default.Encode(passwordResetURL)),
            //    Subject = "Reset Password",
            //    To = request.Email
            //};

            //BackgroundJob.Enqueue(() => _mailService.SendAsync(mailRequest));

            return await Result.SuccessAsync("Password Reset Mail has been sent to your authorized Email.");
        }

        public async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return await Result.FailAsync("An Error has occured!");
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (result.Succeeded)
            {
                return await Result.SuccessAsync("Password Reset Successful!");
            }
            else
            {
                return await Result.FailAsync("An Error has occured!");
            }
        }

        public async Task<int> GetCountAsync()
        {
            var count = await _userManager.Users.CountAsync();
            return count;
        }
    }
}
