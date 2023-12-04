using Harmony.Application.Features.Sprints.Commands.CompleteSprint;
using Harmony.Application.Features.Sprints.Commands.StartSprint;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface IUserNotificationManager : IManager
    {
        Task<IResult<List<NotificationType>>> GetNotificationsAsync(string userId);
    }
}
