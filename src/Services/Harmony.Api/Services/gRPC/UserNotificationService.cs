using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Enums;

namespace Harmony.Api.Services.gRPC
{
    public class UserNotificationService : Protos.UserNotificationService.UserNotificationServiceBase
    {
        private readonly ILogger<UserNotificationService> _logger;
        private readonly IUserNotificationRepository _userNotificationRepository;

        public UserNotificationService(ILogger<UserNotificationService> logger,
            IUserNotificationRepository userNotificationRepository)
        {
            _logger = logger;
            _userNotificationRepository = userNotificationRepository;
        }

        public async override Task<Protos.GetUsersForNotificationTypeResponse>
            GetUsersForNotificationType(Protos.GetUsersForNotificationTypeRequest request, ServerCallContext context)
        {
            var registeredUsers = await _userNotificationRepository
                .GetUsersForType(request.Users.ToList(), (EmailNotificationType)request.Type);

            var response = new Protos.GetUsersForNotificationTypeResponse { };
            response.Users.AddRange(registeredUsers);

            return response;
        }

        public override async Task<Protos.UserIsRegisterForNotificationResponse> UserIsRegisterForNotification(Protos.UserIsRegisterForNotificationRequest request, ServerCallContext context)
        {
            var notificationRegistration = await _userNotificationRepository
                .GetForUser(request.UserId, (EmailNotificationType)request.Type);

            return new Protos.UserIsRegisterForNotificationResponse()
            {
                 IsRegistered = notificationRegistration != null
            };
        }
    }
}
