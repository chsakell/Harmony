using AutoMapper;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Account;
using Harmony.Application.Requests.Identity;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UpdatePassword
{
    /// <summary>
    /// Handler for updating password
    /// </summary>
    public class UpdatePasswordCommandHandler : IRequestHandler<UpdatePasswordCommand, Result<UpdatePasswordResponse>>
    {
        private readonly IAccountService _accountService;
        private readonly ICurrentUserService _currentUserService;

        public UpdatePasswordCommandHandler(IAccountService accountService,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _accountService = accountService;
            _currentUserService = currentUserService;
        }

        public async Task<Result<UpdatePasswordResponse>> Handle(UpdatePasswordCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var updateResult = await _accountService
                    .ChangePasswordAsync(new ChangePasswordRequest()
                    {
                        Password = command.Password,
                        NewPassword = command.NewPassword,
                        ConfirmNewPassword = command.ConfirmNewPassword,
                    }, userId);

            if(updateResult.Succeeded)
            {
                return Result<UpdatePasswordResponse>.Success(new UpdatePasswordResponse(), "Password updated");
            }

            return Result<UpdatePasswordResponse>.Fail(updateResult.Messages);
        }
    }
}
