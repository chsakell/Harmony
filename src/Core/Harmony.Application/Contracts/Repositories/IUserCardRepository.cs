using Harmony.Application.Features.Cards.Queries.GetCardMembers;
using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    /// <summary>
    /// Repository to access User Cards
    /// </summary>
    public interface IUserCardRepository
    {
        Task<UserCard?> GetUserCard(Guid cardId, string userId);
        Task<List<string>> GetCardMembers(Guid cardId);
        Task<int> CreateAsync(UserCard Board);

        Task<int> CountCardUsers(Guid cardId);

        Task<List<CardMemberResponse>> GetCardUsers(Guid cardId);

        Task<CardMemberResponse?> GetBoardAccessMember(Guid cardId, string userId);

        Task<int> Delete(UserCard userBoard);
    }
}
