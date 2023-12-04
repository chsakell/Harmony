using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using System.ComponentModel.DataAnnotations;

namespace Harmony.Application.Features.Users.Commands.UpdateUserNotifications;

/// <summary>
/// Command for updating user notifications
/// </summary>
public class UpdateUserNotificationsCommand : IRequest<Result<bool>>
{
    public UpdateUserNotificationsCommand(List<NotificationType> notifications)
    {
        Notifications = notifications;
    }

    public List<NotificationType> Notifications { get; set; }
}
