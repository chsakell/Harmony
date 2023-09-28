using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Repositories
{
    public interface IUserBoardRepository
    {
        /// <summary>
        /// Create a User Board
        /// </summary>
        /// <param name="userBoard"></param>
        /// <returns></returns>
        Task<int> CreateAsync(UserBoard userBoard);
	}
}
