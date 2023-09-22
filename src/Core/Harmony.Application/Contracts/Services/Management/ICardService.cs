using Harmony.Domain.Entities;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface ICardService
	{
		Task<bool> PositionCard(Card card, Guid listId, byte position);
	}
}
