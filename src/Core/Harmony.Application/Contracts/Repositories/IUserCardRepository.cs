using Harmony.Application.Features.Boards.Queries.GetBoardUsers;
using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IUserCardRepository
    {
        Task<UserCard?> GetUserCard(Guid cardId, string userId);
        Task<int> CreateAsync(UserCard Board);

        Task<int> CountCardUsers(Guid cardId);

        Task<List<CardMemberResponse>> GetCardUsers(Guid cardId);

        Task<CardMemberResponse?> GetBoardAccessMember(Guid cardId, string userId);

        Task<int> Delete(UserCard userBoard);
    }
}
