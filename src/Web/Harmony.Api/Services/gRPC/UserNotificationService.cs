using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;
using Harmony.Domain.Enums;
using Harmony.Infrastructure.Repositories;

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
    }
}
