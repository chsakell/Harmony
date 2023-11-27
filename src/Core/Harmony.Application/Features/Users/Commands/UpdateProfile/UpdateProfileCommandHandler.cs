using AutoMapper;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Account;
using Harmony.Application.Requests.Identity;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UpdateProfile
{
    /// <summary>
    /// Handler for updating profile
    /// </summary>
    public class UpdateProfileCommandHandler : IRequestHandler<UpdateProfileCommand, Result<UpdateProfileResponse>>
    {
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;

        public UpdateProfileCommandHandler(IAccountService accountService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UpdateProfileResponse>> Handle(UpdateProfileCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var updateResult = await _accountService
                    .UpdateProfileAsync(new UpdateProfileRequest()
                    {
                        LastName = command.LastName,
                        FirstName = command.FirstName,
                        Email = command.Email,
                        PhoneNumber = command.PhoneNumber
                    }, userId);

            if(updateResult.Succeeded)
            {
                return Result<UpdateProfileResponse>.Success(new UpdateProfileResponse(), "Profile updated");
            }

            return Result<UpdateProfileResponse>.Fail("Fail to upload image");
        }
    }
}
