using Harmony.Application.SourceControl.DTO;
using Harmony.Client.Infrastructure.Extensions;
using Harmony.Shared.Wrapper;

namespace Harmony.Client.Infrastructure.Managers.SourceControl
{
    public interface IRepositoryManager : IManager
    {
        Task<IResult<List<BranchDto>>> GetCardBranches(string serialKey);
    }
    public class RepositoryManager : IRepositoryManager
    {
        private readonly HttpClient _httpClient;

        public RepositoryManager(HttpClient client)
        {
            _httpClient = client;
        }

        public async Task<IResult<List<BranchDto>>> GetCardBranches(string serialKey)
        {
            var response = await _httpClient
                .GetAsync(Routes.SourceControlEndpoints.Branches(serialKey));

            return await response.ToResult<List<BranchDto>>();
        }
    }
}
