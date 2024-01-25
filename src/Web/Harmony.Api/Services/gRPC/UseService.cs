using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Application.Contracts.Services.Identity;
using Harmony.Application.Responses;
using Harmony.Domain.Entities;

namespace Harmony.Api.Services.gRPC
{
    public class UserService : Protos.UserService.UserServiceBase
    {
        private readonly ILogger<UserCardService> _logger;
        private readonly IUserService _userService;

        public UserService(ILogger<UserCardService> logger, IUserService userService)
        {
            _logger = logger;
            _userService = userService;
        }

        public override async Task<Protos.UserResponse> GetUser(Protos.UserFilterRequest request,
            ServerCallContext context)
        {
            var userResult = await _userService.GetAsync(request.UserId);

            if (!userResult.Succeeded || userResult.Data == null || !userResult.Data.IsActive)
            {
                return new Protos.UserResponse()
                {
                    Found = false
                };
            }

            var user = userResult.Data;

            return MapToProto(user);
        }

        private Protos.UserResponse MapToProto(UserResponse user)
        {
            var proto = new Protos.User()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                UserName = user.UserName
            };

            return new Protos.UserResponse()
            {
                Found = true,
                User = proto
            };
        }
    }
}
