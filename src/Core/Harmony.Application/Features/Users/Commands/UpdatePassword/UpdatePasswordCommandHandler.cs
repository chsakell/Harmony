using AutoMapper;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Account;
using Harmony.Application.Contracts.Services.Hubs;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Contracts.Services.Management;
using Harmony.Application.DTO;
using Harmony.Application.Extensions;
using Harmony.Application.Helpers;
using Harmony.Application.Requests.Identity;
using Harmony.Application.Specifications.Cards;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Net.Mail;

namespace Harmony.Application.Features.Users.Commands.UpdatePassword
{
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
