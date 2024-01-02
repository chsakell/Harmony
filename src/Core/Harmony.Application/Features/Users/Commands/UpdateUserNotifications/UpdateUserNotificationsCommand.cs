using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UpdateUserNotifications;

/// <summary>
/// Command for updating user notifications
/// </summary>
public class UpdateUserNotificationsCommand : IRequest<Result<bool>>
{
    public UpdateUserNotificationsCommand(List<EmailNotificationType> notifications)
    {
        Notifications = notifications;
    }

    public List<EmailNotificationType> Notifications { get; set; }
}
