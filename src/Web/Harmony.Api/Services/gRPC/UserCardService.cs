using Grpc.Core;
using Harmony.Application.Contracts.Repositories;
using Harmony.Domain.Entities;

namespace Harmony.Api.Services.gRPC
{
    public class UserCardService : Protos.UserCardService.UserCardServiceBase
    {
        private readonly ILogger<UserCardService> _logger;
        private readonly IUserCardRepository _userCardRepository;

        public UserCardService(ILogger<UserCardService> logger, IUserCardRepository userCardRepository)
        {
            _logger = logger;
            _userCardRepository = userCardRepository;
        }

        public override async Task<Protos.UserCardResponse> GetUserCard(Protos.UserCardFilterRequest request,
            ServerCallContext context)
        {
            var userCard = await _userCardRepository
                .GetUserCard(Guid.Parse(request.CardId), request.UserId);

            return MapToProto(userCard);
        }

        private Protos.UserCardResponse MapToProto(UserCard userCard)
        {
            if (userCard == null)
            {
                return new Protos.UserCardResponse()
                {
                    Found = false
                };
            }

            var protoCard = new Protos.UserCard()
            {
                CardId = userCard.CardId.ToString(),
                UserId = userCard.UserId,
            };

            return new Protos.UserCardResponse()
            {
                Found = true,
                UserCard = protoCard
            };
        }
    }
}
