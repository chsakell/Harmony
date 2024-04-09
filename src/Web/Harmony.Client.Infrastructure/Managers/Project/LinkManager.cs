using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.Project
{
    public interface ILinkManager : IManager
    {
        Task<IResult<List<Guid>>> DeleteLink(Guid boardId, Guid linkId);
    }

    public class LinkManager : ILinkManager
    {
        private readonly HttpClient _httpClient;

        public LinkManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<List<Guid>>> DeleteLink(Guid boardId, Guid linkId)
        {
            var response = await _httpClient
                .DeleteAsync(Routes.LinkEndpoints.Link(linkId, boardId));

            return await response.ToResult<List<Guid>>();
        }
    }
}
