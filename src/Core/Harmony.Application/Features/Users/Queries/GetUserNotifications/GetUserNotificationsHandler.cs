using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Features.Users.Queries.GetUserNotifications;
using Harmony.Application.Responses;
using Harmony.Domain.Enums;
using Harmony.Shared.Wrapper;
using MediatR;
using Microsoft.Extensions.Localization;

namespace Harmony.Application.Features.Users.Queries.GetUser
{
    public class GetUserNotificationsHandler : IRequestHandler<GetUserNotificationsQuery, IResult<List<NotificationType>>>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IStringLocalizer<GetUserHandler> _localizer;
        private readonly IUserNotificationRepository _userNotificationRepository;

        public GetUserNotificationsHandler(ICurrentUserService currentUserService,
            IStringLocalizer<GetUserHandler> localizer,
            IUserNotificationRepository userNotificationRepository)
        {
            _currentUserService = currentUserService;
            _localizer = localizer;
            _userNotificationRepository = userNotificationRepository;
        }

        public async Task<IResult<List<NotificationType>>> Handle(GetUserNotificationsQuery request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            if (string.IsNullOrEmpty(userId))
            {

                return await Result<List<NotificationType>>.FailAsync(_localizer["Login required to complete this operator"]);
            }

            var userNotifications = await _userNotificationRepository.GetAllForUser(userId);

            return await Result<List<NotificationType>>.SuccessAsync(userNotifications
                .Select(un => un.NotificationType).ToList());
        }
    }
}
