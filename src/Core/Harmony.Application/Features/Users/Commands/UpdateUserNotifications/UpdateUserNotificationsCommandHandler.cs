using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Features.Users.Commands.UpdateUserNotifications;
using Harmony.Domain.Entities;
using Harmony.Shared.Wrapper;
using MediatR;

namespace Harmony.Application.Features.Users.Commands.UpdatePassword
{
    /// <summary>
    /// Handler for updating user notifications
    /// </summary>
    public class UpdateUserNotificationsCommandHandler : IRequestHandler<UpdateUserNotificationsCommand, Result<bool>>
    {
        private readonly IUserNotificationRepository _userNotificationRepository;
        private readonly ICurrentUserService _currentUserService;

        public UpdateUserNotificationsCommandHandler(IUserNotificationRepository userNotificationRepository,
            ICurrentUserService currentUserService)
        {
            _userNotificationRepository = userNotificationRepository;
            _currentUserService = currentUserService;
        }

        public async Task<Result<bool>> Handle(UpdateUserNotificationsCommand command, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var userNotifications = await _userNotificationRepository.GetAllForUser(userId);

            foreach(var notification in command.Notifications)
            {
                if(userNotifications.Any(un => un.NotificationType == notification))
                {
                    continue;
                }
                else
                {
                    await _userNotificationRepository.AddEntryAsync(new UserNotification()
                    {
                        NotificationType = notification,
                        UserId = userId,
                    });
                }
            }

            foreach(var userNotification in userNotifications)
            {
                if(!command.Notifications
                    .Any(notification => notification == userNotification.NotificationType))
                {
                    _userNotificationRepository.DeleteEntry(userNotification);
                }
            }

            var updateResult = await _userNotificationRepository.Commit();
            
            return Result<bool>.Success(true, "Notifications updated");
        }
    }
}
