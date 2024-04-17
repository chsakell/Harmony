
using Harmony.Domain.SourceControl;

namespace Harmony.Application.Contracts.Repositories
{
    public interface ISourceControlRepository
    {
        Task CreateRepository(Repository repo);
        Task<Repository> GetRepository(string repositoryId);
        Task<List<Repository>> GetRepositories(List<string> repositories);
        Task CreateBranch(Branch branch);
        Task CreatePush(Push push);
        Task<Branch> GetBranch(string name, string repositoryId);
        Task<List<Branch>> SearchBranches(string term);
        Task<bool> DeleteBranch(string name);
    }
}
