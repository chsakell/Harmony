using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Users.Commands.UpdatePassword;

/// <summary>
/// Command for updating password
/// </summary>
public class UpdatePasswordCommand : IRequest<Result<UpdatePasswordResponse>>
{
    [Required]
    public string Password { get; set; }

    [Required]
    public string NewPassword { get; set; }

    [Required]
    public string ConfirmNewPassword { get; set; }
}
