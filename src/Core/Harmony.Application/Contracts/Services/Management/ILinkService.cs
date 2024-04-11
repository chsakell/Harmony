using Harmony.Application.DTO;

namespace Harmony.Application.Contracts.Services.Management
{
    public interface ILinkService
    {
        Task<List<LinkDetailsDto>> GetLinksForCard(Guid cardId);
    }
}
